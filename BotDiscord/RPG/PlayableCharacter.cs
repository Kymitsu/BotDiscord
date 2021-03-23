using System;
using System.Collections.Generic;
using System.Text;

namespace BotDiscord.RPG
{
    public class PlayableCharacter
    {
        public string Player { get; set; }
        public string Name { get; set; }
        public bool IsCurrent { get; set; }
        public string ImageUrl { get; set; }

    }
}
