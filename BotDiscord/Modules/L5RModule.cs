using BotDiscord.RPG;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using BotDiscord.RPG.L5R;
using BotDiscord.Services;

namespace BotDiscord.Modules
{
    [Summary("L5R")]
    public class L5RModule : ModuleBase
    {
        private readonly CharacterService _characterService;

        public L5RModule(CharacterService characterService)
        {
            _characterService = characterService;
        }

        [Command("!l")]
        [Summary("Lance des dés blancs et noirs de L5R `!l 2b3n`")]
        public async Task L5rRoll(params string[] s)
        {
            string expr = string.Concat(s);
            // 1b2n  2b  3n   2b1n
            int whiteDiceNum = 0;
            int blackDiceNum = 0;

            L5RDiceHelper.GuildEmotes = Context.Guild.Emotes;

            int bIndex = expr.IndexOf('b');
            if (bIndex != -1)
                int.TryParse(expr[bIndex - 1].ToString(), out whiteDiceNum);

            int nIndex = expr.IndexOf('n');
            if (nIndex != -1)
                int.TryParse(expr[nIndex - 1].ToString(), out blackDiceNum);

            MessageReference msgRef = new MessageReference(Context.Message.Id);
            await Context.Channel.SendMessageAsync(L5RDiceHelper.Roll(whiteDiceNum, blackDiceNum), messageReference: msgRef);
        }

        [Command("!l r")]
        [Priority(1)]
        [Summary("Lance les dés pour la stat passée en paramètre `!l r théologie`")]
        public async Task CharacterRoll(params string[] s)
        {
            L5RDiceHelper.GuildEmotes = Context.Guild.Emotes;

            L5RCharacter character = _characterService.FindCurrentByMention<L5RCharacter>(Context.Message.Author.Mention);
            if (character == null)
            {
                await Context.Channel.SendMessageAsync("Error 404: Character not found or not loaded!");
                return;
            }

            string ring = "";
            if (character.Rings.ContainsKey(s.Last()))
                ring = s.Last();

            string rawStat = string.Join(" ", s).ToLower();
            if (!string.IsNullOrWhiteSpace(ring))
            {
                rawStat = rawStat.Replace($" {ring}", "");
            }

            if (rawStat == null || rawStat == string.Empty)
            {
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync(character.KeywordsHelp());
            }
            else
            {
                MessageReference msgRef = new MessageReference(Context.Message.Id);
                await Context.Channel.SendMessageAsync(character.Roll(rawStat, ring), messageReference: msgRef);
            }
        }

        [Command("!l posture")]
        [Alias("!l stance", "!l p")]
        [Summary("Change la posture utilisée en combat `!l p feu`")]
        [Priority(1)]
        public async Task SetCharacterStance(string s = "")
        {
            _ = Context.Message.DeleteAsync();
            L5RCharacter character = _characterService.FindCurrentByMention<L5RCharacter>(Context.Message.Author.Mention);
            if (character == null)
            {
                await Context.Channel.SendMessageAsync("Error 404: Character not found or not loaded!");
                return;
            }

            try
            {
                character.SetCurrentStance(s);
            }
            catch (Exception ex)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} {ex.Message}");
            }

            var msg = await Context.Channel.SendMessageAsync($"{Context.User.Mention} Posture modifiée!");
            await Task.Delay(3000);
            _ = msg.DeleteAsync();
        }
    }
}
