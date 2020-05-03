using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WarLight.Shared.AI.Common.Util
{
    /// <summary>
    /// Contains methods for writing data collecting throughout a game.
    /// </summary>
    public static class DataCollector
    {

        public static int currentGameID;
        public static int currentTurnNumber;

        public static void WriteStandingArmyData(IEnumerable<KeyValuePair<TerritoryIDType, double>> armies)
        {
            // create the JSON object for the turn.
            var data = DataCollector.CreateStandingArmyJson(armies, currentTurnNumber);

            // Append data to file
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Games");
            var gamePath = currentGameID.ToString() + ".txt";
            AppendToFile(data.ToString() + '!', dir, gamePath);
        }

        public static void WriteMapStandingArmyData(List<Dictionary<TerritoryIDType, double>> armies, MapIDType mapID)
        {
            // create the JSON object for the turn.
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Maps");
            var armyData = new JArray();
            for (var i = 0; i < armies.Count; i++)
            {
                var dict = armies[i];
                var data = DataCollector.CreateStandingArmyJson(dict.ToList(), i);
                var gamePath = ((int)mapID).ToString() + ".txt";
                AppendToFile(data.ToString() + '!', dir, gamePath);
            }
        }
        
        public static void WriteMapStandingArmyComprehensiveData(List<Dictionary<TerritoryIDType, List<double>>> armies, MapIDType mapID) 
        {
            // create the JSON object for the turn.
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Maps//Comprehensive");
            for (var i = 0; i < armies.Count; i++)
            {
                var dict = armies[i];
                var armyData = new JArray();

                foreach (var kvp in dict)
                {
                    var entry = new JObject();
                    entry["territoryID"] = (int)kvp.Key;
                    entry["armies"] = new JArray(kvp.Value);

                    armyData.Add(entry);
                }

                var data = new JObject();
                data["turnNumber"] = i;
                data["borderArmies"] = armyData;

                var gamePath = ((int)mapID).ToString() + ".txt";
                AppendToFile(data.ToString() + '!', dir, gamePath);
            }
        }

        private static JObject CreateStandingArmyJson(IEnumerable<KeyValuePair<TerritoryIDType, double>> armies, int turnNumber)
        {
            var armyData = new JArray();
            foreach (KeyValuePair<TerritoryIDType, double> kvp in armies)
            {
                var entry = new JObject();
                entry["territoryID"] = (int)kvp.Key;
                entry["armies"] = kvp.Value;
                armyData.Add(entry);
            }

            var data = new JObject();
            data["turnNumber"] = turnNumber;
            data["borderArmies"] = armyData;

            return data;
        }

        private static void AppendToFile(string content, string dir, string filename)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir, filename), content);
        }
    }
}
