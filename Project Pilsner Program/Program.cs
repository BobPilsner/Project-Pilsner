using System;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

public partial class Program
{
    private DiscordSocketClient _client;
    private DiscordSocketClient _bobClient;
    private DiscordSocketClient _lilithClient;
    private DataManager _data;
    static void Main()
    {
        new Program().RunBotAsync().GetAwaiter().GetResult();
    }
    public async Task RunBotAsync()
    {
        _data = new DataManager();
        _data.GetCoreData();
        DataManager.SeedData();

        var config = new DiscordSocketConfig
        {
            GatewayIntents =
            GatewayIntents.Guilds |
            GatewayIntents.GuildMessages |
            GatewayIntents.GuildMembers |
            GatewayIntents.MessageContent |
            GatewayIntents.GuildMessageReactions
        };

        // World configuration
        _client = new DiscordSocketClient(config);
        _client.Log += Log;
        await _client.LoginAsync(TokenType.Bot, _data.WorldToken);
        await _client.StartAsync();

        _client.MessageReceived += HandleMessageAsync;
        _client.ReactionAdded += HandleReactionAsync;

        // Bob configuration
        _bobClient = new DiscordSocketClient(config);
        _bobClient.Log += Log;
        await _bobClient.LoginAsync(TokenType.Bot, _data.BobToken);
        await _bobClient.StartAsync();

        _bobClient.MessageReceived += BobMessageAsync;

        // Lilith configuration
        _lilithClient = new DiscordSocketClient(config);
        _lilithClient.Log += Log;
        await _lilithClient.LoginAsync(TokenType.Bot, _data.LilithToken);
        await _lilithClient.StartAsync();

        _lilithClient.MessageReceived += LilithMessageAsync;

        // Start monster spawns
        await SpawnMonster();        

        // Keep the bot running
        await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg);
        return Task.CompletedTask;
    }
}