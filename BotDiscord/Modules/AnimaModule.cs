using BotDiscord.RPG;
using BotDiscord.RPG.Anima;
using BotDiscord.Services;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotDiscord.Modules
{
    [Summary("Anima")]
    public class AnimaModule : ModuleBase
    {
        private readonly CharacterService _characterService;

        public AnimaModule(CharacterService characterService)
        {
            _characterService = characterService;
        }

        [Command("!a roll")]
        [Summary("Lance un Dé 100 avec jet ouvert `!a roll 15 vigilance`")]
        public async Task AnimaRoll([Summary("Bonus à ajouter")] int num = 0, [Summary("Decription du lancé")] string desc = "")
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(Context.User.Mention
                + " rolled : "
                + AnimaDiceHelper.AnimaRoll(false, num).ResultText
                + (!string.IsNullOrEmpty(desc.Trim()) ? " (" + desc + ")" : "")
                );
        }

        [Command("!a new")]
        [Summary("Lance les dés pour un nouveau perso. Caractéristique la plus basse n'est pas modifiée. Relance pour les valeurs inférieures ou égales `!a new 4`")]
        public async Task New(int rerollVal = 0)
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

        [Command("!c r")]
        [Summary("Lance les dés pour la stat passée en paramètre `!c r parade -30`")]
        public async Task Roll(params string[] s)
        {
            AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.Message.Author.Mention);
            if (character == null)
            {
                await Context.Channel.SendMessageAsync("Error 404: Character not found or not loaded!");
                return;
            }

            string statBonusStr = s.FirstOrDefault(x => x.StartsWith("+") || x.StartsWith("-"));
            int bonus = Convert.ToInt32(statBonusStr);
            string stat = string.Join(" ", s).ToLower();
            if (!string.IsNullOrWhiteSpace(statBonusStr))
            {
                stat = stat.Replace($" {statBonusStr}", "");
            }

            if (stat == null || stat == string.Empty)
            {
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync(character.KeywordsHelp());
            }
            else
            {
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync(string.Format("{0} {1}",
                    Context.User.Mention,
                    character.Roll(stat, bonus)));
            }
        }

        [Command("!c fight")]
        [Summary("Affiche un message pour lancer automatiquement des jets de combat")]
        public async Task FightMessage()
        {
            _ = Context.Message.DeleteAsync();
            var embed = new EmbedBuilder();
            embed.Title = "Vos actions de combat!";

            StringBuilder sb = new StringBuilder();
            foreach (var kvp in CommandHandlingService.EmotesAction)
            {
                sb.AppendLine($"{kvp.Key} : {kvp.Value}");
            }
            embed.Description = sb.ToString();

            var msg = await Context.Channel.SendMessageAsync("", false, embed.Build());

            CommandHandlingService.ReactionMessages.Add(msg.Id);

            foreach (Emoji emoji in CommandHandlingService.EmotesAction.Keys)
            {
                await msg.AddReactionAsync(emoji);
                await Task.Delay(1000);
            }
        }

        [Command("!c status")]
        [Alias("!c Status", "!c statut", "!c Statut")]
        [Summary("Envoi en MP le statut de ton personnage")]
        public async Task Status()
        {
            AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.Message.Author.Mention);
            if (character == null)
            {
                await Context.Channel.SendMessageAsync("Error 404: Character not found or not loaded!");
                return;
            }

            _ = Context.Message.DeleteAsync();
            var embed = new EmbedBuilder
            {
                Title = character.Name
            };
            embed.WithThumbnailUrl(character.ImageUrl)
                .AddField("Hp", $"{character.CurrentHp}/{character.Hp}", true)
                .AddField("Fatigue", $"{character.CurrentFatigue}/{character.Fatigue}", true)
                .AddField("Points de Ki", $"{character.CurrentKi}/{character.TotalKiPoints}", true)
                .AddField("Zéon", $"{character.CurrentZeon}/{character.ZeonPoints}", true)
                .AddField("Ppp libres", $"{character.CurrentPpp}/{character.PppFree}", true);

            await Context.User.SendMessageAsync("", false, embed.Build());
        }

        [Command("!c reset")]
        [Alias("!c Reset")]
        [Summary("Réinitialise toutes les stat du personnage")]
        public async Task ResetCurrentStat()
        {
            AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.Message.Author.Mention);
            _ = Context.Message.DeleteAsync();

            character.CurrentHp = character.Hp;
            character.CurrentFatigue = character.Fatigue;
            character.CurrentZeon = character.ZeonPoints;
            character.CurrentPpp = character.PppFree;
            character.CurrentKi = character.TotalKiPoints;

            var msg = await Context.Channel.SendMessageAsync($"{Context.User.Mention} {character.Name} reset");
            await Task.Delay(3000);
            _ = msg.DeleteAsync();
        }

        [Command("!c hp")]
        [Alias("!c HP", "!c Hp")]
        [Summary("Définit les HP actuels du personnage `!c hp 100` `!c hp -10` `!c hp reset`")]
        public async Task SetHp(string s)
        {
            AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.Message.Author.Mention);
            _ = Context.Message.DeleteAsync();

            if (s == "reset")
                character.CurrentHp = character.Hp;
            else if (s.Contains("+") || s.Contains("-"))
                character.CurrentHp += Convert.ToInt32(s);
            else
                character.CurrentHp = Convert.ToInt32(s);

            var msg = await Context.Channel.SendMessageAsync($"{Context.User.Mention} {character.Name}'s hp set to {character.CurrentHp}");
            await Task.Delay(3000);
            _ = msg.DeleteAsync();
        }

        [Command("!c fatigue")]
        [Alias("!c Fatigue")]
        [Summary("Définit les point de fatigue actuels du personnage `!c fatigue 6` `!c fatigue -1` `!c fatigue reset`")]
        public async Task SetFatigue(string s)
        {
            AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.Message.Author.Mention);
            _ = Context.Message.DeleteAsync();

            if (s == "reset")
                character.CurrentFatigue = character.Fatigue;
            else if (s.Contains("+") || s.Contains("-"))
                character.CurrentFatigue += Convert.ToInt32(s);
            else
                character.CurrentFatigue = Convert.ToInt32(s);

            var msg = await Context.Channel.SendMessageAsync($"{Context.User.Mention} {character.Name}'s fatigue set to {character.CurrentFatigue}");
            await Task.Delay(3000);
            _ = msg.DeleteAsync();
        }

        [Command("!c zéon")]
        [Alias("!c zeon", "!c Zéon", "!c Zeon")]
        [Summary("Définit le Zéon actuel du personnage `!c zéon 500` `!c zéon -80` `!c zéon reset`")]
        public async Task SetZeon(string s)
        {
            AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.Message.Author.Mention);
            _ = Context.Message.DeleteAsync();

            if (s == "reset")
                character.CurrentZeon = character.ZeonPoints;
            else if (s.Contains("+") || s.Contains("-"))
                character.CurrentZeon += Convert.ToInt32(s);
            else
                character.CurrentZeon = Convert.ToInt32(s);

            var msg = await Context.Channel.SendMessageAsync($"{Context.User.Mention} {character.Name}'s zeon set to {character.CurrentZeon}");
            await Task.Delay(3000);
            _ = msg.DeleteAsync();
        }

        [Command("!c ppp")]
        [Alias("!c Ppp")]
        [Summary("Définit les PPP actuels du personnage `!c ppp 4` `!c ppp +1` `!c ppp reset`")]
        public async Task SetPpp(string s)
        {
            AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.Message.Author.Mention);
            _ = Context.Message.DeleteAsync();

            if (s == "reset")
                character.CurrentPpp = character.PppFree;
            else if (s.Contains("+") || s.Contains("-"))
                character.CurrentPpp += Convert.ToInt32(s);
            else
                character.CurrentPpp = Convert.ToInt32(s);

            var msg = await Context.Channel.SendMessageAsync($"{Context.User.Mention} {character.Name}'s ppp set to {character.CurrentPpp}");
            await Task.Delay(3000);
            _ = msg.DeleteAsync();
        }

        [Command("!c ki")]
        [Alias("!c Ki")]
        [Summary("Définit le Ki actuels du personnage `!c ki 75` `!c ki -12` `!c ki reset`")]
        public async Task SetKi(string s)
        {
            AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.Message.Author.Mention);
            _ = Context.Message.DeleteAsync();

            if (s == "reset")
                character.CurrentKi = character.TotalKiPoints;
            else if (s.Contains("+") || s.Contains("-"))
                character.CurrentKi += Convert.ToInt32(s);
            else
                character.CurrentKi = Convert.ToInt32(s);

            var msg = await Context.Channel.SendMessageAsync($"{Context.User.Mention} {character.Name}'s ki set to {character.CurrentKi}");
            await Task.Delay(3000);
            _ = msg.DeleteAsync();
        }
    }
}
