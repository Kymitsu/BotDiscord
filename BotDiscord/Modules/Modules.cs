using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Discord;
using BotDiscord.RPG;
using BotDiscord.RPG.Anima;

namespace BotDiscord.Modules
{
    [Group("")]
    public class TestModule : ModuleBase
    {
        private CommandService _commandService;
        private static DateTime _sessionStart = DateTime.MinValue;

        public TestModule(CommandService commandService)
        {
            _commandService = commandService;
        }

        [Command("!")]
        public async Task MultipleRoll(params string[] s)
        {
            string expr = string.Join("", s).Replace("!", "");

            var values = expr.Split('d', '+');
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
                results.Add(DiceHelper.SimpleRoll(size));
            }
            string msg = Context.User.Mention + $" ({expr}) : " + string.Join(" + ", results);
            if (values.Count() > 2)
            {
                msg += " + " + bonus;
            }
            msg += " = " + (results.Sum() + bonus);
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("!session start")]
        public async Task JdrSessionStart()
        {
            await Context.Message.DeleteAsync();
            if (_sessionStart == DateTime.MinValue)
            {
                _sessionStart = DateTime.Now;
                await Context.Channel.SendMessageAsync($"> **Début de séance**"); 
            }
            else
            {
                TimeSpan ellapsedTime = DateTime.Now - _sessionStart;
                await Context.Channel.SendMessageAsync($"> Séance en cours depuis {ellapsedTime.Hours}h {ellapsedTime.Minutes}m");
            }
        }

        [Command("!session end")]
        public async Task JdrSessionEnd()
        {
            await Context.Message.DeleteAsync();
            DateTime end = DateTime.Now;
            TimeSpan ellapsedTime = end - _sessionStart;
            await Context.Channel.SendMessageAsync($">>> **Fin de séance**{Environment.NewLine}Durée de la séance: {ellapsedTime.Hours}h {ellapsedTime.Minutes}m");
            _sessionStart = DateTime.MinValue;

            AnimaCharacterRepository.SaveLoadedCharacter();
        }

        [Command("!session stats")]
        public async Task SessionStats()
        {
            await Context.Message.DeleteAsync();
            List<AnimaCharacter> characters = AnimaCharacterRepository.animaCharacters.Where(x => x.IsCurrent).ToList();

            Dictionary<string, List<DiceResult>> allStatistics = new Dictionary<string, List<DiceResult>>();
            allStatistics = characters.SelectMany(x => x.RollStatistics)
                .ToLookup(kvp => kvp.Key, kvp => kvp.Value)
                .ToDictionary(group => group.Key, group => group.SelectMany(x => x).ToList());

            int longestStat = allStatistics.Keys.Select(x => x).Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format($"|{{0,-{longestStat}}}|{{1,-13}}|{{2,-10}}|{{3,-10}}|", "*Stat*", "*Nb rolls*", "*Moy*", "*Moy/dé*"));
            foreach (var kvp in allStatistics)
            {
                string stat = kvp.Key;
                int rolls = kvp.Value.Count;
                int mean = (int)(kvp.Value.Sum(x => x.DiceResults.Sum()) /kvp.Value.Count);
                int meanPerDice = (int)(kvp.Value.Sum(x => x.DiceResults.Sum()) / kvp.Value.Sum(x => x.DiceResults.Count));
                sb.AppendLine(string.Format($"|{{0,-{longestStat}}}|{{1,13}}|{{2,10}}|{{3,10}}|", stat, rolls, mean, meanPerDice));
            }

            await Context.Channel.SendMessageAsync($"```{sb.ToString()}```");

        }

        [Command("!r"), Summary("Lance un Dé")]
        public async Task Roll([Summary("Taille du Dé")]int dieSize, [Summary("Bonus à ajouter")]int bonus = 0)
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(Context.User.Mention + " rolled : " + DiceHelper.SimpleRoll(dieSize, bonus).ResultText);
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
            var messages = this.Context.Channel.GetMessagesAsync((int)amount + 1).Flatten();
            _ = messages.ForEachAsync(x => this.Context.Channel.DeleteMessageAsync(x.Id));

            const int delay = 5000;
            var m = await this.ReplyAsync($"Deleting messages... _This message will be deleted in {delay / 1000} seconds._");
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
                if (module.Name != "AudioModule")
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
                        helpLine += Environment.NewLine;
                    }
                    helpLine += Environment.NewLine;
                }
            }
            helpLine += "```";
            await Context.Channel.SendMessageAsync(helpLine);
        }
    }
}
