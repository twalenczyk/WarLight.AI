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
                var rawData = GetDeploymentComprehensiveData(mapID, true);

                // consider storing the rawData collection somewhere
                ret = rawData.Select(
                    dict => dict.Select(
                            kvp => new KeyValuePair<TerritoryIDType, double>(kvp.Key, kvp.Value.Average()))
                        .ToDictionary(
                            kvp => kvp.Key, kvp => kvp.Value))
                        .ToList();

                // write the data for future use
                DataCollector.WriteMapAttackDeploymentMeans(ret, mapID);
            }

            return ret;
        }

        public static List<Dictionary<TerritoryIDType, List<double>>> GetAttackDeploymentsMeansComprehensiveData(MapIDType mapID)
        {
            var ret = new List<Dictionary<TerritoryIDType, List<double>>>();

            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Consolidated//Maps//" + mapID.ToString() + "//AttackDeployments");
            var fp = Path.Combine("means_comp.txt");

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
                    var borderArmies = data["deployments"];

                    // in case turn numbers are unordered/missing
                    for (var i = ret.Count; i <= turnNumber; i++)
                    {
                        ret.Add(new Dictionary<TerritoryIDType, List<double>>());
                    }

                    foreach (var border in borderArmies)
                    {
                        var id = (TerritoryIDType)border["territoryID"].Value<int>();
                        var armies = border["armies"].Select(token => token.Value<double>()).ToList();
                        ret[turnNumber][id] = armies;
                    }
                }
            }
            else
            {
                // create the file from relevant game files on the map
                ret = GetDeploymentComprehensiveData(mapID, true);
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
                var rawData = GetDeploymentComprehensiveData(mapID, false);

                // consider storing the rawData collection somewhere
                ret = rawData.Select(
                    dict => dict.Select(
                            kvp => new KeyValuePair<TerritoryIDType, double>(kvp.Key, kvp.Value.Average()))
                        .ToDictionary(
                            kvp => kvp.Key, kvp => kvp.Value))
                        .ToList();

                // write the data for future use
                DataCollector.WriteMapDefenseDeploymentMeans(ret, mapID);
            }

            return ret;
        }

        public static List<Dictionary<TerritoryIDType, List<double>>> GetDefenseDeploymentsMeansComprehensiveData(MapIDType mapID)
        {
            var ret = new List<Dictionary<TerritoryIDType, List<double>>>();

            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Consolidated//Maps//" + mapID.ToString() + "//DefenseDeployments");
            var fp = Path.Combine("means_comp.txt");

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
                    var borderArmies = data["deployments"];

                    // in case turn numbers are unordered/missing
                    for (var i = ret.Count; i <= turnNumber; i++)
                    {
                        ret.Add(new Dictionary<TerritoryIDType, List<double>>());
                    }

                    foreach (var border in borderArmies)
                    {
                        var id = (TerritoryIDType)border["territoryID"].Value<int>();
                        var armies = border["armies"].Select(token => token.Value<double>()).ToList();
                        ret[turnNumber][id] = armies;
                    }
                }
            }
            else
            {
                // create the file from relevant game files on the map
                ret = GetDeploymentComprehensiveData(mapID, false);
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

            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Consolidated//Maps//" + mapID.ToString() + "//StandingArmies");
            var fp = Path.Combine(dir, "means.txt");
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
                // consolidate the data from the set of current games on file
                var rawData = GetStandingArmiesComprehensiveData(mapID);

                // consider storing the rawData collection somewhere
                ret = rawData.Select(
                    dict => dict.Select(
                            kvp => new KeyValuePair<TerritoryIDType, double>(kvp.Key, kvp.Value.Average()))
                        .ToDictionary(
                            kvp => kvp.Key, kvp => kvp.Value))
                        .ToList();

                // write the data for future use
                DataCollector.WriteMapStandingArmyMeans(ret, mapID);
            }


            return ret;
        }

        public static List<Dictionary<TerritoryIDType, List<double>>> GetStandingArmyComprehensive(MapIDType mapID)
        {
            var ret = new List<Dictionary<TerritoryIDType, List<double>>>();

            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Consolidated//Maps//" + mapID.ToString() + "//StandingArmies");
            var fp = Path.Combine(dir, "means_comp.txt");

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
                ret = GetStandingArmiesComprehensiveData(mapID);
            }

            return ret;
        }

        /*
         * *************************
         * Helper Methods
         * *************************
         */
        private static List<Dictionary<TerritoryIDType, List<double>>> ParseStandingArmiesGame(string filename)
        {
            var ret = new List<Dictionary<TerritoryIDType, List<double>>>();

            var text = File.ReadAllText(filename);
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
                    var armies = border["armies"].Value<double>();

                    if (!ret[turnNumber].ContainsKey(id))
                    {
                        ret[turnNumber].Add(id, new List<double>());
                    }
                    ret[turnNumber][id].Add(armies);
                }
            }

            return ret;
        }

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

        private static List<Dictionary<TerritoryIDType, List<double>>> GetDeploymentComprehensiveData(MapIDType mapID, bool isAttackQuery)
        {
            var rawData = new List<Dictionary<TerritoryIDType, List<double>>>();
            var dirEnding = isAttackQuery ? "//AttackDeployments" : "//DefenseDeployments";
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Raw//Maps//" + mapID.ToString() + dirEnding);

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

            if (isAttackQuery)
            {
                DataCollector.WriteMapAttackDeploymentMeansComprehensiveData(rawData, mapID);
            }
            else
            {
                DataCollector.WriteMapDefenseDeploymentMeansComprehensiveData(rawData, mapID);
            }
            return rawData;
        }

        private static List<Dictionary<TerritoryIDType, List<double>>> GetStandingArmiesComprehensiveData(MapIDType mapID)
        {
            var rawData = new List<Dictionary<TerritoryIDType, List<double>>>();
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Raw//Maps//" + mapID.ToString() + "//StandingArmies");

            foreach (var gameFile in Directory.GetFiles(dir))
            {
                var deploymentDataPerTurn = ParseStandingArmiesGame(gameFile);

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

            // update this
            DataCollector.WriteMapStandingArmyMeansComprehensiveData(rawData, mapID);
            return rawData;
        }
    }
}
