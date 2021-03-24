using BotDiscord.RPG.Anima;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BotDiscord.Modules
{
    public class AnimaModule : ModuleBase
    {
        [Command("!a roll"), Summary("Lance un Dé 100 avec jet ouvert")]
        public async Task AnimaRoll([Summary("Bonus à ajouter")]int num = 0, [Summary("Decription du lancé")]string desc = "")
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(Context.User.Mention
                + " rolled : "
                + AnimaDiceHelper.AnimaRoll(false, num).ResultText
                + (!string.IsNullOrEmpty(desc.Trim()) ? " (" + desc + ")" : "")
                );
        }

        [Command("!a new"), Summary("Lance les dés pour un nouveau perso. Caractéristique la plus basse n'est pas modifiée.")]
        public async Task New([Summary("Relance pour les valeurs inférieures ou égales")]int rerollVal = 0)
        {
            string msg = $"{Context.Message.Author.Mention}{Environment.NewLine}```Caractéristiques : ";
            List<int> caract = new List<int>();
            for (int i = 0; i < 8; i++)
            {
                caract.Add(AnimaDiceHelper.CaractRoll(rerollVal));
            }
            caract.Sort();
            msg += string.Join(" | ", caract);
            msg += $"{Environment.NewLine}Apparence : {AnimaDiceHelper.CaractRoll(0)}";
            msg += "```";
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(msg);
        }
    }
}
