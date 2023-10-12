using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class DMHandler
    {
        public DiscordSocketClient _client;

        public DMHandler(DiscordSocketClient Client)
        {
            _client = Client;
            Client.MessageReceived += HandleDM;
        }

        private async Task HandleDM(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            var context = new SocketCommandContext(_client, message);
            
            if (context.IsPrivate && !context.User.IsBot)
            {
                SocketTextChannel channel = (SocketTextChannel)await _client.GetChannelAsync(1109329599583756292);
                if (channel != null)
                {
                    EmbedBuilder builder = new EmbedBuilder()
                        .WithAuthor(message.Author)
                        .WithDescription(message.Content)
                        .WithFooter($"ID: {message.Author.Id}")
                        .WithColor(Color.Blue);
                    if (message.Attachments.Count != 0)
                    {
                        string value = "";
                        foreach (var x in message.Attachments)
                            value += $"{x.Url}\n";
                        builder.AddField(new EmbedFieldBuilder().WithName("Attachments").WithValue(value.Trim()));
                    }
                    await channel.SendMessageAsync(embed: builder.Build());
                }
            }
        }

    }
}
