using Discord.WebSocket;
using Discord;

namespace DiscordBotFramework
{
    public abstract class SlashCommand
    {
        private readonly List<SlashCommandOptionBuilder> _params = new();
        public abstract string CommandName { get; }
        public abstract string Description { get; }
        public abstract Task ExecuteAsync(SocketSlashCommand command, IServiceProvider services);

        /// <summary>
        /// Add a parameter to the command
        /// </summary>
        /// <param name="name">Name of param</param>
        /// <param name="type">Type of param</param>
        /// <param name="description">Description of param</param>
        /// <param name="isRequired">Is param required</param>
        public void AddParam(string name, ApplicationCommandOptionType type, string description, bool isRequired = false)
        {
            _params.Add(new SlashCommandOptionBuilder()
                .WithName(name)
                .WithDescription(description)
                .WithType(type)
                .WithRequired(isRequired));
        }

        internal SlashCommandProperties Build()
        {
            var commandBuilder = new SlashCommandBuilder()
                .WithName(CommandName)
                .WithDescription(Description);
            if (_params.Count() > 0)
            {
                foreach (var param in _params)
                {
                    commandBuilder.AddOption(param);
                }
            }
            return commandBuilder.Build();
        }
    }
}