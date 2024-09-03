using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotDiscord.RPG
{
    internal class RollStatisticEntry
    {
        public RollableStat Stat { get; set; }
        public List<DiceResult> Rolls { get; set; }

    }
}
