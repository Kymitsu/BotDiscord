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

        [Command("!")]
        public async Task MultipleRoll(string exrp)
        {
            var values = exrp.Split('d', '+');
            int.TryParse(values[0], out int number);
            int.TryParse(values[1], out int size);
            int bonus = 0;
            if (values.Count() >2)
            {
                int.TryParse(values[2], out bonus); 
            }

            List<int> results = new List<int>();

            for (int i = 0; i < number; i++)
            {
                results.Add(GenericTools.SimpleRoll(size).Total);
            }
            string msg = Context.User.Mention + $" ({exrp}) : " + string.Join(" + ", results);
            if (values.Count() > 2)
            {
                msg += " + " + bonus;
            }
            msg += " = " + (results.Sum() + bonus);
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("!ar"), Summary("Lance un Dé 100 avec jet ouvert")]
        public async Task AnimaRoll([Summary("Bonus à ajouter")]int num = 0, [Summary("Decription du lancé")]string desc = "")
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(Context.User.Mention 
                + " rolled : " 
                + GenericTools.AnimaRoll(false,num).ResultText 
                + (!string.IsNullOrEmpty(desc.Trim())? " (" + desc + ")":"")
                );
        }

        [Command("!r"), Summary("Lance un Dé")]
        public async Task Roll([Summary("Taille du Dé")]int dieSize, [Summary("Bonus à ajouter")]int bonus = 0)
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(Context.User.Mention + " rolled : " + GenericTools.SimpleRoll(dieSize, bonus).ResultText);
        }

        [Command("!me"), Summary("Pour t'aider dans ton RP parce qu'un autre joueur parle trop")]
        public async Task Me(params string[] s)
        {
            string text = string.Join(" ", s);
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync($"***{Context.User.Mention} {text}***");
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
            string helpLine = "```";
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
            helpLine += "```";
            await Context.Channel.SendMessageAsync(helpLine);
        }
    }
}
