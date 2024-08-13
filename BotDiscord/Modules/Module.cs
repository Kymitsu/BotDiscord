using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Discord;
using BotDiscord.RPG;
using BotDiscord;
using BotDiscord.Services;
using BotDiscord.RPG.Anima;

namespace BotDiscord.Modules
{
    [Summary("Général")]
    public class Module : ModuleBase<SocketCommandContext>
    {
        private CommandService _commandService;
        private readonly CharacterService _characterService;
        private static DateTime _sessionStart = DateTime.MinValue;

        public Module(CommandService commandService, CharacterService charService)
        {
            _commandService = commandService;
            _characterService = charService;
        }

        [Command("!test")]
        public async Task TestCommand()
        {
            
            await Context.Channel.SendMessageAsync($"{Context.User.Mention} Test command");
        }

        [Command("!")]
        [Summary("Lancer un ou plusieurs dés. `! 2d12+5`")]
        public async Task MultipleRoll(params string[] s)
        {
            string expr = string.Join("", s).Replace("!", "");

            var values = expr.Split('d', '+');
            int.TryParse(values[0], out int number);
            int.TryParse(values[1], out int size);
            int bonus = 0;
            if (values.Count() > 2)
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
        [Summary("Démarre une séance de JDR")]
        public async Task JdrSessionStart()
        {
            await Context.Message.DeleteAsync();
            if (_sessionStart == DateTime.MinValue)
            {
                _sessionStart = DateTime.Now;
                await Context.Channel.SendMessageAsync($"> **Début de séance** {Environment.NewLine}_N'oubliez pas de charger votre personnage!_");
            }
            else
            {
                TimeSpan ellapsedTime = DateTime.Now - _sessionStart;
                await Context.Channel.SendMessageAsync($"> Séance en cours depuis {ellapsedTime.Hours}h {ellapsedTime.Minutes}m");
            }
        }

        [Command("!session end")]
        [Summary("Fin de la séance de JDR.")]
        public async Task JdrSessionEnd()
        {
            await Context.Message.DeleteAsync();
            if (_sessionStart != DateTime.MinValue)
            {
                DateTime end = DateTime.Now;
                TimeSpan ellapsedTime = end - _sessionStart;
                await Context.Channel.SendMessageAsync($">>> **Fin de séance**{Environment.NewLine}Durée de la séance: {ellapsedTime.Hours}h {ellapsedTime.Minutes}m");
                _sessionStart = DateTime.MinValue;

                await Task.Run(_characterService.SaveLoadedCharacters);

                _characterService.UnloadCharacters();
            }
            else
            {
                await Context.Channel.SendMessageAsync($"Aucune séance en cour");
            }
        }

        [Command("!session stats")]
        [Summary("Affiche les statistiques de jets de dés (Anima uniquement)")]
        public async Task SessionStats()
        {
            await Context.Message.DeleteAsync();
            List<AnimaCharacter> characters = _characterService.Characters.OfType<AnimaCharacter>().Where(x => x.IsCurrent).ToList();

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
                int mean = kvp.Value.Sum(x => x.DiceResults.Sum()) / kvp.Value.Count;
                int meanPerDice = kvp.Value.Sum(x => x.DiceResults.Sum()) / kvp.Value.Sum(x => x.DiceResults.Count);
                sb.AppendLine(string.Format($"|{{0,-{longestStat}}}|{{1,13}}|{{2,10}}|{{3,10}}|", stat, rolls, mean, meanPerDice));
            }

            await Context.Channel.SendMessageAsync($"```{sb.ToString()}```");
            characters.ForEach(x => x.RollStatistics = new Dictionary<string, List<DiceResult>>());
        }

        [Command("!r")]
        [Summary("Lance un dé.`!r 10 3`")]
        public async Task Roll([Summary("Taille du Dé")] int dieSize, [Summary("Bonus à ajouter")] int bonus = 0)
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(Context.User.Mention + " rolled : " + DiceHelper.SimpleRoll(dieSize, bonus).ResultText);
        }

        [Command("!me")]
        [Summary("Pour t'aider dans ton RP parce qu'un autre joueur parle trop")]
        public async Task Me(params string[] s)
        {
            string text = string.Join(" ", s);
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync($"***{Context.User.Mention} {text}***");
        }

        [Command("!purge"), Summary("Purge un Channel (Admin uniquement)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Purge([Summary("Nb de msg à suppr")] int amount)
        {
            var messages = Context.Channel.GetMessagesAsync(amount + 1).Flatten();
            _ = messages.ForEachAsync(x => Context.Channel.DeleteMessageAsync(x.Id));

            const int delay = 5000;
            var m = await ReplyAsync($"Deleting messages... _This message will be deleted in {delay / 1000} seconds._");
            await Task.Delay(delay);
            await m.DeleteAsync();
        }

        [Command("!help")]
        [Summary("Liste de toutes les commandes")]
        public async Task Help(string category = "")
        {
            await Context.Message.DeleteAsync();
            HelpEmbed helpEmbeds = new HelpEmbed(_commandService.Modules);

            switch (category.ToLower())
            {
                case "général":
                    helpEmbeds.CurrentPage = 2;
                    break;
                case "personnages":
                    helpEmbeds.CurrentPage = 3;
                    break;
                case "anima":
                    helpEmbeds.CurrentPage = 4;
                    break;
                case "l5r":
                    helpEmbeds.CurrentPage = 5;
                    break;
                case "audio":
                    helpEmbeds.CurrentPage = 6;
                    break;
            }

            var msg = await Context.Channel.SendMessageAsync(embed: helpEmbeds.GetCurrentPage().Build());
            CommandHandlingService.HelpMessages.Add(msg.Id, helpEmbeds);
            await msg.AddReactionAsync(new Emoji("\U000025c0"));
            await msg.AddReactionAsync(new Emoji("\U000025b6"));
        }
    }
}
