using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotDiscord.RPG
{
    public static class AnimaCharacterRepository
    {
        public static List<AnimaCharacter> animaCharacters = new List<AnimaCharacter>();

        public static void SerializeToJson(object o)
        {
            string jsonData = JsonConvert.SerializeObject(o, Formatting.Indented);

            //Response.Write(jsonData);
        }

        public static void SaveCharacters()
        {
            SerializeToJson(animaCharacters);
        }
    }
}
