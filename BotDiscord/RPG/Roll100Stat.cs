using System;
using System.Collections.Generic;
using System.Text;

namespace BotDiscord.RPG
{
    public class Roll100Stat : RollableStat
    {
        public Roll100Stat(string group, string name, int value) : base(group, name, value)
        {
        }

        public override DiceResult Roll(int temporaryBonus, Boolean destinFuneste)
        {
            return GenericTools.AnimaRoll(Group,destinFuneste, Value + temporaryBonus);
        }
        public override DiceResult FailRoll(int score)
        {
            return GenericTools.FaillRoll(Group, Name, Value, score);

        }
    }
}
