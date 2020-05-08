﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public static class BotFactory
    {
        public static readonly string[] Names = new[] { "Wunderwaffe", "Prod", "ProdRandom", "Cowzow", "OptiProd" };

        public static IWarLightAI Construct(string name)
        {
            switch (name.ToLower())
            {
                case "wunderwaffe":
                    return new Wunderwaffe.Bot.BotMain();
                case "prod":
                    return new Prod.BotMain(false);
                case "prodrandom":
                    return new Prod.BotMain(true);
                case "cowzow":
                    return new Cowzow.Bot.CowzowBot();
                case "optiprod":
                    return new OptiProd.BotMain(false);
                default:
                    throw new Exception("No bot found named " + name + ", supported names are: " + Names.JoinStrings(", "));
            }
        }
    }
}
