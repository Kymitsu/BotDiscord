using System;
using System.Collections.Generic;
using System.Text;

namespace BotDiscord.RPG.Anima
{
    public abstract class AnimaRollableStat : RollableStat
    {
        public AnimaRollableStat(string group, string name, int value) : base(group, name, value)
        {
        }

        public abstract DiceResult Roll(int temporaryBonus, Boolean destinFuneste);
        public abstract DiceResult FailRoll(int score);
    }
}
