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
    /// Contains methods for writing data collected throughout a game.
    /// </summary>
    public static class DataCollector
    {

        public static int currentGameID;
        public static int currentTurnNumber;
        public static int currentMapID;

        public static void WriteStandingArmiesData(IEnumerable<KeyValuePair<TerritoryIDType, double>> armies)
        {
            // create the JSON object for the turn.
            var data = DataCollector.CreateStandingArmyJson(armies, currentTurnNumber);

            // Append data to file
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Raw//Maps//" + currentMapID.ToString() + "//StandingArmies");
            var gamePath = currentGameID.ToString() + ".txt";
            AppendToFile(data.ToString() + '!', dir, gamePath);
        }

        

        public static void WriteGameAttackDeploymentData(TerritoryIDType terrID, int armiesDeployed, MapIDType mapID)
        {
            // get map id somehow
            var entry = new JObject();
            entry["territoryID"] = (int)terrID;
            entry["armiesDeployed"] = armiesDeployed;

            var turnData = new JObject();
            turnData["turnNumber"] = currentTurnNumber;
            turnData["deployment"] = entry;

            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Raw//Maps//" + mapID.ToString() + "//AttackDeployments");
            var gamePath = currentGameID.ToString() + ".txt";
            AppendToFile(turnData.ToString() + '!', dir, gamePath);
        }

        public static void WriteMapAttackDeploymentMeans(List<Dictionary<TerritoryIDType, double>> means, MapIDType mapID)
        {
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Consolidated//Maps//" + mapID.ToString() + "//AttackDeployments");
            var gamePath = "means.txt";
            ForceCreateFile(dir, gamePath); // overwrite because we don't want conflicting information

            for (var i = 0; i < means.Count; i++)
            {
                var dict = means[i];
                var data = CreateDeploymentJson(dict.ToList(), i);
                AppendToFile(data.ToString() + '!', dir, gamePath);
            }
        }

        public static void WriteMapAttackDeploymentMeansComprehensiveData(List<Dictionary<TerritoryIDType, List<double>>> armies, MapIDType mapID)
        {
            // create the JSON object for the turn.
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Consolidated//Maps//" + mapID.ToString() + "//AttackDeployments");
            var gamePath = "means_comp.txt";
            ForceCreateFile(dir, gamePath); // overwrite because we don't want conflicting information

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
                data["deployments"] = armyData;

                AppendToFile(data.ToString() + '!', dir, gamePath);
            }
        }

        public static void WriteGameDefenseDeploymentData(TerritoryIDType terrID, int armiesDeployed, MapIDType mapID)
        {
            // get map id somehow
            var entry = new JObject();
            entry["territoryID"] = (int)terrID;
            entry["armiesDeployed"] = armiesDeployed;

            var turnData = new JObject();
            turnData["turnNumber"] = currentTurnNumber;
            turnData["deployment"] = entry;

            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Raw//Maps//" + mapID.ToString() + "//DefenseDeployments");
            var gamePath = currentGameID.ToString() + ".txt";
            AppendToFile(turnData.ToString() + '!', dir, gamePath);
        }

        public static void WriteMapDefenseDeploymentMeans(List<Dictionary<TerritoryIDType, double>> means, MapIDType mapID)
        {
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Consolidated//Maps//" + mapID.ToString() + "//DefenseDeployments");
            var gamePath = "means.txt";
            ForceCreateFile(dir, gamePath); // overwrite because we don't want conflicting information

            for (var i = 0; i < means.Count; i++)
            {
                var dict = means[i];
                var data = CreateDeploymentJson(dict.ToList(), i);
                AppendToFile(data.ToString() + '!', dir, gamePath);
            }
        }

        public static void WriteMapDefenseDeploymentMeansComprehensiveData(List<Dictionary<TerritoryIDType, List<double>>> armies, MapIDType mapID)
        {
            // create the JSON object for the turn.
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Consolidated//Maps//" + mapID.ToString() + "//DefenseDeployments");
            var gamePath = "means_comp.txt";
            ForceCreateFile(dir, gamePath); // overwrite because we don't want conflicting information

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
                data["deployments"] = armyData;

                AppendToFile(data.ToString() + '!', dir, gamePath);
            }
        }
        public static void WriteMapStandingArmyMeans(List<Dictionary<TerritoryIDType, double>> armies, MapIDType mapID)
        {
            // create the JSON object for the turn.
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Consolidated//Maps//" + mapID.ToString() + "StandingArmies");
            var gamePath = "means.txt";
            ForceCreateFile(dir, gamePath); // overwrite because we don't want conflicting information

            var armyData = new JArray();
            for (var i = 0; i < armies.Count; i++)
            {
                var dict = armies[i];
                var data = CreateStandingArmyJson(dict.ToList(), i);
                AppendToFile(data.ToString() + '!', dir, gamePath);
            }
        }

        public static void WriteMapStandingArmyMeansComprehensiveData(List<Dictionary<TerritoryIDType, List<double>>> armies, MapIDType mapID)
        {
            // create the JSON object for the turn.
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Consolidated//Maps//" + mapID.ToString() + "StandingArmies");
            var gamePath = "means_comp.txt";
            ForceCreateFile(dir, gamePath); // overwrite because we don't want conflicting information

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

        private static JObject CreateDeploymentJson(IEnumerable<KeyValuePair<TerritoryIDType, double>> armies, int turnNumber)
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
            data["deployment"] = armyData;

            return data;
        }

        private static void AppendToFile(string content, string dir, string filename)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir, filename), content);
        }

        private static void ForceCreateFile(string dir, string filename)
        {
            Directory.CreateDirectory(dir);
            var sw = File.Create(Path.Combine(dir, filename));
            sw.Close(); // hacky
        }
    }
}
