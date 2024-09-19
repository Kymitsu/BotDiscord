using BotDiscord.RPG;
using BotDiscord.RPG.Anima;
using Discord.Interactions;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotDiscord.Modules
{
    [Group("anima", "Anima")]
    public class AnimaSlashModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("roll", "Lance un Dé 100 avec jet ouvert")]
        public async Task AnimaRoll([Summary(description:"Bonus à ajouter")] int bonus = 0, [Summary(description:"Decription du lancé")] string stat = "")
        {
            await DeferAsync();
            _ = DeleteOriginalResponseAsync();

            await Context.Channel.SendMessageAsync($"{Context.User.Mention} rolled : {AnimaDiceHelper.AnimaRoll(false, bonus).ResultText} {stat}");
        }

        [SlashCommand("new", "Lance les dés pour un nouveau perso. Caractéristique la plus basse n'est pas modifiée.")]
        public async Task New([Summary(description: "Relance pour les valeurs inférieures ou égales")]int reroll = 0)
        {
            await DeferAsync();
            if (reroll >= 11)
                reroll = 10;

            var msg = await ReplyAsync("\u200b");

            int tempCount = 0;

            int[] caract = new int[8];
            string[] caractString = new string[8];

            for (int i = 0; i < 8; i++)
            {
                bool continueRoll = true;
                while (continueRoll)
                {
                    caract[i] = caract[i] <= reroll ? DiceHelper.SimpleRoll(10) : caract[i];

                    DiscordFormats f;
                    if (caract[i] <= reroll)
                    {
                        f = DiscordFormats.Underline | DiscordFormats.TextRed;
                    }
                    else
                    {
                        f = DiscordFormats.Bold;
                        continueRoll = false;
                    }
                    caractString[i] = DiscordFormater.CodeBlockColor(caract[i], f);

                    tempCount++;
                    await msg.ModifyAsync(x => x.Content = $"```ansi{Environment.NewLine}[{tempCount}]Caractéristiques : {string.Join(" | ", caractString)}{Environment.NewLine}```");
                    await Task.Delay(500);
                }
            }

            await msg.ModifyAsync(x => x.Content = $"```ansi{Environment.NewLine}Caractéristiques : {string.Join(" | ", caractString)}{Environment.NewLine}Apparence : {DiceHelper.SimpleRoll(10)}```");

            await ModifyOriginalResponseAsync(x => x.Content = "Done!");
        }
    }
}
