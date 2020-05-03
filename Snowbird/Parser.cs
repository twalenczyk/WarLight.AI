﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Common.Util;

namespace WarLight.Shared.AI.Snowbird
{
    public static class Parser
    {
        public static List<Dictionary<TerritoryIDType, double>> GetStandingArmyMean(MapIDType mapID)
        {
            var comprehensive = new List<Dictionary<TerritoryIDType, List<double>>>();
            var ret = new List<Dictionary<TerritoryIDType, double>>();

            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Maps");
            var fp = Path.Combine(dir, mapID.GetValue().ToString() + ".txt");
            if (File.Exists(fp))
            {
                // Simply parse the relevant file
                var text = File.ReadAllText(Path.Combine(fp));
                text = text.Replace("\n", string.Empty).Replace("\r", string.Empty);
                var check = text.Split('!');
                foreach (var chunk in text.Split('!').Where(s => s != string.Empty))
                {
                    var data = JObject.Parse(chunk);
                    var turnNumber = data["turnNumber"].Value<int>();
                    var borderArmies = data["borderArmies"];

                    // in case turn numbers are unordered/missing
                    for (var i = comprehensive.Count; i <= turnNumber; i++)
                    {
                        ret.Add(new Dictionary<TerritoryIDType, double>());
                    }

                    foreach (var border in borderArmies)
                    {
                        var id = (TerritoryIDType)border["territoryID"].Value<int>();
                        var armies = border["armies"].Value<double>();
                        ret[turnNumber].Add(id, armies);
                    }
                }
            }
            else
            {
                // create the file
                // assume all games are on the right map.
                var gameDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Games");
                var gameFps = Directory.GetFiles(gameDir);
                foreach (var gameFp in gameFps)
                {
                    var text = File.ReadAllText(Path.Combine(gameFp));
                    text = text.Replace("\n", string.Empty).Replace("\r", string.Empty);
                    var check = text.Split('!');
                    foreach (var chunk in text.Split('!').Where(s => s != string.Empty))
                    {
                        var data = JObject.Parse(chunk);
                        var turnNumber = data["turnNumber"].Value<int>();
                        var borderArmies = data["borderArmies"];

                        // in case turn numbers are unordered/missing
                        for (var i = comprehensive.Count; i <= turnNumber; i++)
                        {
                            comprehensive.Add(new Dictionary<TerritoryIDType, List<double>>());
                        }

                        foreach (var border in borderArmies)
                        {
                            var id = (TerritoryIDType)border["territoryID"].Value<int>();
                            var armies = border["armies"].Value<double>();
                            
                            if (!comprehensive[turnNumber].ContainsKey(id))
                            {
                                comprehensive[turnNumber].Add(id, new List<double>());
                            }
                            comprehensive[turnNumber][id].Add(armies);
                        }
                    }
                }

                // store comprehensive as a file for later use
                DataCollector.WriteMapStandingArmyComprehensiveData(comprehensive, mapID);

                // convert to averagess
                foreach (var dict in comprehensive)
                {
                    ret.Add(new Dictionary<TerritoryIDType, double>());
                    foreach (var key in dict.Keys)
                    {
                        var retDict = ret.Last();
                        retDict[key] = dict[key].Average();
                    }
                }

                DataCollector.WriteMapStandingArmyData(ret, mapID);
            }
            

            return ret;
        }

        public static List<Dictionary<TerritoryIDType, List<double>>> GetStandingArmyComprehensive(MapIDType mapID) 
        {
            var ret = new List<Dictionary<TerritoryIDType, List<double>>>();

            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Maps//Comprehensive");
            var fp = Path.Combine(dir, mapID.GetValue().ToString() + ".txt");

            if (File.Exists(fp))
            {
                // Simply parse the relevant file
                var text = File.ReadAllText(Path.Combine(fp));
                text = text.Replace("\n", string.Empty).Replace("\r", string.Empty);
                var check = text.Split('!');
                foreach (var chunk in text.Split('!').Where(s => s != string.Empty))
                {
                    var data = JObject.Parse(chunk);
                    var turnNumber = data["turnNumber"].Value<int>();
                    var borderArmies = data["borderArmies"];

                    // in case turn numbers are unordered/missing
                    for (var i = ret.Count; i <= turnNumber; i++)
                    {
                        ret.Add(new Dictionary<TerritoryIDType, List<double>>());
                    }

                    foreach (var border in borderArmies)
                    {
                        var id = (TerritoryIDType)border["territoryID"].Value<int>();
                        var armies = border["armies"].Value<List<double>>();
                        ret[turnNumber].Add(id, armies);
                    }
                }
            }
            else 
            {
                // create the file from relevant game files on the map
            }

            return ret;
        }
    }
}
