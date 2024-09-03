using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BotDiscord.Services
{
    public class SlashCommandHandlingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly InteractionService _interactionService;
        private readonly ILogger _logger;
        private IServiceProvider _provider;

        public SlashCommandHandlingService(IServiceProvider provider, DiscordSocketClient discord, InteractionService interactionService)
        {
            _discord = discord;
            _interactionService = interactionService;
            _provider = provider;
            _logger = provider.GetRequiredService<ILogger<SlashCommandHandlingService>>();

            _discord.InteractionCreated += InteractionCreated;
            _interactionService.InteractionExecuted += InterationExecuted;
            _interactionService.AutocompleteHandlerExecuted += AutoCompleteHandlerExecuted;
            //_discord.SlashCommandExecuted += SlashCommandhandler;
        }

        

        public async Task IntialyzeAsync()
        {
            _logger.LogInformation("Initialyze: Add Modules");
            var modules = await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _provider);
        }

        public async Task RegisterCommands()
        {
            var test = _discord.Guilds.FirstOrDefault();
            _logger.LogInformation($"Register Slash commands in guild : {test.Id}");
            var result = await _interactionService.RegisterCommandsToGuildAsync(test.Id);
        }

        private async Task InteractionCreated(SocketInteraction arg)
        {
            var context = new SocketInteractionContext(_discord, arg);
            var result = await _interactionService.ExecuteCommandAsync(context, _provider);

            if (result.Error.HasValue && result.Error.Value == InteractionCommandError.UnmetPrecondition)
            {
                await arg.RespondAsync(result.ErrorReason, ephemeral: true);
            }
        }

        private async Task InterationExecuted(ICommandInfo arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            if (arg3.IsSuccess)
            {
                _logger.LogInformation($"Interaction result success {arg1.Name}");
            }
            else
            {
                _logger.LogError($"Interaction result error {arg1.Name} - {arg3.ErrorReason}");
                await arg2.Interaction.RespondAsync($"{arg3.ErrorReason}", ephemeral:true);
            }
        }

        private Task AutoCompleteHandlerExecuted(IAutocompleteHandler arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            if (arg3.IsSuccess)
            {
                _logger.LogInformation($"AutoComplete result success {arg1.GetType().ToString()}");
            }
            else
            {
                _logger.LogError($"AutoComplete result error {arg1.GetType().ToString()} - {arg3.ErrorReason}");
            }

            return Task.CompletedTask;
        }

        private async Task SlashCommandhandler(SocketSlashCommand command)
        {
            //var test = new SocketInteractionContext(_discord, command);
            //var result = await _interactionService.ExecuteCommandAsync(test, _provider);
            throw new NotImplementedException();

        }
    }
}
