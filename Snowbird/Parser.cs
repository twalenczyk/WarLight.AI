using Newtonsoft.Json.Linq;
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

        /*
         * *************************
         * Attack Deployment Methods
         * *************************
         */
        public static List<Dictionary<TerritoryIDType, double>> GetAttackDeploymentMeans(MapIDType mapID)
        {
            List<Dictionary<TerritoryIDType, double>> ret = null;

            // see if a stored file exists in the consolidated data directory
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Consolidated//Maps//" + mapID.ToString() + "//AttackDeployments");
            var fp = Path.Combine(dir, "means.txt");

            if (File.Exists(fp))
            {
                ret = new List<Dictionary<TerritoryIDType, double>>();

                // simply parse it and return it.
                var text = File.ReadAllText(fp);
                text = text.Replace("\n", string.Empty).Replace("\r", string.Empty);
                var check = text.Split('!');
                foreach (var chunk in text.Split('!').Where(s => s != string.Empty))
                {
                    var data = JObject.Parse(chunk);
                    var turnNumber = data["turnNumber"].Value<int>();
                    var borderArmies = data["deployment"];

                    // in case turn numbers are unordered/missing
                    for (var i = ret.Count; i <= turnNumber; i++)
                    {
                        ret.Add(new Dictionary<TerritoryIDType, double>());
                    }

                    foreach (var border in borderArmies)
                    {
                        var id = (TerritoryIDType)border["territoryID"].Value<int>();
                        var armies = border["armies"].Value<double>();
                        ret[turnNumber][id] = armies;
                    }
                }
            }
            else
            {
                // consolidate the data from the set of current games on file
                var rawData = new List<Dictionary<TerritoryIDType, List<double>>>();
                dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Raw//Maps//" + mapID.ToString() + "//AttackDeployments");

                foreach (var gameFile in Directory.GetFiles(dir))
                {
                    var deploymentDataPerTurn = ParseDeploymentGame(gameFile);

                    for (var i = 0; i < deploymentDataPerTurn.Count; i++)
                    {
                        if (rawData.Count <= i)
                        {
                            rawData.Add(deploymentDataPerTurn[i]);
                        }
                        else
                        {
                            deploymentDataPerTurn[i].ForEach(kvp =>
                            {
                                if (rawData[i].ContainsKey(kvp.Key))
                                {
                                    rawData[i][kvp.Key].AddRange(kvp.Value);
                                }
                                else
                                {
                                    rawData[i][kvp.Key] = kvp.Value;
                                }
                            });
                        }
                    }
                }

                // consider storing the rawData collection somewhere
                ret = rawData.Select(
                    dict => dict.Select(
                            kvp => new KeyValuePair<TerritoryIDType, double>(kvp.Key, kvp.Value.Average()))
                        .ToDictionary(
                            kvp => kvp.Key, kvp => kvp.Value))
                        .ToList();

                // write the data for future use
                DataCollector.WriteMapAttackDeploymentMeansComprehensiveData(rawData, mapID);
                DataCollector.WriteMapAttackDeploymentMeans(ret, mapID);
            }

            return ret;
        }

        /*
         * *************************
         * Defense Deployment Methods
         * *************************
         */

        /// <summary>
        /// If possible, gets the stored means for defense deployments on a map. If a file holding the data does not exist, it
        /// will attempt to create one. If all else fails, it returns null.
        /// </summary>
        /// <param name="mapID"></param>
        /// <returns>Returns a list of means for defense deployments. Indexing into the list gives you a dictionary of territory IDs
        /// that you have stats for at turn. If null is returned, then no files existed for this map.</returns>
        public static List<Dictionary<TerritoryIDType, double>> GetDefenseDeploymentMeans(MapIDType mapID)
        {
            List<Dictionary<TerritoryIDType, double>> ret = null;

            // see if a stored file exists in the consolidated data directory
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Consolidated//Maps//" + mapID.ToString() + "//DefenseDeployments");
            var fp = Path.Combine(dir, "means.txt");

            if (File.Exists(fp))
            {
                ret = new List<Dictionary<TerritoryIDType, double>>();

                // simply parse it and return it.
                var text = File.ReadAllText(fp);
                text = text.Replace("\n", string.Empty).Replace("\r", string.Empty);
                var check = text.Split('!');
                foreach (var chunk in text.Split('!').Where(s => s != string.Empty))
                {
                    var data = JObject.Parse(chunk);
                    var turnNumber = data["turnNumber"].Value<int>();
                    var borderArmies = data["deployment"];

                    // in case turn numbers are unordered/missing
                    for (var i = ret.Count; i <= turnNumber; i++)
                    {
                        ret.Add(new Dictionary<TerritoryIDType, double>());
                    }

                    foreach (var border in borderArmies)
                    {
                        var id = (TerritoryIDType)border["territoryID"].Value<int>();
                        var armies = border["armies"].Value<double>();
                        ret[turnNumber][id] = armies;
                    }
                }
            }
            else
            {
                // consolidate the data from the set of current games on file
                var rawData = new List<Dictionary<TerritoryIDType, List<double>>>();
                dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Raw//Maps//" + mapID.ToString() + "//DefenseDeployments");
                
                foreach (var gameFile in Directory.GetFiles(dir))
                {
                    var deploymentDataPerTurn = ParseDeploymentGame(gameFile);

                    for (var i = 0; i < deploymentDataPerTurn.Count; i++)
                    {
                        if (rawData.Count <= i)
                        {
                            rawData.Add(deploymentDataPerTurn[i]);
                        }
                        else
                        {
                            deploymentDataPerTurn[i].ForEach(kvp =>
                            {
                                if (rawData[i].ContainsKey(kvp.Key))
                                {
                                    rawData[i][kvp.Key].AddRange(kvp.Value);
                                }
                                else
                                {
                                    rawData[i][kvp.Key] = kvp.Value;
                                }
                            });
                        }
                    }
                }

                // consider storing the rawData collection somewhere
                ret = rawData.Select(
                    dict => dict.Select(
                            kvp => new KeyValuePair<TerritoryIDType, double>(kvp.Key, kvp.Value.Average()))
                        .ToDictionary(
                            kvp => kvp.Key, kvp => kvp.Value))
                        .ToList();

                // write the data for future use
                DataCollector.WriteMapDefenseDeploymentMeansComprehensiveData(rawData, mapID);
                DataCollector.WriteMapDefenseDeploymentMeans(ret, mapID);
            }

            return ret;
        }


        /*
         * *************************
         * Standing Army Methods
         * *************************
         */
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
                        var armies = border["armies"].Select(token => token.Value<double>()).ToList();
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

        /*
         * *************************
         * Helper Methods
         * *************************
         */
        private static List<Dictionary<TerritoryIDType, List<double>>> ParseDeploymentGame(string filename)
        {
            var ret = new List<Dictionary<TerritoryIDType, List<double>>>();

            var text = File.ReadAllText(Path.Combine(filename));
            text = text.Replace("\n", string.Empty).Replace("\r", string.Empty);
            var check = text.Split('!');
            foreach (var chunk in text.Split('!').Where(s => s != string.Empty))
            {
                var data = JObject.Parse(chunk);
                var turnNumber = data["turnNumber"].Value<int>();
                var deployment = data["deployment"];

                while (ret.Count <= turnNumber)
                {
                    ret.Add(new Dictionary<TerritoryIDType, List<double>>());
                }

                var terrID = (TerritoryIDType)deployment["territoryID"].Value<int>();
                var armiesDeployed = deployment["armiesDeployed"].Value<int>();

                if (!ret[turnNumber].ContainsKey(terrID))
                {
                    ret[turnNumber][terrID] = new List<double>();
                }

                ret[turnNumber][terrID].Add(armiesDeployed);
            }

            return ret;
        }
    }
}
