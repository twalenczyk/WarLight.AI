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

        /*
         * Attack deployment vectors.
         */
        private List<Dictionary<TerritoryIDType, double>> AttackDeploymentMeansPerTurn;
        private List<Dictionary<TerritoryIDType, List<double>>> AttackDeploymentMeansComprehensiveDataPerTurn;
        private List<Dictionary<TerritoryIDType, double>> AttackDeploymentVariancesPerTurn;
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> AttackDeploymentCorrelationsPerTurn;

        /*
         * Attack power vectors.
         */
        private List<Dictionary<TerritoryIDType, double>> AttackPowerMeansPerTurn;
        private List<Dictionary<TerritoryIDType, List<double>>> AttackPowerMeansComprehensiveDataPerTurn;
        private List<Dictionary<TerritoryIDType, double>> AttackPowerVariancesPerTurn;
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> AttackPowerCorrelationsPerTurn;

        /*
         * Defense deployment vectors.
         */
        private List<Dictionary<TerritoryIDType, double>> DefenseDeploymentMeansPerTurn;
        private List<Dictionary<TerritoryIDType, List<double>>> DefenseDeploymentMeansComprehensiveDataPerTurn;
        private List<Dictionary<TerritoryIDType, double>> DefenseDeploymentVariancesPerTurn;
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> DefenseDeploymentCorrelationsPerTurn;

        /*
         * Defemse power vectors.
         */
        private List<Dictionary<TerritoryIDType, double>> DefensePowerMeansPerTurn;
        private List<Dictionary<TerritoryIDType, List<double>>> DefensePowerMeansComprehensiveDataPerTurn;
        private List<Dictionary<TerritoryIDType, double>> DefensePowerVariancesPerTurn;
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> DefensePowerCorrelationsPerTurn;

        /*
         * Standing army vectors.
         */
        private List<Dictionary<TerritoryIDType, double>> StandingArmiesPerTurnMean;
        private List<Dictionary<TerritoryIDType, List<double>>> StandingArmyMeansComprehensiveDataPerTurn;
        private List<Dictionary<TerritoryIDType, double>> StandingArmiesPerTurnVariance;
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> StandingArmiesPerTurnCorrelations;

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
            this.SetAttackDeploymentMeans();
            var turn = turnNumber >= this.AttackDeploymentMeansPerTurn.Count ? this.AttackDeploymentMeansPerTurn.Count - 1 : turnNumber; // heuristic
            return this.AttackDeploymentMeansPerTurn[turn]
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Dictionary<TerritoryIDType, double> GetAttackDeploymentVariances(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetAttackDeploymentVariances();
            var turn = turnNumber >= this.AttackDeploymentVariancesPerTurn.Count ? this.AttackDeploymentVariancesPerTurn.Count - 1 : turnNumber; // heuristic
            return this.AttackDeploymentVariancesPerTurn[turn]
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>> GetAttackDeploymentCorrelations(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetAttackDeploymentCorrelations();
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
         * Attack Power Methods
         * *********************
         */
        public Dictionary<TerritoryIDType, double> GetAttackPowerMeans(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetAttackPowerMeans();
            var turn = turnNumber >= this.AttackPowerMeansPerTurn.Count ? this.AttackPowerMeansPerTurn.Count - 1 : turnNumber; // heuristic
            return this.AttackPowerMeansPerTurn[turn]
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /*
         * *********************
         * Defense Deployment Methods
         * *********************
         */
        public Dictionary<TerritoryIDType, double> GetDefenseDeploymentMeans(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetDefenseDeploymentMeans();
            var turn = turnNumber >= this.StandingArmiesPerTurnMean.Count ? this.StandingArmiesPerTurnMean.Count - 1 : turnNumber; // heuristic
            return this.StandingArmiesPerTurnMean[turn]
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Dictionary<TerritoryIDType, double> GetDefenseDeploymentVariances(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetDefenseDeploymentVariances();
            var turn = turnNumber >= this.DefenseDeploymentVariancesPerTurn.Count ? this.DefenseDeploymentVariancesPerTurn.Count - 1 : turnNumber; // heuristic
            return this.DefenseDeploymentVariancesPerTurn[turn]
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        public Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>> GetDefenseDeploymentsCorrelations(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetDefenseDeploymentCorrelations();
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
         * Defense Power Methods
         * *********************
         */
        public Dictionary<TerritoryIDType, double> GetDefensePowerMeans(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetDefensePowerMeans();
            var turn = turnNumber >= this.AttackPowerMeansPerTurn.Count ? this.AttackPowerMeansPerTurn.Count - 1 : turnNumber; // heuristic
            return this.AttackPowerMeansPerTurn[turn]
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /*
         * *********************
         * Standing Army Methods
         * *********************
         */
        public Dictionary<TerritoryIDType, double> GetStandingArmyMean(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetStandingArmiesMeans();
            var turn = turnNumber >= this.StandingArmiesPerTurnMean.Count ? this.StandingArmiesPerTurnMean.Count - 1 : turnNumber; // heuristic
            return this.StandingArmiesPerTurnMean[turn]
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Dictionary<TerritoryIDType, double> GetStandingArmyVariances(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetStandingArmiesVariances();
            var turn = turnNumber >= this.StandingArmiesPerTurnVariance.Count ? this.StandingArmiesPerTurnVariance.Count - 1 : turnNumber; // heuristic
            return this.StandingArmiesPerTurnVariance[turn]
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>> GetStandingArmyCorrelations(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetStandingArmiesCorrelations();
            var turn = turnNumber >= this.StandingArmiesPerTurnCorrelations.Count ? this.StandingArmiesPerTurnCorrelations.Count - 1 : turnNumber; // heuristic
            return this.StandingArmiesPerTurnCorrelations[turn]
                .Select(
                    kvp => new KeyValuePair<TerritoryIDType, IEnumerable<KeyValuePair<TerritoryIDType, double>>>(
                        kvp.Key,
                        kvp.Value.Where(pvk => territories.Contains(pvk.Key))))
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDictionary(pvk => pvk.Key, pvk => pvk.Value));
        }

        private void SetAttackDeploymentMeans()
        {
            if (this.AttackDeploymentMeansPerTurn == null)
                this.AttackDeploymentMeansPerTurn = Parser.GetAttackDeploymentMeans(this.MapID);
        }

        private void SetAttackDeploymentMeansComprehensiveData()
        {
            if (this.AttackDeploymentMeansComprehensiveDataPerTurn == null)
                this.AttackDeploymentMeansComprehensiveDataPerTurn = Parser.GetAttackDeploymentsMeansComprehensiveData(this.MapID);
        }

        private void SetAttackDeploymentVariances()
        {
            if (this.AttackDeploymentVariancesPerTurn == null)
            {
                // initialize the vector
                this.SetAttackDeploymentMeans();
                this.SetAttackDeploymentMeansComprehensiveData();
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
        }

        private void SetAttackDeploymentCorrelations()
        {
            if (this.AttackDeploymentCorrelationsPerTurn == null)
            {
                // initialize the matrix
                this.SetAttackDeploymentMeans();
                this.SetAttackDeploymentMeansComprehensiveData();
                this.SetAttackDeploymentVariances();

                var correlationRandomVariables = new List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>();
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
                    foreach (var ikvp in this.AttackDeploymentVariancesPerTurn[index])
                    {
                        var i = ikvp.Key;
                        this.AttackDeploymentCorrelationsPerTurn[index][i] = new Dictionary<TerritoryIDType, double>();

                        foreach (var jkvp in this.AttackDeploymentVariancesPerTurn[index])
                        {
                            var j = jkvp.Key;
                            var iSig = ikvp.Value;
                            var jSig = jkvp.Value;

                            if (iSig == 0 || jSig == 0)
                            {
                                // throw an error!
                            }

                            var correlationValue = correlationRandomVariables[index][i][j] / (iSig * jSig);
                            this.AttackDeploymentCorrelationsPerTurn[index][i][j] = correlationValue;
                        }
                    }
                }
            }
        }

        private void SetAttackPowerMeans()
        {
            // unlike other methods in this class, we create this statistic from dat on our hands
            if (this.AttackPowerMeansPerTurn == null)
            {
                this.SetAttackPowerMeansComprehensiveData();
                this.AttackPowerMeansPerTurn = this.AttackPowerMeansComprehensiveDataPerTurn
                    .Select(dict => dict
                        .Select(kvp => new KeyValuePair<TerritoryIDType, double>(
                            kvp.Key,
                            kvp.Value.Average()))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
                    .ToList();
            }
        }

        private void SetAttackPowerMeansComprehensiveData()
        {
            // unlike other methods in this class, we create this statistic from dat on our hands
            if (this.AttackPowerMeansComprehensiveDataPerTurn == null)
            {
                this.SetAttackDeploymentMeansComprehensiveData();
                this.SetStandingArmiesMeans();
                this.SetStandingArmiesMeansComprehensiveData();

                // the mean for this random variable is the average of the different deployments and standing army power
                this.AttackPowerMeansComprehensiveDataPerTurn = new List<Dictionary<TerritoryIDType, List<double>>>();

                for (var index = 0; index < this.StandingArmyMeansComprehensiveDataPerTurn.Count; index++)
                {
                    this.AttackPowerMeansComprehensiveDataPerTurn.Add(new Dictionary<TerritoryIDType, List<double>>());

                    foreach (var kvp in this.StandingArmyMeansComprehensiveDataPerTurn[index])
                    {
                        // for each territory and each standing army figure, add the various deployments to that territory
                        var territoryID = kvp.Key;

                        if (this.AttackDeploymentMeansComprehensiveDataPerTurn[index].ContainsKey(territoryID))
                        {
                            this.AttackPowerMeansComprehensiveDataPerTurn[index][territoryID] = kvp.Value
                                .SelectMany(val => this.AttackDeploymentMeansComprehensiveDataPerTurn[index][territoryID]
                                    .Select(deployment => val + deployment))
                                .ToList();
                        }
                        else
                        {
                            // no deployments, so it's just the average standing army on that turn
                            this.AttackPowerMeansComprehensiveDataPerTurn[index][territoryID] = this.StandingArmyMeansComprehensiveDataPerTurn[index][territoryID];
                        }
                    }
                }
            }
        }

        private void SetAttackPowerVariances()
        {

        }

        private void SetDefenseDeploymentMeans()
        {
            if (this.DefenseDeploymentMeansPerTurn == null)
                this.DefenseDeploymentMeansPerTurn = Parser.GetDefenseDeploymentMeans(this.MapID);
        }

        private void SetDefenseDeploymentMeansComprehensiveData()
        {
            if (this.DefenseDeploymentMeansComprehensiveDataPerTurn == null)
                this.DefenseDeploymentMeansComprehensiveDataPerTurn = Parser.GetDefenseDeploymentsMeansComprehensiveData(this.MapID);
        }

        private void SetDefenseDeploymentVariances()
        {
            if (this.DefenseDeploymentVariancesPerTurn == null)
            {
                // initialize the vector
                this.SetDefenseDeploymentMeans();
                this.SetDefenseDeploymentMeansComprehensiveData();
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
        }

        private void SetDefenseDeploymentCorrelations()
        {
            if (!(this.DefenseDeploymentCorrelationsPerTurn is List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>))
            {
                // initialize the matrix
                this.SetDefenseDeploymentMeans();
                this.SetDefenseDeploymentMeansComprehensiveData();
                this.SetDefenseDeploymentVariances();

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
                    foreach (var ikvp in this.DefenseDeploymentVariancesPerTurn[index])
                    {
                        var i = ikvp.Key;
                        this.DefenseDeploymentCorrelationsPerTurn[index][i] = new Dictionary<TerritoryIDType, double>();

                        foreach (var jkvp in this.DefenseDeploymentVariancesPerTurn[index])
                        {
                            var j = jkvp.Key;
                            var iSig = ikvp.Value;
                            var jSig = jkvp.Value;
                            var correlationValue = correlationRandomVariables[index][i][j] / (iSig * jSig);
                            this.DefenseDeploymentCorrelationsPerTurn[index][i][j] = correlationValue;
                        }
                    }
                }
            }
        }

        private void SetDefensePowerMeans()
        {
            // unlike other methods in this class, we create this statistic from dat on our hands
            if (this.DefensePowerMeansPerTurn == null)
            {
                this.SetDefensePowerMeansComprehensiveData();
                this.DefensePowerMeansPerTurn = this.DefensePowerMeansComprehensiveDataPerTurn
                    .Select(dict => dict
                        .Select(kvp => new KeyValuePair<TerritoryIDType, double>(
                            kvp.Key,
                            kvp.Value.Average()))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
                    .ToList();
            }
        }

        private void SetDefensePowerMeansComprehensiveData()
        {
            if (this.DefensePowerMeansComprehensiveDataPerTurn == null)
            {
                this.SetDefenseDeploymentMeansComprehensiveData();
                this.SetStandingArmiesMeans();
                this.SetStandingArmiesMeansComprehensiveData();

                // the mean for this random variable is the average of the different deployments and standing army power
                this.DefensePowerMeansComprehensiveDataPerTurn = new List<Dictionary<TerritoryIDType, List<double>>>();

                for (var index = 0; index < this.StandingArmyMeansComprehensiveDataPerTurn.Count; index++)
                {
                    this.DefensePowerMeansComprehensiveDataPerTurn.Add(new Dictionary<TerritoryIDType, List<double>>());

                    foreach (var kvp in this.StandingArmyMeansComprehensiveDataPerTurn[index])
                    {
                        // for each territory and each standing army figure, add the various deployments to that territory
                        var territoryID = kvp.Key;

                        if (this.DefenseDeploymentMeansComprehensiveDataPerTurn[index].ContainsKey(territoryID))
                        {
                            this.DefensePowerMeansComprehensiveDataPerTurn[index][territoryID] = kvp.Value
                                .SelectMany(
                                    val => this.DefenseDeploymentMeansComprehensiveDataPerTurn[index][territoryID].Select(
                                        deployment => val + deployment))
                                .ToList();
                        }
                        else
                        {
                            // no deployments, so it's just the average standing army on that turn
                            // TODO: Consider adding a comprehensive structure for this random variable.
                            this.DefensePowerMeansComprehensiveDataPerTurn[index][territoryID] = this.StandingArmyMeansComprehensiveDataPerTurn[index][territoryID];
                        }
                    }
                }
            }
        }

        private void SetStandingArmiesMeans()
        {
            if (this.StandingArmiesPerTurnMean == null)
                this.StandingArmiesPerTurnMean = Parser.GetStandingArmyMean(this.MapID);
        }

        private void SetStandingArmiesMeansComprehensiveData()
        {
            if (this.StandingArmyMeansComprehensiveDataPerTurn == null)
                this.StandingArmyMeansComprehensiveDataPerTurn = Parser.GetStandingArmyComprehensive(this.MapID);
        }

        private void SetStandingArmiesVariances()
        {

            if (this.StandingArmiesPerTurnVariance == null)
            {
                // initialize the vector
                this.SetStandingArmiesMeans();
                this.SetStandingArmiesMeansComprehensiveData();
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
        }

        private void SetStandingArmiesCorrelations()
        {
            if (this.StandingArmiesPerTurnCorrelations is List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>)
            {
                // initialize the matrix
                this.SetStandingArmiesMeans();
                this.SetStandingArmiesMeansComprehensiveData();
                this.SetStandingArmiesVariances();

                // considering caching to improve performance
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
                    foreach (var ikvp in this.StandingArmiesPerTurnVariance[index])
                    {
                        var i = ikvp.Key;
                        this.StandingArmiesPerTurnCorrelations[index][i] = new Dictionary<TerritoryIDType, double>();

                        foreach (var jkvp in this.StandingArmiesPerTurnVariance[index])
                        {
                            var j = jkvp.Key;
                            var iSig = ikvp.Value;
                            var jSig = jkvp.Value;
                            var correlationValue = correlationRandomVariables[index][i][j] / (iSig * jSig);
                            this.StandingArmiesPerTurnCorrelations[index][i][j] = correlationValue;
                        }
                    }
                }
            }
        }
    }
}
