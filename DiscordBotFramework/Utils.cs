using Discord;
using Discord.WebSocket;

namespace DiscordBotFramework
{
    public class Utils
    {
        /// <summary>
        /// Send a direct message to a user
        /// </summary>
        /// <param name="user">User to send message to</param>
        /// <param name="data">Message data</param>
        /// <returns>MessageId</returns>
        public static async Task<ulong> SendDirectMessageAsync(IUser user, string data)
        {
            var msg = await user.SendMessageAsync(data);
            return msg.Id;
        }

        /// <summary>
        /// Send a message in a guild channel
        /// </summary>
        /// <param name="channel">Channel to send message in</param>
        /// <param name="data">Message data</param>
        /// <param name="time">Time message should persist</param>
        /// <param name="pinned">Pin message</param>
        /// <returns>MessageId</returns>
        public static async Task<ulong> SendMessageAsync(ISocketMessageChannel channel, string data, int time = -1, bool pinned = false)
        {
            var msg = await channel.SendMessageAsync(data);
            if (time == -1 && pinned)
                await msg.PinAsync();
            else
            {
                await Task.Delay(time);
                await channel.DeleteMessageAsync(msg);
            }
            return msg.Id;
        }

        /// <summary>
        /// Send an embed in a guild channel
        /// </summary>
        /// <param name="channel">Channel to send message within</param>
        /// <param name="title">Title of embed</param>
        /// <param name="data">Embed message data</param>
        /// <param name="color">Color of embed</param>
        /// <param name="image">Has image</param>
        /// <param name="direct">Is direct message</param>
        /// <param name="user">User to send to or use for image</param>
        /// <param name="time">Time message should persist</param>
        /// <param name="pinned">Pin message</param>
        /// <param name="url">Url for the embed click</param>
        /// <param name="footer">Fotter of embed</param>
        /// <param name="mentions">Initial message before embed, can mention</param>
        /// <returns>MessageId</returns>
        public static async Task<ulong> SendEmbedAsync(ISocketMessageChannel channel, string title, string data, uint color = 0xd5a8ff, bool image = true, bool direct = false, IUser? user = null, int time = -1, bool pinned = false, string url = "", string footer = "", string mentions = "")
        {
            var embed = EmbedWriter(title, data, color, image, user, url, footer);
            var msg = direct ? await user.SendMessageAsync(mentions, false, embed) : await channel.SendMessageAsync(mentions, false, embed);
            if (time == -1 && !direct && pinned)
                await msg.PinAsync();
            else if (!direct)
            {
                await Task.Delay(time);
                await channel.DeleteMessageAsync(msg);
            }
            return msg.Id;
        }

        /// <summary>
        /// Make an embed from data provided
        /// </summary>
        /// <param name="title"></param>
        /// <param name="data"></param>
        /// <param name="color"></param>
        /// <param name="image"></param>
        /// <param name="user"></param>
        /// <param name="url"></param>
        /// <param name="footer"></param>
        /// <returns>A fully built embed</returns>
        public static Embed? EmbedWriter(string title, string data, uint color = 0xd5a8ff, bool image = true, IUser? user = null, string url = "", string footer = "")
        {

            var embed = new EmbedBuilder();
            embed.WithTitle(title);
            embed.WithUrl(url);
            embed.WithDescription(data);
            embed.WithColor(new Discord.Color(color));
            embed.WithFooter(footer);
            if (image && user != null)
            {
                var imageUrl = user?.GetAvatarUrl();
                embed.WithThumbnailUrl(imageUrl);
            }
            return embed.Build();
        }

        /// <summary>
        /// Get a slash commands options by name
        /// </summary>
        /// <param name="command"></param>
        /// <param name="optionName"></param>
        /// <returns>An object for the option or null</returns>
        public static object? GetOptionValue(SocketSlashCommand command, string optionName)
        {
            return command.Data.Options.FirstOrDefault(x => x.Name == optionName)?.Value;
        }

        /// <summary>
        /// Get a guild by its name
        /// </summary>
        /// <param name="client"></param>
        /// <param name="guildName"></param>
        /// <returns>A SocketGuild or null</returns>
        public static SocketGuild? GetGuildByName(DiscordSocketClient client, string guildName)
        {
            return client.Guilds.FirstOrDefault(x => x.Name == guildName);
        }

        /// <summary>
        /// Get guild by its ID
        /// </summary>
        /// <param name="client"></param>
        /// <param name="id"></param>
        /// <returns>A SocketGuild or null</returns>
        public static SocketGuild? GetGuildById(DiscordSocketClient client, ulong id)
        {
            return client.Guilds.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Get channel by its name
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="channelName"></param>
        /// <returns>An ISocketMessageChannel or null</returns>
        public static ISocketMessageChannel? GetChannelByName(SocketGuild guild, string channelName)
        {
            return guild.Channels.FirstOrDefault(x => x.Name == channelName) as ISocketMessageChannel;
        }

        /// <summary>
        /// Get channel by its ID
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="id"></param>
        /// <returns>An ISocketMessageChannel or null</returns>
        public static ISocketMessageChannel? GetChannelById(SocketGuild guild, ulong id)
        {
            return guild.Channels.FirstOrDefault(x => x.Id == id) as ISocketMessageChannel;
        }

        /// <summary>
        /// Check if a user has a role by role ID
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public static bool UserHasRole(SocketGuildUser user, ulong roleId)
        {
            if (user.Roles.FirstOrDefault(r => r.Id == roleId) != null)
                return true;
            return false;
        }

        /// <summary>
        /// Check if a user has a role by role name
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public static bool UserHasRole(SocketGuildUser user, string roleName)
        {
            if (user.Roles.FirstOrDefault(r => r.Name == roleName) != null)
                return true;
            return false;
        }

        /// <summary>
        /// Check if a user has a permission
        /// </summary>
        /// <param name="user"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public static bool UserHasPermission(SocketGuildUser user, GuildPermission permission)
        {
            if (user.GuildPermissions.Has(permission))
                return true;
            return false;
        }
    }
}