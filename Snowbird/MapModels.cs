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
            if (!(this.StandingArmiesPerTurnVariance is List<Dictionary<TerritoryIDType, double>>))
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
                        var variance = kvp.Value.Select(value => Math.Pow(relevantMean - value, 2)).Average();
                        this.StandingArmiesPerTurnVariance[i].Add(territoryID, variance);
                    }
                }
            }

            var turn = turnNumber >= this.StandingArmiesPerTurnMean.Count ? this.StandingArmiesPerTurnMean.Count - 1 : turnNumber; // heuristic
            return this.StandingArmiesPerTurnVariance[turn];
        }

        public Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>> GetStandingArmyCorrelations(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            if (!(this.StandingArmiesPerTurnCorrelations is List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>))
            {
                // initialize the matrix
                if (!(this.StandingArmiesPerTurnMean is List<Dictionary<TerritoryIDType, double>>))
                {
                    _ = this.GetStandingArmyMean(territories, turnNumber);
                }

                if (!(this.StandingArmiesPerTurnVariance is List<Dictionary<TerritoryIDType, double>>))
                {
                    _ = this.GetStandingArmyVariances(territories, turnNumber);
                }

                // considering caching to improve performance
                var standingArmiesComprehensive = Parser.GetStandingArmyComprehensive(this.MapID);
                var correlationRandomVariables = new List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>();

                // set up the new random variable matrix
                for (var index = 0; index < standingArmiesComprehensive.Count; index++)
                {
                    correlationRandomVariables.Add(new Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>());
                    // per turn, calculate the expected value of the correlation
                    var armyData = standingArmiesComprehensive[index];
                    foreach (var ikvp in armyData)
                    {
                        var i = ikvp.Key;
                        var iMean = this.StandingArmiesPerTurnMean[index][i];
                        var iVarianceVector = ikvp.Value.Select(entry => entry - iMean);
                        correlationRandomVariables[index].Add(i, new Dictionary<TerritoryIDType, double>());

                        foreach (var jkvp in armyData)
                        {
                            var j = jkvp.Key;
                            var jMean = this.StandingArmiesPerTurnMean[index][j];
                            var jVarianceVector = jkvp.Value.Select(entry => entry - jMean);

                            var ijCorrMean = iVarianceVector.SelectMany(ivar => jVarianceVector.Select(jvar => ivar * jvar)).Average();
                            correlationRandomVariables[index][i][j] = ijCorrMean;
                        }
                    }
                }

                this.StandingArmiesPerTurnCorrelations = new List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>();

                // correlation between the two random variables for each turn
                for (var index = 0; index < this.StandingArmiesPerTurnVariance.Count; index++)
                {
                    this.StandingArmiesPerTurnCorrelations.Add(new Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>());

                    // improve efficienes by storing ij = ji
                    foreach (var i in territories)
                    {
                        // simple error prevention
                        if (!this.StandingArmiesPerTurnVariance[index].ContainsKey(i))
                        {
                            continue;
                        }

                        this.StandingArmiesPerTurnCorrelations[index].Add(i, new Dictionary<TerritoryIDType, double>());
                        foreach (var j in territories)
                        {
                            // simple error prevention
                            if (!this.StandingArmiesPerTurnVariance[index].ContainsKey(j))
                            {
                                continue;
                            }

                            var iSig = this.StandingArmiesPerTurnVariance[index][i];
                            var jSig = this.StandingArmiesPerTurnVariance[index][j];
                            var correlationValue = 0.5; // correlationRandomVariables[index][i][j] / (iSig * jSig);
                            this.StandingArmiesPerTurnCorrelations[index][i][j] = correlationValue;
                        }
                    }
                }
            }

            var turn = turnNumber >= this.StandingArmiesPerTurnCorrelations.Count ? this.StandingArmiesPerTurnCorrelations.Count - 1 : turnNumber; // heuristic
            return this.StandingArmiesPerTurnCorrelations[turn];
        }
    }
}
