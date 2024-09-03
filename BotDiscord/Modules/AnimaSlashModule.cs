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
            List<int> caract = new List<int>();
            for (int i = 0; i < 8; i++)
            {
                caract.Add(AnimaDiceHelper.CaractRoll(reroll));
            }
            caract.Sort();
            
            await RespondAsync($"```Caractéristiques : {string.Join(" | ", caract)}{Environment.NewLine}Apparence : {AnimaDiceHelper.CaractRoll(0)}```");
        }
    }
}
