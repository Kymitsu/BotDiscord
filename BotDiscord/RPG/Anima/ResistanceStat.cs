using System;
using System.Collections.Generic;
using System.Text;

namespace BotDiscord.RPG.Anima
{
    public class ResistanceStat : AnimaRollableStat
    {
        public ResistanceStat(string group, string name, int value) : base(group, name, value)
        {
        }

        public override DiceResult FailRoll(int score)
        {
            throw new NotImplementedException();
        }

        public override DiceResult Roll(int temporaryBonus, bool destinFuneste)
        {
            return DiceHelper.SimpleRoll(100, Value, temporaryBonus);
        }
    }
}
