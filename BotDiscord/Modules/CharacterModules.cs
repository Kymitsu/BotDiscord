using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotDiscord.Modules
{
    [Group("!character")]
    public class CharacterModules : ModuleBase
    {
        [Command("")]
        public async Task Help()
        {
            await Context.Channel.SendMessageAsync("Upload, Info");
        }

        [Command("upload"), Summary("Charge les données d'un personnage depuis sa feuille Excel")]
        public async Task Upload()
        {
            //if (Context.Message.Attachments.Any())
            //{
            //    GenericTools.HandleFile(Context.Message.Attachments.First());
            //}
            //else
            //{
            //    await Context.Channel.SendMessageAsync("File not found. Please upload an Excel file while using this command.");
            //}
            await Context.Channel.SendMessageAsync("Command Not Implemented Yet.");
        }

        [Command("info"), Summary("Informations sur le personnage")]
        public async Task Info()
        {
            await Context.Channel.SendMessageAsync("Command Not Implemented Yet.");
        }
    }
}
