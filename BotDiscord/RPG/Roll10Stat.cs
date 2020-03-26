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

        public override DiceResult Roll(int temporaryBonus,Boolean destinFuneste)
        {
            return GenericTools.BaseStatRoll(Value + temporaryBonus);
        }
        public override DiceResult FailRoll(int score)
        {
            return GenericTools.FaillRoll(Group, Name, Value, score);

        }
    }
}
