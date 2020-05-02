using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Snowbird
{
    public class MapModels
    {
        public MapIDType MapID;
        private List<Dictionary<TerritoryIDType, double>> AverageStandingArmiesPerTerritoryPerTurn;

        public MapModels(MapIDType mapID)
        {
            this.MapID = mapID;
        }

        public Dictionary<TerritoryIDType, double> GetStandingArmyProbabilities(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            if (!(AverageStandingArmiesPerTerritoryPerTurn is List<Dictionary<TerritoryIDType, double>>))
            {
                // initialize the model
                this.AverageStandingArmiesPerTerritoryPerTurn = Parser.GetStandingArmyMean(this.MapID);
            }

            var turn = turnNumber > this.AverageStandingArmiesPerTerritoryPerTurn.Count ? this.AverageStandingArmiesPerTerritoryPerTurn.Count : turnNumber; // heuristic

            return this.AverageStandingArmiesPerTerritoryPerTurn[turn - 1];
        }
    }
}
