using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.OptiProd.Modeling
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
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> AttackDeploymentCovariancesPerTurn;

        /*
         * Attack power vectors.
         */
        private List<Dictionary<TerritoryIDType, double>> AttackPowerMeansPerTurn;
        private List<Dictionary<TerritoryIDType, List<double>>> AttackPowerMeansComprehensiveDataPerTurn;
        private List<Dictionary<TerritoryIDType, double>> AttackPowerVariancesPerTurn;
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> AttackPowerCovariancesPerTurn;

        /*
         * Defense deployment vectors.
         */
        private List<Dictionary<TerritoryIDType, double>> DefenseDeploymentMeansPerTurn;
        private List<Dictionary<TerritoryIDType, List<double>>> DefenseDeploymentMeansComprehensiveDataPerTurn;
        private List<Dictionary<TerritoryIDType, double>> DefenseDeploymentVariancesPerTurn;
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> DefenseDeploymentCovariancesPerTurn;

        /*
         * Defemse power vectors.
         */
        private List<Dictionary<TerritoryIDType, double>> DefensePowerMeansPerTurn;
        private List<Dictionary<TerritoryIDType, List<double>>> DefensePowerMeansComprehensiveDataPerTurn;
        private List<Dictionary<TerritoryIDType, double>> DefensePowerVariancesPerTurn;
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> DefensePowerCovariancesPerTurn;

        /*
         * Standing army vectors.
         */
        private List<Dictionary<TerritoryIDType, double>> StandingArmiesPerTurnMean;
        private List<Dictionary<TerritoryIDType, List<double>>> StandingArmyMeansComprehensiveDataPerTurn;
        private List<Dictionary<TerritoryIDType, double>> StandingArmiesPerTurnVariance;
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> StandingArmiesPerTurnCovariances;

        /*
         * Reward function vectors. 
         */
        private List<Dictionary<TerritoryIDType, double>> RewardMeansPerTurn;
        private List<Dictionary<TerritoryIDType, List<double>>> RewardMeansComprehensiveDataPerTurn;
        private List<Dictionary<TerritoryIDType, double>> RewardVariancesPerTurn;
        private List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>> RewardCovariancesPerTurn;

        public MapModels(MapIDType mapID)
        {
            this.MapID = mapID;
        }

        /*
         * TODO:
         * 1) Refactor commonly used code to improve readibility.
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

        public Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>> GetAttackDeploymentCovariances(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetAttackDeploymentCovariances();
            var turn = turnNumber >= this.AttackDeploymentCovariancesPerTurn.Count ? this.AttackDeploymentCovariancesPerTurn.Count - 1 : turnNumber; // heuristic
            return this.AttackDeploymentCovariancesPerTurn[turn]
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

        public Dictionary<TerritoryIDType, double> GetAttackPowerVariances(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetAttackPowerVariances();
            var turn = turnNumber >= this.AttackPowerMeansPerTurn.Count ? this.AttackPowerMeansPerTurn.Count - 1 : turnNumber; // heuristic
            return this.AttackPowerVariancesPerTurn[turn]
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>> GetAttackPowerCovariances(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetAttackPowerCovariances();
            var turn = turnNumber >= this.AttackPowerMeansPerTurn.Count ? this.AttackPowerMeansPerTurn.Count - 1 : turnNumber; // heuristic
            return this.AttackPowerCovariancesPerTurn[turn]
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
        public Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>> GetDefenseDeploymentsCovariances(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetDefenseDeploymentCovariances();
            var turn = turnNumber >= this.DefenseDeploymentCovariancesPerTurn.Count ? this.DefenseDeploymentCovariancesPerTurn.Count - 1 : turnNumber; // heuristic
            return this.DefenseDeploymentCovariancesPerTurn[turn]
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

        public Dictionary<TerritoryIDType, double> GetDefensePowerVariances(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetDefensePowerVariances();
            var turn = turnNumber >= this.AttackPowerMeansPerTurn.Count ? this.AttackPowerMeansPerTurn.Count - 1 : turnNumber; // heuristic
            return this.DefensePowerVariancesPerTurn[turn]
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>> GetDefensePowerCovariances(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetDefensePowerCovariances();
            var turn = turnNumber >= this.AttackPowerMeansPerTurn.Count ? this.AttackPowerMeansPerTurn.Count - 1 : turnNumber; // heuristic
            return this.DefensePowerCovariancesPerTurn[turn]
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

        public Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>> GetStandingArmyCovariances(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            this.SetStandingArmiesCovariances();
            var turn = turnNumber >= this.StandingArmiesPerTurnCovariances.Count ? this.StandingArmiesPerTurnCovariances.Count - 1 : turnNumber; // heuristic
            return this.StandingArmiesPerTurnCovariances[turn]
                .Select(
                    kvp => new KeyValuePair<TerritoryIDType, IEnumerable<KeyValuePair<TerritoryIDType, double>>>(
                        kvp.Key,
                        kvp.Value.Where(pvk => territories.Contains(pvk.Key))))
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDictionary(pvk => pvk.Key, pvk => pvk.Value));
        }

        /*
         * *********************
         * Reward Methods
         * *********************
         */
        public Dictionary<TerritoryIDType, double> GetRewardMeans(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            //this.SetStandingArmiesMeans();
            var turn = turnNumber >= this.RewardMeansPerTurn.Count ? this.RewardMeansPerTurn.Count - 1 : turnNumber; // heuristic
            return this.RewardMeansPerTurn[turn]
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Dictionary<TerritoryIDType, double> GetRewardVariances(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            //this.SetStandingArmiesVariances();
            var turn = turnNumber >= this.RewardVariancesPerTurn.Count ? this.RewardVariancesPerTurn.Count - 1 : turnNumber; // heuristic
            return this.RewardVariancesPerTurn[turn]
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>> GetRewardCorrelations(IEnumerable<TerritoryIDType> territories, int turnNumber)
        {
            //this.SetStandingArmiesCovariances();
            var turn = turnNumber >= this.RewardCovariancesPerTurn.Count ? this.RewardCovariancesPerTurn.Count - 1 : turnNumber; // heuristic
            return this.RewardCovariancesPerTurn[turn]
                .Select(
                    kvp => new KeyValuePair<TerritoryIDType, IEnumerable<KeyValuePair<TerritoryIDType, double>>>(
                        kvp.Key,
                        kvp.Value.Where(pvk => territories.Contains(pvk.Key))))
                .Where(kvp => territories.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDictionary(pvk => pvk.Key, pvk => pvk.Value));
        }

        /*
         * *********************
         * Helper Methods
         * *********************
         */
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
                        var variance = kvp.Value.Select(value => Math.Pow(relevantMean - value, 2)).Sum() / (kvp.Value.Count - 1); // sample formula
                        this.AttackDeploymentVariancesPerTurn[i][territoryID] = variance;
                    }
                }
            }
        }

        private void SetAttackDeploymentCovariances()
        {
            if (this.AttackDeploymentCovariancesPerTurn == null)
            {
                // initialize the matrix
                this.SetAttackDeploymentMeans();
                this.SetAttackDeploymentMeansComprehensiveData();
                this.SetAttackDeploymentVariances();

                this.AttackDeploymentCovariancesPerTurn = new List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>();
                for (var index = 0; index < this.AttackDeploymentMeansComprehensiveDataPerTurn.Count; index++)
                {
                    this.AttackDeploymentCovariancesPerTurn.Add(new Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>());
                    var armyData = this.AttackDeploymentMeansComprehensiveDataPerTurn[index];

                    // per turn, calculate the expected value of the correlation
                    foreach (var ikvp in armyData)
                    {
                        var i = ikvp.Key;
                        var iMean = this.AttackDeploymentMeansPerTurn[index][i];
                        var iVarianceVector = ikvp.Value.Select(entry => entry - iMean);
                        this.AttackDeploymentCovariancesPerTurn[index][i] = new Dictionary<TerritoryIDType, double>();

                        foreach (var jkvp in armyData)
                        {
                            var j = jkvp.Key;
                            var jMean = this.AttackDeploymentMeansPerTurn[index][j];
                            var jVarianceVector = jkvp.Value.Select(entry => entry - jMean);

                            var ijCorrMean = iVarianceVector.Zip(jVarianceVector, (o, p) => o * p).Sum() / (iVarianceVector.Count() - 1);
                            this.AttackDeploymentCovariancesPerTurn[index][i][j] = ijCorrMean;
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
                            kvp.Value.Sum() / (kvp.Value.Count - 1))) // sample formula
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
            if (this.AttackPowerVariancesPerTurn == null)
            {
                this.SetAttackPowerMeans();
                this.SetAttackPowerMeansComprehensiveData();
                this.AttackPowerVariancesPerTurn = new List<Dictionary<TerritoryIDType, double>>();

                // compute the variance for each turn and each territory
                for (int i = 0; i < this.AttackPowerMeansComprehensiveDataPerTurn.Count; i++)
                {
                    this.AttackPowerVariancesPerTurn.Add(new Dictionary<TerritoryIDType, double>());
                    var armyData = this.AttackPowerMeansComprehensiveDataPerTurn[i];

                    foreach (var kvp in armyData)
                    {
                        var territoryID = kvp.Key;
                        var relevantMean = this.AttackPowerMeansPerTurn[i][territoryID];
                        var variance = kvp.Value.Select(value => Math.Pow(value - relevantMean, 2)).Sum() / (kvp.Value.Count - 1); // sample formula
                        this.AttackPowerVariancesPerTurn[i][territoryID] = variance;
                    }
                }
            }
        }

        private void SetAttackPowerCovariances()
        {
            if (this.AttackPowerCovariancesPerTurn == null)
            {
                // initialize the matrix
                this.SetAttackPowerMeans();
                this.SetAttackPowerMeansComprehensiveData();
                this.SetAttackPowerVariances();

                // considering caching to improve performance
                this.AttackPowerCovariancesPerTurn = new List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>();

                // set up the new random variable matrix
                for (var index = 0; index < this.AttackPowerMeansComprehensiveDataPerTurn.Count; index++)
                {
                    this.AttackPowerCovariancesPerTurn.Add(new Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>());
                    var armyData = this.AttackPowerMeansComprehensiveDataPerTurn[index];

                    // per turn, calculate the expected value of the correlation
                    // TODO cache the variance comprehensive data
                    foreach (var ikvp in armyData)
                    {
                        var i = ikvp.Key;
                        var iMean = this.AttackPowerMeansPerTurn[index][i];
                        var iVarianceVector = ikvp.Value.Select(entry => entry - iMean);
                        this.AttackPowerCovariancesPerTurn[index][i] = new Dictionary<TerritoryIDType, double>();

                        foreach (var jkvp in armyData)
                        {
                            var j = jkvp.Key;
                            var jMean = this.AttackPowerMeansPerTurn[index][j];
                            var jVarianceVector = jkvp.Value.Select(entry => entry - jMean);

                            var ijCorrMean = iVarianceVector.Zip(jVarianceVector, (o, p) => o * p).Sum() / (iVarianceVector.Count() - 1);
                            this.AttackPowerCovariancesPerTurn[index][i][j] = ijCorrMean;
                        }
                    }
                }
            }
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
                        var variance = kvp.Value.Select(value => Math.Pow(relevantMean - value, 2)).Sum() / (kvp.Value.Count - 1); // sample formula
                        this.DefenseDeploymentVariancesPerTurn[i][territoryID] = variance;
                    }
                }
            }
        }

        private void SetDefenseDeploymentCovariances()
        {
            if (this.DefenseDeploymentCovariancesPerTurn == null)
            {
                // initialize the matrix
                this.SetDefenseDeploymentMeans();
                this.SetDefenseDeploymentMeansComprehensiveData();
                this.SetDefenseDeploymentVariances();

                // considering caching to improve performance
                this.DefenseDeploymentCovariancesPerTurn = new List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>();

                // set up the new random variable matrix
                for (var index = 0; index < this.DefenseDeploymentMeansComprehensiveDataPerTurn.Count; index++)
                {
                    this.DefenseDeploymentCovariancesPerTurn.Add(new Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>());
                    var armyData = this.DefenseDeploymentMeansComprehensiveDataPerTurn[index];

                    // per turn, calculate the expected value of the correlation
                    foreach (var ikvp in armyData)
                    {
                        var i = ikvp.Key;
                        var iMean = this.DefenseDeploymentMeansPerTurn[index][i];
                        var iVarianceVector = ikvp.Value.Select(entry => entry - iMean);
                        this.DefenseDeploymentCovariancesPerTurn[index][i] = new Dictionary<TerritoryIDType, double>();

                        foreach (var jkvp in armyData)
                        {
                            var j = jkvp.Key;
                            var jMean = this.DefenseDeploymentMeansPerTurn[index][j];
                            var jVarianceVector = jkvp.Value.Select(entry => entry - jMean);

                            var ijCorrMean = iVarianceVector.Zip(jVarianceVector, (o, p) => o * p).Sum() / (iVarianceVector.Count() - 1);
                            this.DefenseDeploymentCovariancesPerTurn[index][i][j] = ijCorrMean;
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


        private void SetDefensePowerVariances()
        {
            if (this.DefensePowerVariancesPerTurn == null)
            {
                this.SetDefensePowerMeans();
                this.SetDefensePowerMeansComprehensiveData();
                this.DefensePowerVariancesPerTurn = new List<Dictionary<TerritoryIDType, double>>();

                // compute the variance for each turn and each territory
                for (int i = 0; i < this.DefensePowerMeansComprehensiveDataPerTurn.Count; i++)
                {
                    this.DefensePowerVariancesPerTurn.Add(new Dictionary<TerritoryIDType, double>());
                    var armyData = this.DefensePowerMeansComprehensiveDataPerTurn[i];

                    foreach (var kvp in armyData)
                    {
                        var territoryID = kvp.Key;
                        var relevantMean = this.DefensePowerMeansPerTurn[i][territoryID];
                        var variance = kvp.Value.Select(value => Math.Pow(relevantMean - value, 2)).Sum() / (kvp.Value.Count - 1); // sample formula
                        this.DefensePowerVariancesPerTurn[i][territoryID] = variance;
                    }
                }
            }
        }

        private void SetDefensePowerCovariances()
        {
            if (this.DefensePowerCovariancesPerTurn == null)
            {
                // initialize the matrix
                this.SetDefensePowerMeans();
                this.SetDefensePowerMeansComprehensiveData();
                this.SetDefensePowerVariances();

                // considering caching to improve performance
                this.DefensePowerCovariancesPerTurn = new List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>();

                // set up the new random variable matrix
                for (var index = 0; index < this.DefensePowerMeansComprehensiveDataPerTurn.Count; index++)
                {
                    this.DefensePowerCovariancesPerTurn.Add(new Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>());
                    var armyData = this.DefensePowerMeansComprehensiveDataPerTurn[index];

                    // per turn, calculate the expected value of the correlation
                    foreach (var ikvp in armyData)
                    {
                        var i = ikvp.Key;
                        var iMean = this.DefensePowerMeansPerTurn[index][i];
                        var iVarianceVector = ikvp.Value.Select(entry => entry - iMean);
                        this.DefensePowerCovariancesPerTurn[index][i] = new Dictionary<TerritoryIDType, double>();

                        foreach (var jkvp in armyData)
                        {
                            var j = jkvp.Key;
                            var jMean = this.DefensePowerMeansPerTurn[index][j];
                            var jVarianceVector = jkvp.Value.Select(entry => entry - jMean);

                            var ijCorrMean = iVarianceVector.Zip(jVarianceVector, (o, p) => o * p).Sum() / (iVarianceVector.Count() - 1);
                            this.DefensePowerCovariancesPerTurn[index][i][j] = ijCorrMean;
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
                        var variance = kvp.Value.Select(value => Math.Pow(relevantMean - value, 2)).Sum() / (kvp.Value.Count - 1); // sample formula
                        this.StandingArmiesPerTurnVariance[i].Add(territoryID, variance);
                    }
                }
            }
        }

        private void SetStandingArmiesCovariances()
        {
            if (this.StandingArmiesPerTurnCovariances == null)
            {
                // initialize the matrix
                this.SetStandingArmiesMeans();
                this.SetStandingArmiesMeansComprehensiveData();
                this.SetStandingArmiesVariances();

                // considering caching to improve performance
                this.StandingArmiesPerTurnCovariances = new List<Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>>();

                // set up the new random variable matrix
                for (var index = 0; index < this.StandingArmyMeansComprehensiveDataPerTurn.Count; index++)
                {
                    this.StandingArmiesPerTurnCovariances.Add(new Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>>());
                    // per turn, calculate the expected value of the correlation
                    var armyData = this.StandingArmyMeansComprehensiveDataPerTurn[index];
                    foreach (var ikvp in armyData)
                    {
                        var i = ikvp.Key;
                        var iMean = this.StandingArmiesPerTurnMean[index][i];
                        var iVarianceVector = ikvp.Value.Select(entry => entry - iMean);
                        this.StandingArmiesPerTurnCovariances[index].Add(i, new Dictionary<TerritoryIDType, double>());

                        foreach (var jkvp in armyData)
                        {
                            var j = jkvp.Key;
                            var jMean = this.StandingArmiesPerTurnMean[index][j];
                            var jVarianceVector = jkvp.Value.Select(entry => entry - jMean);

                            var ijCorrMean = iVarianceVector.Zip(jVarianceVector, (o, p) => o * p).Sum() / (iVarianceVector.Count() - 1);
                            this.StandingArmiesPerTurnCovariances[index][i][j] = ijCorrMean;
                        }
                    }
                }
            }
        }

        private void SetRewardMeans()
        {
            // unlike other methods in this class, we create this statistic from dat on our hands
            if (this.RewardMeansPerTurn == null)
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

        private void SetRewardMeansComprehensive()
        {
            if (this.RewardMeansComprehensiveDataPerTurn == null)
            {

            }
        }


        private void SetRewardVariances()
        {
            if (this.RewardVariancesPerTurn == null)
            {
            }
        }

        private void SetRewardCorrelations()
        {
            if (this.RewardCovariancesPerTurn == null)
            {
            }
        }
    }
}
