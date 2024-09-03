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

        [RequireOwner]
        [SlashCommand("test-respond", "This is a test slash command")]
        public async Task TestRespond()
        {

            await RespondAsync("ayo!!");
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

            //string msg = $"{Context.User.Mention} ({expr}) : {string.Join(" + ", result)}{(bonusIndex != -1 ? $" + {bonus}": "")} = {result.Sum() + bonus}";
            string msg = $"{Context.User.Mention} ({expr}) : {test.ResultText}";

            await DeleteOriginalResponseAsync();
            await Context.Channel.SendMessageAsync(msg);
        }

        [Group("session", "Session")]
        public class SessionSlashModule : InteractionModuleBase<SocketInteractionContext>
        {
            private readonly CharacterService _characterService;

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
                    DateTime end = DateTime.Now;
                    TimeSpan ellapsedTime = end - _sessionStart;
                    await RespondAsync($">>> **Fin de séance**{Environment.NewLine}Durée de la séance: {ellapsedTime.Hours}h{ellapsedTime.Minutes}");
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
                StringBuilder mainStatSb = new StringBuilder();
                StringBuilder detailSb = new StringBuilder();

                int longestChar = activeChar.Select(x => x.Name).Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;
                int longestStat = activeChar.SelectMany(x => x.RollStatistics.Keys).Select(x => x.Name).Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;
                int longestString = Math.Max(longestChar, longestStat);

                mainStatSb.AppendLine(string.Format($"|{{0,-{longestString}}}|{{1,-10}}|{{2,-10}}|{{3,-10}}|{{4,-10}}|{{5,-10}}|", "Perso", "Moyenne", "Nb lancé", "Jet ouvert", "Max", "Maladresse"));
                

                foreach (AnimaCharacter character in activeChar)
                {
                    string charName = character.Name;

                    List<int> allDice = character.RollStatistics.Values.SelectMany(x => x.SelectMany(y => y.DiceResults)).ToList();
                    double mean = Math.Round(allDice.Sum() / (double)(allDice.Count), 1);
                    int nbDice = allDice.Count;
                    int charOpenRoll = 0;
                    int charMaxRoll = 0;
                    int charFailRoll = 0;

                    detailSb.AppendLine();
                    detailSb.AppendLine(string.Format($"|{{0,-{longestString}}}|{{1,-10}}|{{2,-10}}|{{3,-10}}|{{4,-10}}|{{5,-10}}|", charName, "Moyenne", "Nb lancé", "Jet ouvert", "Max", "Maladresse"));
                    foreach (var kvp in character.RollStatistics)
                    {
                        string stat = kvp.Key.Name;

                        var allStatDice = kvp.Value.SelectMany(x => x.DiceResults).ToList();
                        double statMean = Math.Round(allStatDice.Sum()/(double)allStatDice.Count, 1);
                        int statNbDice = allStatDice.Count;
                        int statOpenRoll = kvp.Value.Count(x => x.DiceResults.Count > 1);
                        charOpenRoll += statOpenRoll;
                        int statMaxRoll = kvp.Value.Select(x => x.DiceResults.Sum()).Max();
                        charMaxRoll = Math.Max(charMaxRoll, statMaxRoll);
                        int statFailRoll = kvp.Value.Count(x => x.DiceResults.Last() <= AnimaDiceHelper.CheckFailValue(character.IsLucky, character.IsUnlucky, kvp.Key.Value));
                        charFailRoll += statFailRoll;

                        detailSb.AppendLine(string.Format($"|{{0,-{longestString}}}|{{1,10}}|{{2,10}}|{{3,10}}|{{4,10}}|{{5,10}}|", 
                            stat,
                            statMean,
                            statNbDice,
                            statOpenRoll,
                            statMaxRoll,
                            statFailRoll));

                    }

                    mainStatSb.AppendLine(string.Format($"|{{0,-{longestString}}}|{{1,10}}|{{2,10}}|{{3,10}}|{{4,10}}|{{5,10}}|",
                        charName,
                        mean,
                        nbDice,
                        charOpenRoll,
                        charMaxRoll,
                        charFailRoll));

                }

                await ModifyOriginalResponseAsync(x => x.Content = $"```xl{Environment.NewLine}{mainStatSb}{detailSb}```");
            }
        }
    }
}
