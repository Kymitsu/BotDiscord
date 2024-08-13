using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;
using BotDiscord.RPG;
using Discord.Rest;
using BotDiscord;
using BotDiscord.RPG.Anima;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BotDiscord.Services
{
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private IServiceProvider _provider;
        private readonly CharacterService _characterService;
        private readonly ILogger _logger;

        public static Dictionary<IEmote, string> EmotesAction = new Dictionary<IEmote, string>();
        public static List<ulong> ReactionMessages { get; set; } = new List<ulong>();
        public static Dictionary<ulong, HelpEmbed> HelpMessages { get; set; } = new Dictionary<ulong, HelpEmbed>();

        public CommandHandlingService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            _characterService = _provider.GetRequiredService<CharacterService>();
            _logger = provider.GetRequiredService<ILogger<CommandHandlingService>>();
            

            _discord.MessageReceived += MessageReceived;
            _discord.ReactionAdded += ReactionAddedOrRemoved;
            _discord.ReactionRemoved += ReactionAddedOrRemoved;
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            _provider = provider;
            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), provider);
            // Add additional initialization code here...

            EmotesAction.Add(new Emoji("🏃‍♂️"), "Initiative");
            EmotesAction.Add(new Emoji("⚔️"), "Attaque");
            EmotesAction.Add(new Emoji("🛡️"), "Défense");
            EmotesAction.Add(new Emoji("👀"), "Observation");
        }

        private async Task MessageReceived(SocketMessage rawMessage)
        {
            // Ignore system messages and messages from bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            int argPos = 0;
            if (!message.Content.StartsWith("!")) return;

            var context = new SocketCommandContext(_discord, message);
            var result = await _commands.ExecuteAsync(context, argPos, _provider);

            if (result.Error.HasValue &&
                result.Error.Value != CommandError.UnknownCommand)
            {
                _logger.LogError($"{context.User.Username}  :  {rawMessage}  :  {result.ToString()}");
                //await context.Channel.SendMessageAsync(context.User.Mention + " : " + result.ToString());
            }
        }

        private async Task ReactionAddedOrRemoved(Cacheable<IUserMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot) return;

            if (ReactionMessages.Contains(cache.Id))
            {
                AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(reaction.User.Value.Mention);
                if (character == null)
                {
                    await channel.Value.SendMessageAsync("Error 404: Character not found or not loaded!");
                    return;
                }

                await channel.Value.SendMessageAsync(string.Format("{0} {1}",
                    reaction.User.Value.Mention,
                    character.Roll(EmotesAction[reaction.Emote].ToLower(), 0)));
            }
            if (HelpMessages.ContainsKey(cache.Id))
            {
                if (reaction.Emote.Name == "\U000025c0")//Previous page
                {
                    var msg = await cache.GetOrDownloadAsync();
                    await msg.ModifyAsync(x =>
                    {
                        x.Content = "";
                        x.Embed = HelpMessages[cache.Id].GetPreviousPage().Build();
                    });
                }
                else if (reaction.Emote.Name == "\U000025b6")//Next page
                {
                    var msg = await cache.GetOrDownloadAsync();
                    await msg.ModifyAsync(x =>
                    {
                        x.Content = "";
                        x.Embed = HelpMessages[cache.Id].GetNextPage().Build();
                    });
                }
            }
        }
    }
}
