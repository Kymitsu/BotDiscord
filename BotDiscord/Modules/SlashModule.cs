using BotDiscord.RPG;
using BotDiscord.RPG.Anima;
using BotDiscord.Services;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BotDiscord.Modules
{
    public class SlashModule : InteractionModuleBase<SocketInteractionContext>
    {
        private static DateTime _sessionStart = DateTime.MinValue;
        private readonly CharacterService _characterService;

        public SlashModule(CharacterService characterService)
        {
            _characterService = characterService;
        }

        [SlashCommand("test-command", "This is a test command")]
        public async Task Test()
        {
            await RespondAsync($"```ansi{Environment.NewLine}{DiscordFormater.CodeBlockColor("This is a test", DiscordFormats.Bold | DiscordFormats.TextCyan)}```");
        }
        
        [SlashCommand("r", "Lancer un ou plusieurs dés.")]
        public async Task Roll([Summary(description:"exemple: 2d12+3")] string expr)
        {
            _ = DeferAsync();

            int dIndex = expr.IndexOf('d');
            var diceNb = Convert.ToInt32(expr.Substring(0, dIndex));
            int bonusIndex = expr.IndexOfAny(['+', '-']);
            int dieSize, bonus;
            if(bonusIndex != -1)
            {
                dieSize = Convert.ToInt32(expr.Substring(dIndex + 1, bonusIndex - dIndex - 1));
                bonus = Convert.ToInt32(expr.Substring(bonusIndex));
            }
            else
            {
                dieSize = Convert.ToInt32(expr.Substring(dIndex + 1));
                bonus = 0;
            }
            
            List<int> result = new List<int>();

            for (int i = 0; i < diceNb; i++)
            {
                result.Add(DiceHelper.SimpleRoll(dieSize));
            }
            var test = new DiceResult(result, bonus);

            string msg = $"{Context.User.Mention} ({expr}) : {test.ResultText}";

            await DeleteOriginalResponseAsync();
            await Context.Channel.SendMessageAsync(msg);
        }

        [Group("session", "Session")]
        public class SessionSlashModule : InteractionModuleBase<SocketInteractionContext>
        {
            private readonly CharacterService _characterService;
            private static BlockMessage _statMessage;

            public SessionSlashModule(CharacterService characterService)
            {
                _characterService = characterService;
            }

            [SlashCommand("start", "Démarre une séance de JDR")]
            public async Task Start()
            {
                if (_sessionStart == DateTime.MinValue)
                {
                    _sessionStart = DateTime.Now;
                    await RespondAsync($"> **Début de séance** {Environment.NewLine}_N'oubliez pas de charger votre personnage!_");
                }
                else
                {
                    TimeSpan ellapsedTime = DateTime.Now - _sessionStart;
                    await RespondAsync($"> Séance en cours depuis {ellapsedTime.Hours}h{ellapsedTime.Minutes}");
                }
            }

            [SlashCommand("end", "Fin de la séance de JDR.")]
            public async Task End()
            {
                if (_sessionStart != DateTime.MinValue)
                {
                    List<AnimaCharacter> activeChar = _characterService.GetAllActiveCharacter<AnimaCharacter>();
                    BuildStatMessage(activeChar);
                    var builder = new ComponentBuilder()
                        .WithButton("-", "pagedmessage-minus")
                        .WithButton("+", "pagedmessage-plus");

                    await RespondAsync(_statMessage.GetCurrentPage(), components: builder.Build());

                    DateTime end = DateTime.Now;
                    TimeSpan ellapsedTime = end - _sessionStart;
                    await FollowupAsync($">>> **Fin de séance**{Environment.NewLine}Durée de la séance: {ellapsedTime.Hours}h{ellapsedTime.Minutes}");
                    _sessionStart = DateTime.MinValue;

                    _characterService.SaveLoadedCharacters();

                    _characterService.UnloadCharacters();
                }
                else
                {
                    await RespondAsync($"Aucune séance en cour", ephemeral:true);
                }
            }

            [SlashCommand("stats", "Affiche les stats de la session")]
            public async Task Stats()
            {
                await DeferAsync();

                List<AnimaCharacter> activeChar = _characterService.GetAllActiveCharacter<AnimaCharacter>();
                BuildStatMessage(activeChar);
                var builder = new ComponentBuilder()
                        .WithButton("◀", "pagedmessage-minus")
                        .WithButton("▶", "pagedmessage-plus");

                await ModifyOriginalResponseAsync(x => 
                { 
                    x.Content = _statMessage.GetCurrentPage();
                    x.Components = builder.Build();
                });
            }

            [ComponentInteraction("pagedmessage-*", true)]
            public async Task StatMessageButton(string action)
            {
                await DeferAsync();
                string temp;
                if (action == "plus")
                    temp = _statMessage.GetNextPage();
                else
                    temp = _statMessage.GetPreviousPage();

                var builder = new ComponentBuilder()
                        .WithButton("◀", "pagedmessage-minus")
                        .WithButton("▶", "pagedmessage-plus");

                await ModifyOriginalResponseAsync(x => 
                {
                    x.Content = temp;
                    x.Components = builder.Build();
                });
            }



            private void BuildStatMessage(List<AnimaCharacter> animaCharacters)
            {
                StringBuilder mainStatSb = new StringBuilder();
                List<StringBuilder> detailStats = new();
                

                int longestChar = animaCharacters.Select(x => x.Name).Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;
                int longestStat = animaCharacters.SelectMany(x => x.RollStatistics.Keys).Select(x => x.Name).Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;
                int longestString = Math.Max(longestChar, longestStat) + 2;
                const int defPad = 12;

                mainStatSb.AppendLine(DiscordFormater.CodeBlockColor(
                    string.Format($"|{{0,-{longestString}}}|{{1,-12}}|{{2,-12}}|{{3,-12}}|{{4,-12}}|{{5,-12}}|", "Perso", "Moyenne", "Nb lancé", "Jet ouvert", "Max", "Maladresse"),
                    DiscordFormats.Bold | DiscordFormats.Underline));


                foreach (AnimaCharacter character in animaCharacters)
                {
                    string charName = character.Name;

                    List<int> allDice = character.RollStatistics.Values.SelectMany(x => x.SelectMany(y => y.DiceResults)).ToList();
                    double mean = Math.Round(allDice.Sum() / (double)(allDice.Count), 1);
                    int nbDice = allDice.Count;
                    int charOpenRoll = 0;
                    int charMaxRoll = 0;
                    int charFailRoll = 0;

                    StringBuilder detailSb = new StringBuilder();
                    detailSb.AppendLine(DiscordFormater.CodeBlockColor(
                        string.Format($"|{{0,-{longestString}}}|{{1,-12}}|{{2,-12}}|{{3,-12}}|{{4,-12}}|{{5,-12}}|", charName, "Moyenne", "Nb lancé", "Jet ouvert", "Max", "Maladresse"),
                        DiscordFormats.Bold | DiscordFormats.Underline));
                    
                    foreach (var kvp in character.RollStatistics)
                    {
                        string stat = kvp.Key.Name;

                        var allStatDice = kvp.Value.SelectMany(x => x.DiceResults).ToList();
                        double statMean = Math.Round(allStatDice.Sum() / (double)allStatDice.Count, 1);
                        int statNbDice = allStatDice.Count;
                        int statOpenRoll = kvp.Value.Count(x => x.DiceResults.Count > 1);
                        charOpenRoll += statOpenRoll;
                        int statMaxRoll = kvp.Value.Select(x => x.DiceResults.Sum()).Max();
                        charMaxRoll = Math.Max(charMaxRoll, statMaxRoll);
                        int statFailRoll = kvp.Value.Count(x => x.DiceResults.Last() <= AnimaDiceHelper.CheckFailValue(character.IsLucky, character.IsUnlucky, kvp.Key.Value));
                        charFailRoll += statFailRoll;


                        detailSb.AppendLine(string.Format("|{0}|{1}|{2}|{3}|{4}|{5}|",
                            DiscordFormater.CodeBlockColor(string.Format($"{{0,-{longestString}}}", stat), DiscordFormats.Bold),
                            DiscordFormater.CodeBlockColor($"{statMean,defPad}", DiscordFormats.TextBlue),
                            DiscordFormater.CodeBlockColor($"{statNbDice,defPad}", DiscordFormats.TextBlue),
                            DiscordFormater.CodeBlockColor($"{statOpenRoll,defPad}", DiscordFormats.TextBlue),
                            DiscordFormater.CodeBlockColor($"{statMaxRoll,defPad}", DiscordFormats.TextBlue),
                            DiscordFormater.CodeBlockColor($"{statFailRoll,defPad}", DiscordFormats.TextBlue)));
                    }
                    detailStats.Add(detailSb);

                    mainStatSb.AppendLine(string.Format("|{0}|{1}|{2}|{3}|{4}|{5}|",
                            DiscordFormater.CodeBlockColor(string.Format($"{{0,-{longestString}}}", charName), DiscordFormats.Bold),
                            DiscordFormater.CodeBlockColor($"{mean,defPad}", DiscordFormats.TextBlue),
                            DiscordFormater.CodeBlockColor($"{nbDice,defPad}", DiscordFormats.TextBlue),
                            DiscordFormater.CodeBlockColor($"{charOpenRoll,defPad}", DiscordFormats.TextBlue),
                            DiscordFormater.CodeBlockColor($"{charMaxRoll,defPad}", DiscordFormats.TextBlue),
                            DiscordFormater.CodeBlockColor($"{charFailRoll,defPad}", DiscordFormats.TextBlue)));

                }
                _statMessage = new BlockMessage();
                _statMessage.AddPage($"```ansi{Environment.NewLine}{mainStatSb}```");
                foreach (var sb in detailStats)
                {
                    _statMessage.AddPage($"```ansi{Environment.NewLine}{sb}```");
                }

                //return $"```ansi{Environment.NewLine}{mainStatSb}{detailSb}```";
            }
        }
    }
}
