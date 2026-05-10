using Discord;
using Discord.WebSocket;

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

        /// <summary>
        /// Add a parameter with predefined choices (shows dropdown)
        /// </summary>
        /// <param name="choices">Dictionary of display names to values e.g. { "Admin", "admin" }</param>
        public void AddParamWithChoices(string name, string description, Dictionary<string, string> choices, bool isRequired = false)
        {
            var option = new SlashCommandOptionBuilder()
                .WithName(name)
                .WithDescription(description)
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(isRequired);

            foreach (var choice in choices)
                option.AddChoice(choice.Key, choice.Value);

            _params.Add(option);
        }

        internal SlashCommandProperties Build()
        {
            var commandBuilder = new SlashCommandBuilder()
                .WithName(CommandName)
                .WithDescription(Description);

            if (_params.Count() > 0)
                foreach (var param in _params)
                    commandBuilder.AddOption(param);

            return commandBuilder.Build();
        }
    }
}