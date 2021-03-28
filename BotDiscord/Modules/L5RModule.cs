using BotDiscord.RPG;
using BotDiscord.RPG.Anima;
using BotDiscord.RPG.L5R;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace BotDiscord.Modules
{
    [Group("")]
    public class L5RModule : ModuleBase
    {
        [Command("!l")]
        public async Task L5rRoll(params string[] s)
        {
            string expr = string.Concat(s);
            // 1b2n  2b  3n   2b1n
            int whiteDiceNum = 0;
            int blackDiceNum = 0;

            var guildEmotes = Context.Guild.Emotes;

            int bIndex = expr.IndexOf('b');
            if(bIndex != -1)
                int.TryParse(expr[bIndex - 1].ToString(), out whiteDiceNum);

            int nIndex = expr.IndexOf('n');
            if (nIndex != -1)
                int.TryParse(expr[nIndex - 1].ToString(), out blackDiceNum);

            string whiteDiceResult = "";
            for (int i = 0; i < whiteDiceNum; i++)
            {
                int roll = DiceHelper.SimpleRoll(12);
                whiteDiceResult += $"<:{L5RDiceHelper.WhiteDiceMapping[roll]}:{guildEmotes.FirstOrDefault(x => x.Name == L5RDiceHelper.WhiteDiceMapping[roll])?.Id}>";
            }
            string blackDiceResult = "";
            for (int i = 0; i < blackDiceNum; i++)
            {
                int roll = DiceHelper.SimpleRoll(6);
                blackDiceResult += $"<:{L5RDiceHelper.BlackDiceMapping[roll]}:{guildEmotes.FirstOrDefault(x => x.Name == L5RDiceHelper.BlackDiceMapping[roll])?.Id}>";
            }

            MessageReference msgRef = new MessageReference(Context.Message.Id);
            await Context.Channel.SendMessageAsync($"{whiteDiceResult}{blackDiceResult}", messageReference: msgRef);
        }
    }
}
