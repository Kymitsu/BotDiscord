using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Discord;

namespace BotDiscord.Modules
{
    [Group("")]
    public class TestModule : ModuleBase
    {
        private CommandService _commandService;

        public TestModule(CommandService commandService)
        {
            _commandService = commandService;
        }

        [Command("!aroll"), Summary("Lance un Dé 100 avec jet ouvert")]
        public async Task AnimaRoll([Summary("Bonus à ajouter")]int num = 0, [Summary("Decription du lancé")]string desc = "")
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(Context.User.Mention 
                + " rolled : " 
                + GenericTools.AnimaRoll(num).ResultText 
                + (!string.IsNullOrEmpty(desc.Trim())? " (" + desc + ")":"")
                );
        }

        [Command("!roll"), Summary("Lance un Dé")]
        public async Task Roll([Summary("Taille du Dé")]int dieSize, [Summary("Bonus à ajouter")]int bonus = 0)
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(Context.User.Mention + " rolled : " + GenericTools.SimpleRoll(dieSize, bonus).ResultText);
        }

        [Command("!purge"), Summary("Purge un Channel (Admin uniquement)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Purge([Summary("Nb de msg à suppr")]int amount)
        {
            var messages = await this.Context.Channel.GetMessagesAsync((int)amount + 1).Flatten();

            await this.Context.Channel.DeleteMessagesAsync(messages);
            const int delay = 5000;
            var m = await this.ReplyAsync($"Purge completed. _This message will be deleted in {delay / 1000} seconds._");
            await Task.Delay(delay);
            await m.DeleteAsync();
        }

        [Command("!help"), Summary("Liste de toutes les commandes")]
        public async Task Help()
        {
            await Context.Message.DeleteAsync();
            string helpLine = "`";
            foreach (var module in _commandService.Modules)
            {
                string moduleName = module.Name;
                moduleName += moduleName != string.Empty ? " " : string.Empty;
                foreach (var command in module.Commands)
                {
                    helpLine += string.Format("{2}{0} \t {1} : Ex => {2}{0} ", command.Name, command.Summary, moduleName);
                    foreach (var parameter in command.Parameters)
                    {
                        helpLine += "[" + parameter.Summary + "] ";
                    }
                    helpLine += "\n";
                }
            }
            helpLine += "`";
            await Context.Channel.SendMessageAsync(helpLine);
        }
    }
}
