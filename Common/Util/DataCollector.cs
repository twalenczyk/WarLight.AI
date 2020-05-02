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

        public static void WriteStandingArmyData(IEnumerable<KeyValuePair<TerritoryIDType, int>> armies)
        {
            // create the JSON object for the turn.
            var armyData = new JArray();
            foreach (KeyValuePair<TerritoryIDType, int> kvp in armies)
            {
                var entry = new JObject();
                entry["territoryID"] = (int)kvp.Key;
                entry["armies"] = kvp.Value;
                armyData.Add(entry);
            }

            var data = new JObject();
            data["turnNumber"] = currentTurnNumber;
            data["borderArmies"] = armyData;

            // Append data to file
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DataCollection//Games");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var gamePath = Path.Combine(dir, currentGameID.ToString() + ".txt");
            File.AppendAllText(gamePath, data.ToString() + "!");
        }
    }
}
