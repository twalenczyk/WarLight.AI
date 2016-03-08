﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.AI.Prod.MakeOrders
{
    public class PossibleAttack
    {
        public TerritoryIDType From, To;
        public double DefenseImportance = 0;
        public double OffenseImportance = 0;
        private BotMain Bot;

        public PossibleAttack(BotMain bot, TerritoryIDType from, TerritoryIDType to)
        {
            From = from;
            To = to;
            Bot = bot;
        }

        public double HighestImportance
        {
            get { return Math.Max(DefenseImportance, OffenseImportance); }
        }

        public override string ToString()
        {
            return "From " + Bot.TerrString(From) + " to " + Bot.TerrString(To) + ".  DefenseImportance=" + DefenseImportance + " OffensiveImportance=" + OffenseImportance;
        }

        public void Weight(Dictionary<PlayerIDType, int> weightedNeighbors)
        {
            var opponentID = Bot.Standing.Territories[this.To].OwnerPlayerID;

            Assert.Fatal(opponentID != TerritoryStanding.NeutralPlayerID);
            Assert.Fatal(!Bot.IsTeammateOrUs(opponentID));
            Assert.Fatal(weightedNeighbors.ContainsKey(opponentID));

            //Seed the border weight with a lessened neighbor weight
            this.DefenseImportance = this.OffenseImportance = weightedNeighbors[opponentID] / 10.0;

            //Are we defending a bonus we control?
            foreach (var defendingBonus in Bot.Map.Territories[this.From].PartOfBonuses
                .Select(b => Bot.Map.Bonuses[b])
                .Where(b => Bot.PlayerControlsBonus(b)
                            && Bot.Map.Territories[this.To].PartOfBonuses
                            .Select(b2 => Bot.Map.Bonuses[b2])
                            .Any(b2 => !Bot.PlayerControlsBonus(b2))))
            {
                //Defend importance is bonus value * 10
                this.DefenseImportance += Bot.BonusValue(defendingBonus.ID) * 10.0;
            }

            //Would attacking break an opponents bonus?
            foreach (var attackingBonus in Bot.Map.Territories[this.To].PartOfBonuses
                .Select(b => Bot.Map.Bonuses[b])
                .Where(b => Bot.OpponentMightControlBonus(b)))
            {
                this.OffenseImportance += Bot.BonusValue(attackingBonus.ID) * (Bot.IsFFA ? 4.0 : 10);  //be conservative in FFAs, but aggressive in heads up.
            }

            //How is our current ratio
            var ourArmies = Bot.Standing.Territories[this.From].NumArmies.NumArmies;
            var theirArmies = Bot.Standing.Territories[this.To].NumArmies.DefensePower;
            var ratio = (double)theirArmies / (double)ourArmies;

            if (ourArmies + theirArmies < 10)
                ratio = 1; //Small numbers change so rapidly anyway that we just consider it equal.

            this.OffenseImportance *= ratio;
            this.DefenseImportance *= ratio;

            //AILog.Log("Returning " + possibleAttack.OffenseImportance + "," + possibleAttack.DefenseImportance);
        }

    }
}
