using System;
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
        private Dictionary<TerritoryIDType, Dictionary<TerritoryIDType, double>> StandingArmiesCovariances;

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
            var territories = new List<TerritoryIDType>(new TerritoryIDType[] { (TerritoryIDType)1, (TerritoryIDType)5, (TerritoryIDType)8 });
            this.StandingArmiesMean = this.MapModel.GetAttackPowerMeans(territories, 10);
            this.StandingArmiesVariance = this.MapModel.GetAttackPowerVariances(territories, 10);
            this.StandingArmiesCovariances = this.MapModel.GetAttackPowerCovariances(territories, 10);

            // define the mean vector
            Vector<double> mu = DenseVector.OfArray(this.StandingArmiesMean.Values.ToArray());

            // define the covariance matrix
            var corrBase = this.StandingArmiesCovariances.Select(kvp => kvp.Value.Select(pvk => pvk.Value).ToArray()).ToArray();
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
            var aBase = new double[G.RowCount + 1, G.RowCount];
            for (var i = 0; i < G.RowCount; i++)
            {
                for (var j = 0; j < G.RowCount; j++) {
                    if (i == 0)
                    {
                        aBase[0, j] = 1;
                    }
                    else
                    {
                        if (j == i-1)
                        {
                            aBase[i, j] = 1;
                        }
                        else
                        {
                            aBase[i, j] = 0;
                        }
                    }
                }
            }
            Matrix<double> A = DenseMatrix.OfArray(aBase);

            // define the solution vector
            var bBase = new double[A.RowCount];
            bBase[0] = 1;
            for (var i = 1;  i < A.RowCount; i++)
            {
                bBase[i] = 0;
            }
            Vector<double> b = DenseVector.OfArray(bBase);

            // generate a random non-singular matrix for testing
            var deploymentDistribution = this.ComputeOptimalDistribution(A, b, mu, G);
            var dict = deploymentDistribution.Zip(territories, (dist, terr) => new KeyValuePair<TerritoryIDType, double>(terr, dist)).ToDictionary(pair => pair.Key);

            // Convert the distribution to integer orders
            // submit orders.
        }

        private Vector<double> ComputeOptimalDistribution(Matrix<double> A, Vector<double> b, Vector<double> mu, Matrix<double> G)
        {
            // Algorithm 16.4
            var m = A.RowCount;

            // define starting point, then form good starting value
            var xBarBase = new double[mu.Count];
            var yBarBase = new double[A.RowCount];
            var lamBarBase = new double[A.RowCount];

            // TODO: improve initialization logic.
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
                    yBarBase[1] = 1;
                    lamBarBase[1] = 0;
                }
                else
                {
                    // TODO take a closer look here
                    xBarBase[i] = yBarBase[i] = lamBarBase[i] = 0;
                }
            }

            yBarBase[A.RowCount - 1] = 1;
            lamBarBase[A.RowCount - 1] = 0;

            Vector<double> xBar = CreateVector.Dense(mu.Count, 1.0 / mu.Count);
            Vector<double> yBar = CreateVector.Dense(A.RowCount, index => index == 0 ? 0 : 1.0 / mu.Count);
            Vector<double> lamBar = CreateVector.Dense(A.RowCount, index => index == 0 ? 1.0 : 0);

            // find the affine scaling measures
            Matrix<double> Y = DenseMatrix.OfDiagonalVector(yBar);
            Matrix<double> Lambda = DenseMatrix.OfDiagonalVector(lamBar);
            Vector<double> e = CreateVector.Dense(mu.Count, 1.0); // quick creation of the all 1 vector

            // Get ahold of the initial affine step values to determine a good starting point.
            var compMeasure = yBar.DotProduct(lamBar) / m;
            var sigma = 0; // correct for the affine calculation per p. 484
            var rd = G * xBar - A.Transpose() * lamBar + mu;
            var rp = A * xBar - yBar - b;

            var affineTransforms = this.GetScalingSteps(A, G, Lambda, Y, rd, rp, compMeasure, sigma);
            var deltaXAff = this.GetStep(affineTransforms, 0, xBar.Count);
            var deltaYAff = this.GetStep(affineTransforms, xBar.Count, yBar.Count);
            var deltaLambdaAff = this.GetStep(affineTransforms, xBar.Count + yBar.Count, lamBar.Count);

            // initialize variables

            var x_prev = xBar;
            var y_prev = CreateVector.Dense(yBar.Count(), index => Math.Max(1, Math.Abs(yBar[index] + deltaYAff[index])));
            var lambda_prev = CreateVector.Dense(lamBar.Count(), index => Math.Max(1, Math.Abs(lamBar[index] + deltaLambdaAff[index])));
            Vector<double> x = x_prev;
            Vector<double> y = y_prev;
            Vector<double> lambda = lambda_prev;
            Vector<double> distance;
            Vector<double> totalSteps;

            // run the algorithm until?
            var tol = 0.000001;
            do
            {

                // probably need to update rd rp compMeasure and sigma
                // get affine scaling steps
                affineTransforms = this.GetScalingSteps(A, G, Lambda, Y, rd, rp, compMeasure, sigma);
                deltaXAff = this.GetStep(affineTransforms, 0, xBar.Count);
                deltaYAff = this.GetStep(affineTransforms, xBar.Count, yBar.Count);
                deltaLambdaAff = this.GetStep(affineTransforms, xBar.Count + yBar.Count, lamBar.Count);

                compMeasure = y.DotProduct(lambda) / m;

                var alphaAffHat = this.MaxAlphaHatAff(y, lambda, deltaYAff, deltaLambdaAff, tol);
                var compMeasureAff = (y + deltaYAff.Multiply(alphaAffHat)).DotProduct(lambda + deltaLambdaAff.Multiply(alphaAffHat)) / m;
                var centeringParam = Math.Pow(compMeasureAff / compMeasure, 3);

                // Solve 16.67 for the new scaling steps
                // I simply need to update the solution vector
                totalSteps = this.GetTotalSteps(A, G, Lambda, Y, rd, rp, deltaLambdaAff, deltaYAff, compMeasure, centeringParam);

                // for simplicity, let taus be the same
                var tauk = 0.5; // TODO: refine this selection (e.g.  as iterates increase, tauk --> 1).
                var alphaTauPri = this.MaxAlphaTau(y, deltaYAff, tauk, tol);
                var alphaTauDual = this.MaxAlphaTau(lambda, deltaLambdaAff, tauk, tol);
                var alphaHat = Math.Min(alphaTauPri, alphaTauDual);

                // minimize  a new function
                // update vector

                var distanceX = this.GetStep(totalSteps, 0, xBar.Count).Multiply(alphaHat);
                var distanceY = this.GetStep(totalSteps, xBar.Count, yBar.Count).Multiply(alphaHat);
                var distanceLambda = this.GetStep(totalSteps, xBar.Count + yBar.Count, lamBar.Count).Multiply(alphaHat);

                x += distanceX;
                y += distanceY;
                lambda += distanceLambda;

                distance = CreateVector.DenseOfEnumerable(distanceX.Concat(distanceY).Concat(distanceLambda));
            } while (distance.Norm(2) > tol); // measures convergence

            return x;
        }

        private Vector<double> GetScalingSteps(
            Matrix<double> A, 
            Matrix<double> G, 
            Matrix<double> Lambda, 
            Matrix<double> Y, 
            Vector<double> rd, 
            Vector<double> rp,
            double mu, 
            double sigma)
        {
            Vector<double> e = CreateVector.Dense(Y.RowCount, 1.0); // quick creation of the all 1 vector
            
            // Create the system of equations to solve using Newton's method
            // TODO: Use the CreateMatrix.DenseOfMatrixArray(...) function for the below purpose
            var cols = G.ColumnCount + Lambda.ColumnCount + Y.ColumnCount;
            var rows = G.RowCount + A.RowCount + Y.RowCount;
            Matrix<double> newtonSystem = CreateMatrix.DenseDiagonal(rows, cols, -1.0);
            newtonSystem.SetSubMatrix(0, 0, G);
            newtonSystem.SetSubMatrix(0, G.ColumnCount + Lambda.ColumnCount, A.Transpose().Multiply(-1));
            newtonSystem.SetSubMatrix(G.RowCount, 0, A);
            newtonSystem.SetSubMatrix(G.RowCount + A.RowCount, G.ColumnCount, Lambda);
            newtonSystem.SetSubMatrix(G.RowCount + A.RowCount, G.ColumnCount + Lambda.ColumnCount, Y);

            // Establish the solution vector to Ax = b
            var newtonB = DenseVector.OfEnumerable(rd.Multiply(-1).Concat(rp.Multiply(-1)).Concat(Lambda * (Y * e.Multiply(-1)) + e.Multiply(sigma * mu)));

            // actually perform newtons method
            Vector<double> xStart = CreateVector.Dense(newtonSystem.ColumnCount, 1.0);
            Func<Vector<double>, Vector<double>> f = z => x1Norm(newtonSystem, newtonB, z);
            Func<Vector<double>, Matrix<double>> df = z => dx1Norm(newtonSystem, newtonB, z);
            var scalingSteps = this.NewtonsMethod(f, df, xStart);

            // need I validate the result?
            return scalingSteps;
        }

        private Vector<double> GetTotalSteps(
            Matrix<double> A,
            Matrix<double> G,
            Matrix<double> Lambda,
            Matrix<double> Y,
            Vector<double> rd,
            Vector<double> rp,
            Vector<double> lambdaAff,
            Vector<double> yAff,
            double mu,
            double sigma)
        {
            Matrix<double> DeltaLambdaAff = DenseMatrix.OfDiagonalVector(lambdaAff);
            Matrix<double> DeltaYAff = DenseMatrix.OfDiagonalVector(yAff);
            Vector<double> e = CreateVector.Dense(Y.RowCount, 1.0); // quick creation of the all 1 vector

            // Create the system of equations to solve using Newton's method
            // TODO: Use the CreateMatrix.DenseOfMatrixArray(...) function for the below purpose
            var cols = G.ColumnCount + Lambda.ColumnCount + Y.ColumnCount;
            var rows = G.RowCount + A.RowCount + Y.RowCount;
            Matrix<double> newtonSystem = CreateMatrix.DenseDiagonal(rows, cols, -1.0);
            newtonSystem.SetSubMatrix(0, 0, G);
            newtonSystem.SetSubMatrix(0, G.ColumnCount + Lambda.ColumnCount, A.Transpose().Multiply(-1));
            newtonSystem.SetSubMatrix(G.RowCount, 0, A);
            newtonSystem.SetSubMatrix(G.RowCount + A.RowCount, G.ColumnCount, Lambda);
            newtonSystem.SetSubMatrix(G.RowCount + A.RowCount, G.ColumnCount + Lambda.ColumnCount, Y);

            // Establish the solution vector to Ax = b
            var partC = e.Multiply(sigma * mu) - Lambda * (Y * e) - DeltaLambdaAff * (DeltaYAff * e);
            var newtonB = DenseVector.OfEnumerable(rd.Multiply(-1).Concat(rp.Multiply(-1)).Concat(partC));

            // set up initial variables and lambda functions for Newton's method
            Vector<double> xStart = CreateVector.Dense(newtonSystem.ColumnCount, 1.0);
            Func<Vector<double>, Vector<double>> f = z => x1Norm(newtonSystem, newtonB, z);
            Func<Vector<double>, Matrix<double>> df = z => dx1Norm(newtonSystem, newtonB, z);
            var scalingSteps = this.NewtonsMethod(f, df, xStart);

            // need I validate the result?
            return scalingSteps;
        }

        private Vector<double> NewtonsMethod(Func<Vector<double>, Vector<double>> f, Func<Vector<double>, Matrix<double>> df, Vector<double> xStart)
        {
            // find the solution to Ax - b = 0
            // I'm not sure what the initial value should be
            var x = xStart; // initial guess is x = 0;
            var fx = f(x);
            var dfx = df(x);
            var tol = 0.000001;
            while (fx.Norm(2) > tol)
            {
                Vector<double> deltaX = dfx.Solve(fx.Multiply(-1));
                x += deltaX;
                fx = f(x);
                dfx = df(x);
            }

            return x;
        }

        private double LineSearch(
            Func<double, double> daf,
            Func<double, double> ddaf,
            double alphaStart, 
            double lowerBound, 
            bool isLowerBoundInclusive,
            double upperBound, 
            bool isUpperBoundInclusive, 
            double tol)
        {
            // f in this context is the 2-norm of the gradient and we want to drive it to 0 to find the min of the negative of the composite vector
            // Recall that computing the maximum of a function is the same as computing the minimum of the negative of the function.
            // Perform a line search over the range (0,1] for the proper alpha.
            Func<double, bool> lowerComp = beta => isLowerBoundInclusive ? beta >= lowerBound : beta > lowerBound;
            Func<double, bool> upperComp = beta => isUpperBoundInclusive ? beta <= upperBound : beta < upperBound;

            var der = daf(alphaStart);
            var dder = ddaf(alphaStart);
            var alpha = alphaStart;
            while (der > tol && lowerComp(alpha) && upperComp(alpha))
            {
                alpha -= der / dder;
                der = daf(alpha);
                dder = ddaf(alpha);
            }

            return alpha;
        }

        private double MaxAlphaHatAff(Vector<double> y, Vector<double> lambda, Vector<double> yAff, Vector<double> lambdaAff, double tol)
        {
            var alpha = 0.01;
            var baseComponent = DenseVector.OfEnumerable(y.Concat(lambda)).Multiply(-1);
            var affComponent = DenseVector.OfEnumerable(yAff.Concat(lambdaAff)).Multiply(-1);
            Func<double, double> daf = beta => this.da2Norm(baseComponent, affComponent, beta);
            Func<double, double> ddaf = beta => this.dda2Norm(baseComponent, affComponent, beta);

            return this.LineSearch(daf, ddaf, alpha, 0, false, 1, true, tol);
        }

        private double MaxAlphaTau(Vector<double> v, Vector<double> deltaV, double tau, double tol)
        {
            var alpha = 0.01;
            var tauFactor = 1 - tau;
            var baseComp = DenseVector.OfEnumerable(v.Select(vi => vi - tauFactor * vi));

            // define derivate lambdas
            Func<double, double> daf = beta => this.da2Norm(baseComp, deltaV, beta);
            Func<double, double> ddaf = beta => this.dda2Norm(baseComp, deltaV, beta);

            return this.LineSearch(daf, ddaf, alpha, 0, false, 1, false, tol);
        }


        private double da2Norm(Vector<double> x, Vector<double> y, double alpha)
        {
            return 2 * (x.Zip(y, (xi, yi) => xi * yi).Sum() + y.Sum(yi => alpha * yi * yi));
        }

        private double dda2Norm(Vector<double> x, Vector<double> y, double alpha)
        {
            return 2 * y.Sum(yi => yi * yi);
        }

        private Vector<double> x1Norm(Matrix<double> A, Vector<double> b, Vector<double> x)
        {
            return A * x - b;
        }

        private Matrix<double> dx1Norm(Matrix<double> A, Vector<double> b, Vector<double> x)
        {
            var a = A.Determinant();
            if (a== 0)
            {
                // consider throwing an error
            }
            return A;
        }

        private Vector<double> GetStep(Vector<double> tuple, int index, int size)
        {
            return DenseVector.OfEnumerable(tuple.SubVector(index, size));
        }
    }
}
