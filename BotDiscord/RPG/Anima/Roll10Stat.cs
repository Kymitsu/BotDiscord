using System;
using System.Collections.Generic;
using System.Text;

namespace BotDiscord.RPG.Anima
{
    public class Roll10Stat : AnimaRollableStat
    {
        public Roll10Stat(string group, string name, int value) : base(group, name, value)
        {
        }

        public override DiceResult Roll(int temporaryBonus, Boolean destinFuneste)
        {
            return AnimaDiceHelper.BaseStatRoll(Value, temporaryBonus);
        }
        public override DiceResult FailRoll(int score)
        {
            throw new NotImplementedException();

        }
    }
}
