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
        private List<Dictionary<TerritoryIDType, List<double>>> StandingArmyMeansComprehensiveDataPerTurn;
        private List<Dictionary<TerritoryIDType, double>> StandingArmiesPerTurnVariance;
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> StandingArmiesPerTurnCorrelations;
        private List<Dictionary<TerritoryIDType, double>> DefenseDeploymentMeansPerTurn;
        private List<Dictionary<TerritoryIDType, List<double>>> DefenseDeploymentMeansComprehensiveDataPerTurn;
        private List<Dictionary<TerritoryIDType, double>> DefenseDeploymentVariancesPerTurn;
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> DefenseDeploymentCorrelationsPerTurn;
        private List<Dictionary<TerritoryIDType, double>> DefensePowerMeansPerTurn;
        private List<Dictionary<TerritoryIDType, double>> DefensePowerVariancesPerTurn;
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> DefensePowerCorrelationsPerTurn;
        private List<Dictionary<TerritoryIDType, double>> AttackDeploymentMeansPerTurn;
        private List<Dictionary<TerritoryIDType, List<double>>> AttackDeploymentMeansComprehensiveDataPerTurn;
        private List<Dictionary<TerritoryIDType, double>> AttackDeploymentVariancesPerTurn;
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> AttackDeploymentCorrelationsPerTurn;
        private List<Dictionary<TerritoryIDType, double>> AttackPowerMeansPerTurn;
        private List<Dictionary<TerritoryIDType, double>> AttackPowerVariancesPerTurn;
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> AttackPowerCorrelationsPerTurn;


        public MapModels(MapIDType mapID)
        {
            this.MapID = mapID;

            // initialize all lists if possible
            this.DefenseDeploymentMeansPerTurn = Parser.GetDefenseDeploymentMeans(mapID);
            this.AttackDeploymentMeansPerTurn = Parser.GetAttackDeploymentMeans(mapID);
        }

        /*
         * TODO:
         * 1) Refactor commonly used code to improve readibility.
         * 2) Store comprehensive data to prevent multiple parsings.
         * 3) Add helper functions to create class vectors to improve readiblity of public getters.
         */

        /*
         * *********************
         * Attack Deployment Methods
         * *********************
         */
        public Dictionary<TerritoryIDType, double> GetAttackDeploymentMeans(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            if (!(this.AttackDeploymentMeansPerTurn is List<Dictionary<TerritoryIDType, double>>))
            {
                // initialize the model
                this.AttackDeploymentMeansPerTurn = Parser.GetAttackDeploymentMeans(this.MapID);
            }

            var turn = turnNumber >= this.AttackDeploymentMeansPerTurn.Count ? this.AttackDeploymentMeansPerTurn.Count - 1 : turnNumber; // heuristic

            return this.AttackDeploymentMeansPerTurn[turn].Where(kvp => territories.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Dictionary<TerritoryIDType, double> GetAttackDeploymentVariances(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            if (!(this.AttackDeploymentVariancesPerTurn is List<Dictionary<TerritoryIDType, double>>))
            {
                // initialize the vector
                this.AttackDeploymentMeansComprehensiveDataPerTurn = Parser.GetAttackDeploymentsMeansComprehensiveData(this.MapID);
                this.AttackDeploymentVariancesPerTurn = new List<Dictionary<TerritoryIDType, double>>();

                // compute the variance for each turn and each territory
                for (int i = 0; i < this.AttackDeploymentMeansComprehensiveDataPerTurn.Count; i++)
                {
                    this.AttackDeploymentVariancesPerTurn.Add(new Dictionary<TerritoryIDType, double>());
                    var armyData = this.AttackDeploymentMeansComprehensiveDataPerTurn[i];

                    foreach (var kvp in armyData)
                    {
                        var territoryID = kvp.Key;
                        var relevantMean = this.AttackDeploymentMeansPerTurn[i][territoryID];
                        var variance = kvp.Value.Select(value => Math.Pow(relevantMean - value, 2)).Average();
                        this.AttackDeploymentVariancesPerTurn[i][territoryID] = variance;
                    }
                }
            }

            var turn = turnNumber >= this.AttackDeploymentVariancesPerTurn.Count ? this.AttackDeploymentVariancesPerTurn.Count - 1 : turnNumber; // heuristic
            return this.AttackDeploymentVariancesPerTurn[turn].Where(kvp => territories.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        public Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>> GetAttackDeploymentCorrelations(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            if (!(this.AttackDeploymentCorrelationsPerTurn is List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>))
            {
                // initialize the matrix
                if (!(this.AttackDeploymentMeansPerTurn is List<Dictionary<TerritoryIDType, double>>))
                {
                    _ = this.GetAttackDeploymentMeans(territories, turnNumber);
                }

                if (!(this.AttackDeploymentVariancesPerTurn is List<Dictionary<TerritoryIDType, double>>))
                {
                    _ = this.GetAttackDeploymentVariances(territories, turnNumber);
                }

                // considering caching to improve performance
                this.AttackDeploymentMeansComprehensiveDataPerTurn = Parser.GetAttackDeploymentsMeansComprehensiveData(this.MapID); // TODO store this data locally to prevent redundant parsing
                var correlationRandomVariables = new List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>();

                // set up the new random variable matrix
                for (var index = 0; index < this.AttackDeploymentMeansComprehensiveDataPerTurn.Count; index++)
                {
                    correlationRandomVariables.Add(new Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>());
                    var armyData = this.AttackDeploymentMeansComprehensiveDataPerTurn[index];

                    // per turn, calculate the expected value of the correlation
                    foreach (var ikvp in armyData)
                    {
                        var i = ikvp.Key;
                        var iMean = this.AttackDeploymentMeansPerTurn[index][i];
                        var iVarianceVector = ikvp.Value.Select(entry => entry - iMean);
                        correlationRandomVariables[index][i] = new Dictionary<TerritoryIDType, double>();

                        foreach (var jkvp in armyData)
                        {
                            var j = jkvp.Key;
                            var jMean = this.AttackDeploymentMeansPerTurn[index][j];
                            var jVarianceVector = jkvp.Value.Select(entry => entry - jMean);

                            var ijCorrMean = iVarianceVector.SelectMany(ivar => jVarianceVector.Select(jvar => ivar * jvar)).Average();
                            correlationRandomVariables[index][i][j] = ijCorrMean;
                        }
                    }
                }

                this.AttackDeploymentCorrelationsPerTurn = new List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>();

                // correlation between the two random variables for each turn
                for (var index = 0; index < this.DefenseDeploymentVariancesPerTurn.Count; index++)
                {
                    this.AttackDeploymentCorrelationsPerTurn.Add(new Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>());

                    // improve efficienes by storing ij = ji
                    foreach (var i in territories)
                    {
                        // simple error prevention
                        if (!this.AttackDeploymentVariancesPerTurn[index].ContainsKey(i))
                        {
                            continue;
                        }

                        this.AttackDeploymentCorrelationsPerTurn[index].Add(i, new Dictionary<TerritoryIDType, double>());
                        foreach (var j in territories)
                        {
                            // simple error prevention
                            if (!this.AttackDeploymentVariancesPerTurn[index].ContainsKey(j))
                            {
                                continue;
                            }

                            var iSig = this.AttackDeploymentVariancesPerTurn[index][i];
                            var jSig = this.AttackDeploymentVariancesPerTurn[index][j];
                            var correlationValue = 0.5; // correlationRandomVariables[index][i][j] / (iSig * jSig);
                            this.AttackDeploymentCorrelationsPerTurn[index][i][j] = correlationValue;
                        }
                    }
                }
            }

            var turn = turnNumber >= this.AttackDeploymentCorrelationsPerTurn.Count ? this.AttackDeploymentCorrelationsPerTurn.Count - 1 : turnNumber; // heuristic
            return this.AttackDeploymentCorrelationsPerTurn[turn]
                .Select(
                    kvp => new KeyValuePair<TerritoryIDType, IEnumerable<KeyValuePair<TerritoryIDType, double>>>(
                        kvp.Key,
                        kvp.Value.Where(pvk => territories.Contains(pvk.Key))))
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDictionary(pvk => pvk.Key, pvk => pvk.Value));
        }

        /*
         * *********************
         * Defense Deployment Methods
         * *********************
         */
        public Dictionary<TerritoryIDType, double> GetDefenseDeploymentMeans(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            if (!(this.DefenseDeploymentMeansPerTurn is List<Dictionary<TerritoryIDType, double>>))
            {
                // initialize the model
                this.DefenseDeploymentMeansPerTurn = Parser.GetDefenseDeploymentMeans(this.MapID);
            }

            var turn = turnNumber >= this.StandingArmiesPerTurnMean.Count ? this.StandingArmiesPerTurnMean.Count - 1 : turnNumber; // heuristic

            return this.StandingArmiesPerTurnMean[turn].Where(kvp => territories.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Dictionary<TerritoryIDType, double> GetDefenseDeploymentVariances(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            if (!(this.DefenseDeploymentVariancesPerTurn is List<Dictionary<TerritoryIDType, double>>))
            {
                // initialize the vector
                this.DefenseDeploymentMeansComprehensiveDataPerTurn = Parser.GetDefenseDeploymentsMeansComprehensiveData(this.MapID);
                this.DefenseDeploymentVariancesPerTurn = new List<Dictionary<TerritoryIDType, double>>();

                // compute the variance for each turn and each territory
                for (int i = 0; i < this.DefenseDeploymentMeansComprehensiveDataPerTurn.Count; i++)
                {
                    this.DefenseDeploymentVariancesPerTurn.Add(new Dictionary<TerritoryIDType, double>());
                    var armyData = this.DefenseDeploymentMeansComprehensiveDataPerTurn[i];

                    foreach (var kvp in armyData)
                    {
                        var territoryID = kvp.Key;
                        var relevantMean = this.DefenseDeploymentMeansPerTurn[i][territoryID];
                        var variance = kvp.Value.Select(value => Math.Pow(relevantMean - value, 2)).Average();
                        this.DefenseDeploymentVariancesPerTurn[i][territoryID] = variance;
                    }
                }
            }

            var turn = turnNumber >= this.DefenseDeploymentVariancesPerTurn.Count ? this.DefenseDeploymentVariancesPerTurn.Count - 1 : turnNumber; // heuristic
            return this.DefenseDeploymentVariancesPerTurn[turn].Where(kvp => territories.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        public Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>> GetDefenseDeploymentsCorrelations(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            if (!(this.DefenseDeploymentCorrelationsPerTurn is List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>))
            {
                // initialize the matrix
                if (!(this.DefenseDeploymentMeansPerTurn is List<Dictionary<TerritoryIDType, double>>))
                {
                    _ = this.GetDefenseDeploymentMeans(territories, turnNumber);
                }

                if (!(this.DefenseDeploymentVariancesPerTurn is List<Dictionary<TerritoryIDType, double>>))
                {
                    _ = this.GetDefenseDeploymentVariances(territories, turnNumber);
                }

                // considering caching to improve performance
                this.DefenseDeploymentMeansComprehensiveDataPerTurn = Parser.GetDefenseDeploymentsMeansComprehensiveData(this.MapID); // TODO store this data locally to prevent redundant parsing
                var correlationRandomVariables = new List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>();

                // set up the new random variable matrix
                for (var index = 0; index < this.DefenseDeploymentMeansComprehensiveDataPerTurn.Count; index++)
                {
                    correlationRandomVariables.Add(new Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>());
                    var armyData = this.DefenseDeploymentMeansComprehensiveDataPerTurn[index];

                    // per turn, calculate the expected value of the correlation
                    foreach (var ikvp in armyData)
                    {
                        var i = ikvp.Key;
                        var iMean = this.DefenseDeploymentMeansPerTurn[index][i];
                        var iVarianceVector = ikvp.Value.Select(entry => entry - iMean);
                        correlationRandomVariables[index][i] = new Dictionary<TerritoryIDType, double>();

                        foreach (var jkvp in armyData)
                        {
                            var j = jkvp.Key;
                            var jMean = this.DefenseDeploymentMeansPerTurn[index][j];
                            var jVarianceVector = jkvp.Value.Select(entry => entry - jMean);

                            var ijCorrMean = iVarianceVector.SelectMany(ivar => jVarianceVector.Select(jvar => ivar * jvar)).Average();
                            correlationRandomVariables[index][i][j] = ijCorrMean;
                        }
                    }
                }

                this.DefenseDeploymentCorrelationsPerTurn = new List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>();

                // correlation between the two random variables for each turn
                for (var index = 0; index < this.DefenseDeploymentVariancesPerTurn.Count; index++)
                {
                    this.DefenseDeploymentCorrelationsPerTurn.Add(new Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>());

                    // improve efficienes by storing ij = ji
                    foreach (var i in territories)
                    {
                        // simple error prevention
                        if (!this.DefenseDeploymentVariancesPerTurn[index].ContainsKey(i))
                        {
                            continue;
                        }

                        this.DefenseDeploymentCorrelationsPerTurn[index].Add(i, new Dictionary<TerritoryIDType, double>());
                        foreach (var j in territories)
                        {
                            // simple error prevention
                            if (!this.DefenseDeploymentVariancesPerTurn[index].ContainsKey(j))
                            {
                                continue;
                            }

                            var iSig = this.DefenseDeploymentVariancesPerTurn[index][i];
                            var jSig = this.DefenseDeploymentVariancesPerTurn[index][j];
                            var correlationValue = 0.5; // correlationRandomVariables[index][i][j] / (iSig * jSig);
                            this.DefenseDeploymentCorrelationsPerTurn[index][i][j] = correlationValue;
                        }
                    }
                }
            }

            var turn = turnNumber >= this.DefenseDeploymentCorrelationsPerTurn.Count ? this.DefenseDeploymentCorrelationsPerTurn.Count - 1 : turnNumber; // heuristic
            return this.DefenseDeploymentCorrelationsPerTurn[turn]
                .Select(
                    kvp => new KeyValuePair<TerritoryIDType, IEnumerable<KeyValuePair<TerritoryIDType, double>>>(
                        kvp.Key, 
                        kvp.Value.Where(pvk => territories.Contains(pvk.Key))))
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDictionary(pvk => pvk.Key, pvk => pvk.Value));
        }

        /*
         * *********************
         * Standing Army Methods
         * *********************
         */
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
                this.StandingArmyMeansComprehensiveDataPerTurn = Parser.GetStandingArmyComprehensive(this.MapID);
                this.StandingArmiesPerTurnVariance = new List<Dictionary<TerritoryIDType, double>>();

                // compute the variance for each turn and each territory
                for (int i = 0; i < this.StandingArmyMeansComprehensiveDataPerTurn.Count; i++) 
                {
                    this.StandingArmiesPerTurnVariance.Add(new Dictionary<TerritoryIDType, double>());
                    var armyData = this.StandingArmyMeansComprehensiveDataPerTurn[i];

                    foreach (var kvp in armyData) 
                    {
                        var territoryID = kvp.Key;
                        var relevantMean = this.StandingArmiesPerTurnMean[i][territoryID];
                        var variance = kvp.Value.Select(value => Math.Pow(relevantMean - value, 2)).Average();
                        this.StandingArmiesPerTurnVariance[i].Add(territoryID, variance);
                    }
                }
            }

            var turn = turnNumber >= this.StandingArmiesPerTurnVariance.Count ? this.StandingArmiesPerTurnVariance.Count - 1 : turnNumber; // heuristic
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
                this.StandingArmyMeansComprehensiveDataPerTurn = Parser.GetStandingArmyComprehensive(this.MapID);
                var correlationRandomVariables = new List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>();

                // set up the new random variable matrix
                for (var index = 0; index < this.StandingArmyMeansComprehensiveDataPerTurn.Count; index++)
                {
                    correlationRandomVariables.Add(new Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>());
                    // per turn, calculate the expected value of the correlation
                    var armyData = this.StandingArmyMeansComprehensiveDataPerTurn[index];
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
