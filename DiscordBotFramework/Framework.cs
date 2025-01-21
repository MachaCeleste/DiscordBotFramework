using Discord;
using Discord.WebSocket;
using System.Reflection;

namespace DiscordBotFramework
{
    public abstract class Framework
    {
        private readonly DiscordSocketClient _client;
        private readonly List<SlashCommand> _commands = new();
        private IServiceProvider _services;
        private TaskCompletionSource _taskComplete;
        private bool _wipe = false;

        /// <summary>
        /// Intents of bot
        /// </summary>
        protected abstract GatewayIntents Intents { get; }

        /// <summary>
        /// Public access to instance of DiscordSocketClient
        /// </summary>
        public DiscordSocketClient Client => _client;

        protected Framework()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents = Intents
            });

            _client.Log += LoggingAsync;
            _client.SlashCommandExecuted += HandleCommandAsync;
        }

        /// <summary>
        /// Connect bot
        /// </summary>
        /// <param name="token">Bot token</param>
        /// <param name="services">Services to expose</param>
        /// <returns></returns>
        public async Task ConnectAsync(string token, IServiceProvider services = null, bool refresh = false)
        {
            if (refresh)
                _wipe = true;
            _services = services;
            _taskComplete = new TaskCompletionSource();
            _client.Ready += OnReadyAsync;
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await _taskComplete.Task;
        }

        /// <summary>
        /// Disconnect bot gracefully
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            try
            {
                _wipe = false;
                _taskComplete.TrySetResult();
                await _client.LogoutAsync();
                await _client.StopAsync();
                await SystemMessage("Bot disconnected gracefully.");
            }
            catch (Exception ex)
            {
                await SystemMessage($"Error during disconnect: {ex.Message}");
            }
        }

        private async Task HandleCommandAsync(SocketSlashCommand command)
        {
            var matchCommand = _commands.Find(x => x.CommandName == command.Data.Name);
            if (matchCommand != null)
            {
                try
                {
                    await matchCommand.ExecuteAsync(command, _services);
                }
                catch (Exception ex)
                {
                    await command.RespondAsync($"Error: {ex.Message}", ephemeral: true);
                }
            }
            else
            {
                await command.RespondAsync("Unknown command.", ephemeral: true);
            }
        }

        private async Task RegisterCommandsAsync()
        {
            var commandTypes = Assembly.GetEntryAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(SlashCommand)) && !t.IsAbstract)
                .ToList();
            if (commandTypes.Count == 0)
                await SystemMessage($"No commands found in assembly!");
            var commands = await _client.Rest.GetGlobalApplicationCommands();
            foreach (var commandType in commandTypes)
            {
                var command = (SlashCommand)Activator.CreateInstance(commandType);
                try
                {
                    var commandProperties = command.Build();
                    if (!commands.Any(x => x.Name == command.CommandName))
                        await _client.Rest.CreateGlobalCommand(commandProperties);
                    _commands.Add(command);
                    await SystemMessage($"Registered command: {command.CommandName}");
                }
                catch (Exception ex)
                {
                    await SystemMessage($"Failed to register command {command.CommandName}: {ex.Message}");
                }
            }
            await SystemMessage("Done!");
        }

        private async Task OnReadyAsync()
        {
            await SystemMessage("Bot connected and ready!");
            if (_wipe)
                await _client.Rest.DeleteAllGlobalCommandsAsync();
            RegisterCommandsAsync();
        }

        /// <summary>
        /// Logging output
        /// </summary>
        /// <param name="msg">Log message</param>
        /// <returns></returns>
        protected abstract Task LoggingAsync(LogMessage msg);

        /// <summary>
        /// System message output
        /// </summary>
        /// <param name="msg">System message</param>
        /// <returns></returns>
        protected abstract Task SystemMessage(string msg);
    }
}