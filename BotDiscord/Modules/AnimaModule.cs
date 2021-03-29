using BotDiscord.RPG.Anima;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotDiscord.Modules
{
    [Group("")]
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

        [Command("!c r"), Summary("Lance les dées pour la stat passée en paramètre")]
        public async Task Roll(params string[] s)
        {
            AnimaCharacter character = CharacterRepository.FindCurrentByMention<AnimaCharacter>(Context.Message.Author.Mention);
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

        [Command("!c Status")]
        [Alias("!c status", "!c statut", "!c Statut")]
        public async Task Status()
        {
            AnimaCharacter character = CharacterRepository.FindCurrentByMention<AnimaCharacter>(Context.Message.Author.Mention);
            if (character == null)
            {
                await Context.Channel.SendMessageAsync("Error 404: Character not found or not loaded!");
                return;
            }

            _ = Context.Message.DeleteAsync();
            var embed = new EmbedBuilder
            {
                Title = "Status"
            };
            embed.WithAuthor(character.Name)
                .WithThumbnailUrl(character.ImageUrl)
                .AddField("Hp", $"{character.CurrentHp}/{character.Hp}", true)
                .AddField("Fatigue", $"{character.CurrentFatigue}/{character.Fatigue}", true)
                .AddField("Points de Ki", $"{character.CurrentKi}/{character.TotalKiPoints}", false)
                .AddField("Zéon", $"{character.CurrentZeon}/{character.ZeonPoints}", true)
                .AddField("Ppp libres", $"{character.CurrentPpp}/{character.PppFree}", true);

            await Context.User.SendMessageAsync("", false, embed.Build());
        }

        [Command("!c reset")]
        [Alias("!c Reset")]
        public async Task ResetCurrentStat()
        {
            AnimaCharacter character = CharacterRepository.FindCurrentByMention<AnimaCharacter>(Context.Message.Author.Mention);
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
        public async Task SetHp(string s)
        {
            AnimaCharacter character = CharacterRepository.FindCurrentByMention<AnimaCharacter>(Context.Message.Author.Mention);
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
        public async Task SetFatigue(string s)
        {
            AnimaCharacter character = CharacterRepository.FindCurrentByMention<AnimaCharacter>(Context.Message.Author.Mention);
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
        public async Task SetZeon(string s)
        {
            AnimaCharacter character = CharacterRepository.FindCurrentByMention<AnimaCharacter>(Context.Message.Author.Mention);
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
        public async Task SetPpp(string s)
        {
            AnimaCharacter character = CharacterRepository.FindCurrentByMention<AnimaCharacter>(Context.Message.Author.Mention);
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
        public async Task SetKi(string s)
        {
            AnimaCharacter character = CharacterRepository.FindCurrentByMention<AnimaCharacter>(Context.Message.Author.Mention);
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
