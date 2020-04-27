using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        /// <inheritdoc/>
        public void Init(GameIDType gameID, PlayerIDType myPlayerID, Dictionary<PlayerIDType, GamePlayer> players, MapDetails map, 
            GameStanding distributionStanding, GameSettings gameSettings, int numberOfTurns, Dictionary<PlayerIDType, 
            PlayerIncome> incomes, GameOrder[] prevTurn, GameStanding latestTurnStanding, GameStanding previousTurnStanding, 
            Dictionary<PlayerIDType, TeammateOrders> teammatesOrders, List<CardInstance> cards, int cardsMustPlay, Stopwatch timer, List<string> directives)
        {

        }

        public List<TerritoryIDType> GetPicks()
        {
            return new List<TerritoryIDType>();
        }

        public List<GameOrder> GetOrders()
        {
            return new List<GameOrder>();
        }
    }
}
