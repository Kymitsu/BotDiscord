using System;
using System.Collections.Generic;
using System.Text;

namespace BotDiscord.RPG.L5R
{
    public class L5RStat : RollableStat
    {
        public L5RStat(string group, string name, int value) : base(group, name, value)
        {
        }

        public string Roll(int ring)
        {
            return L5RDiceHelper.Roll(this.Value, ring);
        }
    }
}
