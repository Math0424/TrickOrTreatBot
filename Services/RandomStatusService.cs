
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal class RandomStatusService(DiscordSocketClient client, Random rand) : BackgroundService
{

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            while (true)
            {
                if (client.ConnectionState != ConnectionState.Connected)
                    continue;

                switch (rand.Next(11))
                {
                    case 7:
                        await SetText($"the screams", ActivityType.Listening);
                        break;
                    default:
                        await SetText(games[rand.Next(games.Count)], ActivityType.Playing);
                        break;
                }
                Thread.Sleep(1000 * 120);
            }

        });
    }

    private readonly List<string> games = new List<string>()
    {
        "Trick Or Treat",
        "Hide And Seek",
        "League of Legends",
        "with Wolf",
        "with Lamb",
        "with Lives",
    };
        
    private async Task SetText(string value, ActivityType type2)
    {
        await client.SetGameAsync(value, type: type2);
    }

}
