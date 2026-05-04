using Discord;
using Discord.WebSocket;
using System.Reflection;

namespace DiscordBotFramework
{
    public abstract class Framework
    {
        private readonly DiscordSocketClient _client;
        private readonly List<SlashCommand> _commands = new();
        private IServiceProvider? _services;
        private TaskCompletionSource? _taskComplete;
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
            _client.Ready += OnReadyAsync;
        }

        /// <summary>
        /// Connect bot
        /// </summary>
        /// <param name="token">Bot token</param>
        /// <param name="services">Services to expose</param>
        /// <returns></returns>
        public async Task ConnectAsync(string token, IServiceProvider? services = null, bool refresh = false)
        {
            if (refresh)
                _wipe = true;
            _services = services;
            _taskComplete = new TaskCompletionSource();
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
                _taskComplete?.TrySetResult();
                await _client.LogoutAsync();
                await _client.StopAsync();
                await LoggingAsync(new LogMessage(LogSeverity.Info, "DisconnectAsync", "Bot disconnected gracefully."));
            }
            catch (Exception ex)
            {
                await LoggingAsync(new LogMessage(LogSeverity.Error, "DisconnectAsync", "Error during disconnect!", ex));
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
            _commands.Clear();
            var commandTypes = Assembly.GetEntryAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(SlashCommand)) && !t.IsAbstract)
                .ToList();
            if (commandTypes.Count == 0)
                await LoggingAsync(new LogMessage(LogSeverity.Warning, "RegisterCommandsAsync", "No commands found in assembly!"));
            var commands = await _client.Rest.GetGlobalApplicationCommands();
            foreach (var commandType in commandTypes)
            {
                SlashCommand command = (SlashCommand)Activator.CreateInstance(commandType);
                try
                {
                    var commandProperties = command.Build();
                    if (!commands.Any(x => x.Name == command.CommandName))
                        await _client.Rest.CreateGlobalCommand(commandProperties);
                    _commands.Add(command);
                    await LoggingAsync(new LogMessage(LogSeverity.Info, "RegisterCommandsAsync", $"Registered command: {command.CommandName}"));
                }
                catch (Exception ex)
                {
                    await LoggingAsync(new LogMessage(LogSeverity.Error, "RegisterCommandsAsync", $"Failed to register command {command.CommandName}!", ex));
                }
            }
            await LoggingAsync(new LogMessage(LogSeverity.Info, "RegisterCommandsAsync", "Done!"));
        }

        private async Task OnReadyAsync()
        {
            await LoggingAsync(new LogMessage(LogSeverity.Info, "OnReadyAsync", "Bot connected and ready!"));
            if (_wipe)
                await _client.Rest.DeleteAllGlobalCommandsAsync();
            _ = RegisterCommandsAsync();
        }

        /// <summary>
        /// Logging output
        /// </summary>
        /// <param name="msg">Log message</param>
        /// <returns></returns>
        protected abstract Task LoggingAsync(LogMessage msg);
    }
}