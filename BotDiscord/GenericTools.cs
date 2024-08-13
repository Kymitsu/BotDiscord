using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Http;
using OfficeOpenXml;
using System.Linq;
using BotDiscord.RPG.Anima;
using BotDiscord.RPG.L5R;
using System.Threading.Tasks;
using BotDiscord.RPG;

namespace BotDiscord
{
    public static class GenericTools
    {

        public static T FindByRawStat<T>(this List<T> list, string rawStat) where T : RollableStat
        {
            return list.First(x => x.Name.ToLower() == rawStat.ToLower() || x.Aliases.Any(y => y.ToLower() == rawStat.ToLower()));
        }

        
    }


}
