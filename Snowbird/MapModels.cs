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
        private List<Dictionary<TerritoryIDType, double>> StandingArmiesPerTurnMean;
        private List<Dictionary<TerritoryIDType, double>> StandingArmiesPerTurnVariance;
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> StandingArmiesPerTurnCorrelations;

        public MapModels(MapIDType mapID)
        {
            this.MapID = mapID;
        }

        public Dictionary<TerritoryIDType, double> GetStandingArmyMean(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            if (!(StandingArmiesPerTurnMean is List<Dictionary<TerritoryIDType, double>>))
            {
                // initialize the model
                this.StandingArmiesPerTurnMean = Parser.GetStandingArmyMean(this.MapID);
            }

            var turn = turnNumber >= this.StandingArmiesPerTurnMean.Count ? this.StandingArmiesPerTurnMean.Count - 1 : turnNumber; // heuristic

            return this.StandingArmiesPerTurnMean[turn];
        }

        public Dictionary<TerritoryIDType, double> GetStandingArmyVariances(IEnumerable<TerritoryIDType> territories, int turnNumber) {
            if (!(StandingArmiesPerTurnVariance is List<Dictionary<TerritoryIDType, double>>))
            {
                // initialize the vector
                var standingArmiesComprehensive = Parser.GetStandingArmyComprehensive(this.MapID);
                this.StandingArmiesPerTurnVariance = new List<Dictionary<TerritoryIDType, double>>();

                // compute the variance for each turn and each territory
                for (int i = 0; i < standingArmiesComprehensive.Count; i++) 
                {
                    this.StandingArmiesPerTurnVariance.Add(new Dictionary<TerritoryIDType, double>());
                    var armyData = standingArmiesComprehensive[i];

                    foreach (var kvp in armyData) 
                    {
                        var territoryID = kvp.Key;
                        var relevantMean = this.StandingArmiesPerTurnMean[i][territoryID];
                        var varianceVector = kvp.Value;
                        varianceVector.ForEach(value => Math.Pow(relevantMean - value, 2));
                        var variance = varianceVector.Average();
                        this.StandingArmiesPerTurnVariance[i].Add(territoryID, variance);
                    }
                }
            }

            var turn = turnNumber >= this.StandingArmiesPerTurnMean.Count ? this.StandingArmiesPerTurnMean.Count - 1 : turnNumber; // heuristic
            return this.StandingArmiesPerTurnVariance[turn];
        }    
    }
}
