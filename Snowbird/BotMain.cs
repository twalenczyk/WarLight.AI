﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace WarLight.Shared.AI.Snowbird
{
    public class BotMain : IWarLightAI
    {
        /// <inheritdoc/>
        public string Name() => "The Snowbird AI.";

        /// <inheritdoc/>
        public string Description() => "A Warzone bot that uses quadratic programming optimizatin algorithms to determine moves.";

        /// <inheritdoc/>
        public bool SupportsSettings(GameSettings settings, out string whyNot)
        {
            /*
             * I need to update the whyNot string with all the reasons why this
             * AI cannot support various settings. As this project is for class,
             * I only care about playing turns in fairly vanilla game settings.
             * 
             * If this project grows into something more, I can add support for more
             * settings later.
             * 
             * Alternatively, I can defer simply use the other bots implementations
             * in those cases.
             */
            whyNot = null;
            return true;
        }

        /// <inheritdoc/>
        public bool RecommendsSettings(GameSettings settings, out string whyNot)
        {
            var sb = new StringBuilder();

            /*
             * I need to amend these recommendations.
             */
            if (settings.NoSplit)
                sb.AppendLine("This bot does not understand no-split mode and will issue attacks as if no-split mode was disabled.");
            if (settings.Cards.ContainsKey(CardType.OrderPriority.CardID))
                sb.AppendLine("This bot does not understand how to play Order Priority cards.");
            if (settings.Cards.ContainsKey(CardType.OrderDelay.CardID))
                sb.AppendLine("This bot does not understand how to play Order Delay cards.");
            if (settings.Cards.ContainsKey(CardType.Airlift.CardID))
                sb.AppendLine("This bot does not understand how to play Airlift cards.");
            if (settings.Cards.ContainsKey(CardType.Gift.CardID))
                sb.AppendLine("This bot does not understand how to play Gift cards.");
            if (settings.Cards.ContainsKey(CardType.Reconnaissance.CardID))
                sb.AppendLine("This bot does not understand how to play Reconnaissance cards.");
            if (settings.Cards.ContainsKey(CardType.Spy.CardID))
                sb.AppendLine("This bot does not understand how to play Spy cards.");
            if (settings.Cards.ContainsKey(CardType.Surveillance.CardID))
                sb.AppendLine("This bot does not understand how to play Surveillance cards.");

            whyNot = sb.ToString();
            return whyNot.Length == 0;
        }

        public GameStanding DistributionStandingOpt;
        public GameStanding Standing;
        public PlayerIDType PlayerID;
        public Dictionary<PlayerIDType, GamePlayer> Players;

        public MapDetails Map;
        public GameSettings Settings;
        public Dictionary<PlayerIDType, TeammateOrders> TeammatesOrders;
        public List<CardInstance> Cards;
        public int CardsMustPlay;
        public Dictionary<PlayerIDType, PlayerIncome> Incomes;

        public PlayerIncome BaseIncome;
        public PlayerIncome EffectiveIncome;

        public List<GamePlayer> Opponents;
        public bool IsFFA; //if false, we're in a 1v1, 2v2, 3v3, etc.  If false, there are more than two entities still alive in the game.  A game can change from FFA to non-FFA as players are eliminated.
        //public Dictionary<PlayerIDType, Neighbor> Neighbors;
        public Dictionary<PlayerIDType, int> WeightedNeighbors;
        public HashSet<TerritoryIDType> AvoidTerritories = new HashSet<TerritoryIDType>(); //we're conducting some sort of operation here, such as a a blockade, so avoid attacking or deploying more here.
        private Stopwatch Timer;
        public List<string> Directives;
        private MapModels MapModel;
        private Dictionary<TerritoryIDType, double> StandingArmiesMean;
        private Dictionary<TerritoryIDType, double> StandingArmiesVariance;
        private Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>> StandingArmiesCorrelations;

        /// <inheritdoc/>
        public void Init(GameIDType gameID, PlayerIDType myPlayerID, Dictionary<PlayerIDType, GamePlayer> players, MapDetails map, 
            GameStanding distributionStanding, GameSettings gameSettings, int numberOfTurns, Dictionary<PlayerIDType, 
            PlayerIncome> incomes, GameOrder[] prevTurn, GameStanding latestTurnStanding, GameStanding previousTurnStanding, 
            Dictionary<PlayerIDType, TeammateOrders> teammatesOrders, List<CardInstance> cards, int cardsMustPlay, Stopwatch timer, List<string> directives)
        {
            this.DistributionStandingOpt = distributionStanding;
            this.Standing = latestTurnStanding;
            this.PlayerID = myPlayerID;
            this.Players = players;
            this.Map = map;
            this.Settings = gameSettings;
            this.TeammatesOrders = teammatesOrders;
            this.Cards = cards;
            this.CardsMustPlay = cardsMustPlay;
            this.Incomes = incomes;
            this.BaseIncome = Incomes[PlayerID];
            this.EffectiveIncome = this.BaseIncome.Clone();
            //this.Neighbors = players.Keys.ExceptOne(PlayerID).ConcatOne(TerritoryStanding.NeutralPlayerID).ToDictionary(o => o, o => new Neighbor(this, o));
            //this.Opponents = players.Values.Where(o => o.State == GamePlayerState.Playing && !IsTeammateOrUs(o.ID)).ToList();
            this.IsFFA = true; // Opponents.Count > 1 && (Opponents.Any(o => o.Team == PlayerInvite.NoTeam) || Opponents.GroupBy(o => o.Team).Count() > 1);
            //this.WeightedNeighbors = WeightNeighbors();
            this.Timer = timer;
            this.Directives = directives;

            AILog.Log("BotMain", "Snowbird initialized.  Starting at " + timer.Elapsed.TotalSeconds + " seconds");
        }

        public List<TerritoryIDType> GetPicks()
        {
            return new List<TerritoryIDType>();
        }

        public List<GameOrder> GetOrders()
        {
            /*var myTerritories = this.Standing.Territories.Values.Where((o) => o.OwnerPlayerID == this.PlayerID);
            var myIDs = myTerritories.Select(o => o.ID);
            var myDetails = myTerritories.Select((o) => this.Map.Territories[o.ID]);
            var possibleExpansions = myDetails.Select(o => o.ConnectedTo.Keys.Where(s => !myIDs.Contains(s)));
            var rewards = possibleExpansions..Select(o => o.Sum(s => this.Map.Territories[s].PartOfBonuses.Sum(t => this.Map.Bonuses[t].Amount)));*/
            return new List<GameOrder>();
        }

        public void TestParser()
        {
            // Test parser
            this.MapModel = new MapModels((MapIDType) 2);
            var territories = new List<TerritoryIDType>();
            this.StandingArmiesMean = this.MapModel.GetStandingArmyMean(territories, 1);
            this.StandingArmiesVariance = this.MapModel.GetStandingArmyVariances(territories, 1);
            this.StandingArmiesCorrelations = this.MapModel.GetStandingArmyCorrelations(new List<TerritoryIDType>(), 1);

            // define the mean vector
            var meanArr = this.StandingArmiesMean.Select(kvp => kvp.Value).ToArray();
            Vector<double> mu = DenseVector.OfArray(meanArr);

            // define the covariance matrix
            var corrBase = this.StandingArmiesCorrelations.Select(kvp => kvp.Value.Select(pvk => pvk.Value).ToArray()).ToArray();
            var gBase = new double[corrBase.Length, corrBase.Length];
            for (var i = 0; i < corrBase.Length; i++)
            {
                var iSig = this.StandingArmiesVariance[territories[i]];
                for (var j = 0; j < corrBase.Length; j++)
                {
                    var jSig = this.StandingArmiesVariance[territories[j]];
                    gBase[i, j] = corrBase[i][j] * iSig * jSig;
                }
            }
            Matrix<double> G = DenseMatrix.OfArray(gBase);

            // develop (equality-constraint) matrix A
            // For starters, we simply care that the vector sums to 1 (not explicitly worrying about positivity).
            var aBase = new double[1, mu.Count];
            for (var i = 0; i < mu.Count; i++)
            {
                aBase[0, i] = 1;
            }
            Matrix<double> A = DenseMatrix.OfArray(aBase);

            // define the solution vector
            var bBase = new double[1];
            bBase[0] = 1;
            Vector<double> b = DenseVector.OfArray(bBase);

            this.ComputeOptimalDistribution(A, b, mu, G);
        }

        private void ComputeOptimalDistribution(Matrix<double> A, Vector<double> b, Vector<double> mu, Matrix<double> G)
        {
            // Algorithm 16.4
            var m = A.RowCount;

            // define starting point, then form good starting value
            var xBarBase = new double[mu.Count];
            var yBarBase = new double[mu.Count];
            var lamBarBase = new double[mu.Count];

            for (var i = 0; i < mu.Count; i++)
            {
                if (i == 0)
                {
                    // satisfies KKT conditions since b
                    xBarBase[0] = 1;
                    yBarBase[0] = 0;
                    lamBarBase[0] = 0;
                }
                else if(i == 1)
                {
                    xBarBase[1] = 0;
                    yBarBase[1] = -1;
                    lamBarBase[1] = 0;
                }
                else
                {
                    // TODO take a clsoer look here
                    xBarBase[i] = yBarBase[i] = lamBarBase[i] = 0;
                }
            }

            Vector<double> xBar = DenseVector.OfArray(xBarBase);
            Vector<double> yBar = DenseVector.OfArray(yBarBase);
            Vector<double> lamBar = DenseVector.OfArray(lamBarBase);

            // find the affine scaling measures
            Matrix<double> Y = DenseMatrix.OfDiagonalArray(yBarBase);
            Matrix<double> Delta = DenseMatrix.OfDiagonalArray(yBarBase);
            Vector<double> e = DenseVector.OfEnumerable(mu.Select(val => 1.0)); // quick creation of the all 1 vector

            var cols = G.ColumnCount + Delta.ColumnCount + Y.ColumnCount;
            var rows = G.RowCount + A.RowCount + Y.RowCount;
            Matrix<double> newtonSystem = DenseMatrix.OfDiagonalArray(e.Select(val => -1.0).ToArray());
            newtonSystem.SetSubMatrix(0, 0, G);
            newtonSystem.SetSubMatrix(0, G.ColumnCount + Delta.ColumnCount, A.Transpose().Multiply(-1));
            newtonSystem.SetSubMatrix(G.RowCount, 0, A);
            newtonSystem.SetSubMatrix(G.RowCount + A.RowCount, G.ColumnCount, Delta);
            newtonSystem.SetSubMatrix(G.RowCount + A.RowCount, G.ColumnCount + Delta.ColumnCount, Y);

            // perform newton's method to get the affine values
            var compMeasure = yBar.DotProduct(lamBar) / m;
            var sigma = 0; // correct for the affine calculation per p. 484
            var rd = G * xBar - A.Transpose() * lamBar + mu;
            var rp = A * xBar - yBar - b;

            var newtonB = DenseVector.OfEnumerable(rd.Multiply(-1).Concat(rp.Multiply(-1)).Concat(Delta * (Y * e.Multiply(-1)) + e.Multiply(sigma * compMeasure)));

            // actually perform newtons method
            var affineTransforms = this.NewtonsMethod(newtonSystem, newtonB);


            // initialize variables
            var yBase = new double[xBar.Count];
            var lamBase = new double[xBar.Count];
            for (var i = 0; i < xBar.Count; i++)
            {
                yBase[i] = Math.Max(1.0, Math.Abs(yBar[i] + this.GetDeltaY(affineTransforms, xBar.Count)[i]));
                lamBase[i] = Math.Max(1.0, Math.Abs(lamBar[i] + this.GetDeltaLambda(affineTransforms, xBar.Count)[i]));
            }

            Vector<double> x;
            Vector<double> y;
            Vector<double> lambda;
            Vector<double> distance;
            var x_prev = xBar;
            var y_prev = DenseVector.OfArray(yBase);
            var lambda_prev = DenseVector.OfArray(lamBase); // update with correct affine index

            // run the algorithm until?
            var tol = 0.000001;
            do
            {
                x = x_prev;
                y = y_prev;
                lambda = lambda_prev;

                compMeasure = y.DotProduct(lambda) / m;

                // line search for alpha bar
                var alphaAffHat = 0;
                var compMeasureAff = (y + this.GetDeltaY(affineTransforms, xBar.Count).Multiply(alphaAffHat)).DotProduct(lambda + this.GetDeltaLambda(affineTransforms, xBar.Count).Multiply(alphaAffHat)) / m;
                var sig = Math.Pow(compMeasureAff / compMeasure, 3);
                // get the new deltas
                // minimize  a new function
                // update vector
                x_prev = x;
                // y_prev = y;
                // lambda_prev = lambda;

                distance = CreateVector.DenseOfEnumerable((x - x_prev).Concat(y - y_prev).Concat(lambda - lambda_prev));
            } while (distance.Norm(2) > tol);
        }

        private Vector<double> NewtonsMethod(Matrix<double> A, Vector<double> b)
        {
            // find the solution to Ax - b = 0
            // I'm not sure what the initial value should be
            var x = CreateVector.DenseOfEnumerable(A.Row(0).Select(entry => 0.0)); // initial guess is x = 0;
            Func<Vector<double>, Vector<double>> f = z => A * z - b;
            Func<Vector<double>, Matrix<double>> df = z =>
            {
                var retBase = new double[A.RowCount, A.ColumnCount];
                for (int i = 0; i < A.RowCount; i++)
                {
                    for (int j = 0; j < A.ColumnCount; j++)
                    {
                        retBase[i, j] = A[i, j] * z[j];
                    }
                }
                return DenseMatrix.OfArray(retBase);
            };
            var fx = f(x);
            var tol = 0.000001;
            while (fx.Norm(2) > tol)
            {
                x -= df(x).Inverse() * f(x); // right?
            }

            return x;
        }

        private Vector<double> GetDeltaX(Vector<double> tuple, int size)
        {
            return DenseVector.OfEnumerable(tuple.SubVector(0, size));
        }

        private Vector<double> GetDeltaY(Vector<double> tuple, int size)
        {
            return DenseVector.OfEnumerable(tuple.SubVector(size, size));
        }

        private Vector<double> GetDeltaLambda(Vector<double> tuple, int size)
        {
            return DenseVector.OfEnumerable(tuple.SubVector(2 * size, size));
        }
    }
}
