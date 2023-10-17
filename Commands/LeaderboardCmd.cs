using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrickOrTreatBot.Commands
{
    public class LeaderboardCmd : InteractionModuleBase<SocketInteractionContext>
    {
        DiscordSocketClient client;

        public LeaderboardCmd(DiscordSocketClient client)
        {
            this.client = client;
        }

        public string GetName(ulong id)
        {
            var user = client.GetUser(id);
            return user?.GlobalName ?? user?.Username ?? id.ToString();
        }

        [SlashCommand("leaderboard", "See the scariest players")]
        public async Task Leaderboard()
        {
            List<Tuple<ulong, int>> leaderboard = Storage.GetScores();

            EmbedBuilder builder = new EmbedBuilder();
            builder.Title = "Spooky Score Leaderboard";
            string value = "";
            for(int i = 0; i < Math.Min(10, leaderboard.Count); i++)
            {
                value += $"**{i + 1}.** {GetName(leaderboard[i].Item1)} - :jack_o_lantern: {leaderboard[i].Item2}\n";
            }
            builder.Description = value;
            builder.Footer = new EmbedFooterBuilder();

            var score = Storage.GetScore(Context.User.Id);
            builder.Footer.Text = $"You are place {score.Item2} with a score of {score.Item1}";

            await RespondAsync(embed: builder.Build(), ephemeral: true);
        }

    }
}
