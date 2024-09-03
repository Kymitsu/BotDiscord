using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using BotDiscord.RPG;
using BotDiscord.Services;
using Discord.Interactions;

namespace BotDiscordConsole
{
    class Program
    {
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private IServiceProvider _servicesProvider;
        private IConfiguration _config;

        public async Task MainAsync()
        {
            var socketConfig = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };
            _client = new DiscordSocketClient(socketConfig);
            

            _config = BuildConfig();

            _servicesProvider = ConfigureServices();
            var init = _servicesProvider.GetRequiredService<CommandHandlingService>().InitializeAsync(_servicesProvider);
            var init2 = _servicesProvider.GetRequiredService<SlashCommandHandlingService>().IntialyzeAsync();
            var test2 = _servicesProvider.GetRequiredService<LoggingService>();

            _servicesProvider.GetRequiredService<CharacterService>().LoadFromCurrentDirectory();
            
            
            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            _client.Ready += Client_Ready;

            await Task.Delay(-1);
        }

        private async Task Client_Ready()
        {
            await _servicesProvider.GetRequiredService<SlashCommandHandlingService>().RegisterCommands();

        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<InteractionService>(new InteractionService(_client, new InteractionServiceConfig() { DefaultRunMode = Discord.Interactions.RunMode.Async}))
                .AddSingleton<StatAutocompleteHandler>()
                .AddSingleton<SlashCommandHandlingService>()
                .AddSingleton<AudioService>()
                // Logging
                .AddLogging()
                .AddSingleton<ILoggerProvider, LoggerProvider<ConsoleLogger>>()
                .AddSingleton<LoggingService>()
                // Extra
                .AddSingleton(_config)
                // Add additional services here...
                .AddSingleton<CharacterService>()
                .BuildServiceProvider();
        }

        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();
        }
    }
}
