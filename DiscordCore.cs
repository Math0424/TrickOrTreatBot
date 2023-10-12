using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using DiscordBot.Objects;
using DiscordBot.Services;
using TrickOrTreatBot.Services;
using System.Linq;

namespace DiscordBot
{
    public class DiscordCore
    {
        private IServiceProvider Service;
        private DiscordSocketClient Client;
        private bool PhaseOne = false;

        static void Main(string[] _)
        {
            try
            {
                new DiscordCore().Run().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine("A critical error has occured!");
                Console.WriteLine(e);
            }
        }

        public void ConsoleIn()
        {
            while (true)
            {
                switch (Console.ReadLine().ToString())
                {
                    case "stop":
                        Client.SetActivityAsync(new Game("shutting down"));
                        Client.StopAsync();
                        Environment.Exit(0);
                        break;
                }
            }
        }

        public async Task Run()
        {
            new Task(() => ConsoleIn()).Start();

            DiscordSocketConfig config = new DiscordSocketConfig();
            config.GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.AllUnprivileged;
            config.AlwaysDownloadUsers = true;

            Utils.Log("Initalizing bot");
            Client = new DiscordSocketClient(config);
            Client.Log += LogAsync;

            //install commands
            Service = new ServiceCollection()
                .AddSingleton(this)
                .AddSingleton(Client)
                .AddSingleton<InteractionService>()

                .AddSingleton<DMHandler>()
                .AddSingleton<InteractionHandler>()
                .AddSingleton<TrickOrTreatModule>()
                
                .BuildServiceProvider();

            Client.Ready += async () =>
            {
                if (!PhaseOne)
                {
                    PhaseOne = true;
                    Service.GetRequiredService<DMHandler>();
                    Service.GetRequiredService<TrickOrTreatModule>();
                    await Service.GetRequiredService<InteractionHandler>().Initalize();
                    await LogAsync(new LogMessage(LogSeverity.Info, "Bot", "Logged in"));
                } 
                else
                {
                    await LogAsync(new LogMessage(LogSeverity.Info, "Bot", "Re-Logged in"));
                }
            };

            await Client.LoginAsync(TokenType.Bot, Storage.GetConfig("token", "nope"));
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        public Task LogAsync(LogMessage message)
        {
            if (message.Exception is CommandException cmdException)
            {
                Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
                    + $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine(cmdException);
            }
            else
                Console.WriteLine($"[General/{message.Severity}] {message}");

            return Task.CompletedTask;
        }
    }
}
