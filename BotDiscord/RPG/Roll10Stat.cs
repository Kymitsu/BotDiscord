using System;
using System.Collections.Generic;
using System.Text;

namespace BotDiscord.RPG
{
    public class Roll10Stat : RollableStat
    {
        public Roll10Stat(string group, string name, int value) : base(group, name, value)
        {
        }

        public override DiceResult Roll()
        {
            return GenericTools.SimpleRoll(10, Value);
        }
    }
}
