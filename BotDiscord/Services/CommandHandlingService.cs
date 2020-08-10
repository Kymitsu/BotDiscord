using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;
using BotDiscord.RPG;

namespace BotDiscord.Services
{
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private IServiceProvider _provider;
        
        public static Dictionary<IEmote, string> EmotesAction = new Dictionary<IEmote, string>();
        public static List<ulong> ReactionMessages = new List<ulong>();

        public CommandHandlingService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;

            _discord.MessageReceived += MessageReceived;
            _discord.ReactionAdded += ReactionAddedOrRemoved;
            _discord.ReactionRemoved += ReactionAddedOrRemoved;
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            _provider = provider;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            // Add additional initialization code here...

            EmotesAction.Add(new Emoji("🏃‍♂️"), "initiative");
            EmotesAction.Add(new Emoji("⚔️"), "attaque");
            EmotesAction.Add(new Emoji("🛡️"), "défense");
            EmotesAction.Add(new Emoji("👀"), "observation");
        }

        private async Task MessageReceived(SocketMessage rawMessage)
        {
            // Ignore system messages and messages from bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;
            
            int argPos = 0;
            //if (!message.Content.StartsWith("!")) return;
            
            var context = new SocketCommandContext(_discord, message);
            var result = await _commands.ExecuteAsync(context, argPos, _provider);

            if (result.Error.HasValue &&
                result.Error.Value != CommandError.UnknownCommand)
            {
                await Log(context.User.Username + " : " + rawMessage + " : " + result.ToString());
                //await context.Channel.SendMessageAsync(context.User.Mention + " : " + result.ToString());
            }
        }

        private async Task ReactionAddedOrRemoved(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot) return;

            if (ReactionMessages.Contains(cache.Id))
            {
                AnimaCharacter character = AnimaCharacterRepository.FindCurrentByMention(reaction.User.Value.Mention);
                if (character == null)
                {
                    await channel.SendMessageAsync("Error 404: Character not found or not loaded!");
                    return;
                }

                await channel.SendMessageAsync(string.Format("{0} {1}",
                    reaction.User.Value.Mention,
                    character.Roll(EmotesAction[reaction.Emote], 0)));
            }
        }

        //methode un peu pourrave mais ça passe
        private Task Log(string msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
