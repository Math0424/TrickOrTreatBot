global using Discord;
global using Discord.Interactions;
global using Discord.WebSocket;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using System.IO;
using System;
using DiscordBot.Objects;
using DiscordBot.Services;


var builder = new HostApplicationBuilder(args);

var loggerConfig = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(Utils.GetDataFolder(), $"logs/log-{DateTime.Now:yy.MM.dd_HH.mm}.log"))
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(loggerConfig, dispose: true);

builder.Services.AddSingleton(new DiscordSocketClient(
    new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.AllUnprivileged,
        FormatUsersInBidirectionalUnicode = false,
        AlwaysDownloadUsers = true,
        LogGatewayIntentWarnings = false,
        LogLevel = LogSeverity.Verbose,
    }));

builder.Services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), new InteractionServiceConfig()
{
    LogLevel = LogSeverity.Info,
    ThrowOnError = true,
}));

builder.Services.AddScoped<Random>();

builder.Services.AddSingleton<InteractionHandler>();

builder.Services.AddHostedService<RandomStatusService>();

builder.Services.AddSingleton<TrickOrTreatService>();
builder.Services.AddHostedService(provider => provider.GetService<TrickOrTreatService>());

var app = builder.Build();

await app.RunAsync();
