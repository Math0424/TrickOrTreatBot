using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Objects;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TrickOrTreatBot.Services
{
    public class InteractionHandler
    {

        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interaction;
        private readonly IServiceProvider _service;

        public InteractionHandler(DiscordSocketClient Client, InteractionService Interaction, IServiceProvider provider)
        {
            _client = Client;
            _interaction = Interaction;
            _service = provider;
        }

        public async Task Initalize()
        {
            await _interaction.AddModulesAsync(Assembly.GetEntryAssembly(), _service);

            _client.InteractionCreated += HandleInteraction;

            Utils.Log($"Adding commands to guild");
            foreach (var x in _client.Guilds)
            {
                Utils.Log($"Added commands to {x.Name}");
                await _interaction.RegisterCommandsToGuildAsync(x.Id, true);
            }
        }

        private async Task HandleInteraction(SocketInteraction context)
        {
            try
            {
                var ctx = new SocketInteractionContext(_client, context);
                await _interaction.ExecuteCommandAsync(ctx, _service);
            } 
            catch (Exception ex)
            {
                Utils.Log($"Error");
                Utils.Log(ex.Message);
            }
        }

    }
}
