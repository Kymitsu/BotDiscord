using System;
using System.Collections.Generic;
using System.Text;

namespace BotDiscord.RPG
{
    public abstract class PlayableCharacter
    {
        public string Player { get; set; }
        public string Name { get; set; }
        public bool IsCurrent { get; set; }
        public string ImageUrl { get; set; }
        public virtual List<RollableStat> AllStats { get; set; } = new List<RollableStat>();

        public abstract string KeywordsHelp();
    }
}
