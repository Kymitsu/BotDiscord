using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BotDiscord.Services
{
    public class LoggingService
    {
        private readonly ILogger _logger;

        public LoggingService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands, InteractionService interactionService)
        {
            _logger = provider.GetRequiredService<ILogger<LoggingService>>();
            
            discord.Log += LogAsync;
            commands.Log += LogAsync;
            interactionService.Log += LogAsync;
        }
        public Task LogAsync(LogMessage message)
        {
            if(message.Exception is CommandException cmdException)
            {
                _logger.LogError(cmdException, $"{cmdException.Command.Aliases.First()} failed to execute in {cmdException.Context.Channel}.");
            }
            else
            {
                switch (message.Severity)
                {
                    case LogSeverity.Error:
                        _logger.LogError(message.ToString());
                        break;
                    case LogSeverity.Warning:
                        _logger.LogWarning(message.ToString());
                        break;
                    case LogSeverity.Info:
                        _logger.LogInformation(message.ToString());
                        break;
                    default:
                        _logger.Log(LogLevel.Information, message.ToString());
                        break;
                }
            }

            return Task.CompletedTask;
        }
    }
}

