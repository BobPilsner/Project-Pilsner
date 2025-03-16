using Discord.WebSocket;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public partial class Program
{
    public async Task HandleMessageAsync(SocketMessage message)
    {
        // Don't answer bots
        if (message.Author.IsBot) return;

        // Receive the necessary data
        ulong userId = message.Author.Id;
        var guild = _bobClient.GetGuild(ulong.Parse(_data.GuildId));
        var user = guild.GetUser(userId);

        // Checks how to handle the command
        if (message.Content.Substring(0, 1) == "!")
        {
            // Make a player
            if (message.Content.Substring(1) == "start")
            {
                if (DataManager.GetPlayerById(userId) == null)
                {
                    DataManager.AddPlayer(userId, message.Author.GlobalName);
                    Player player = DataManager.GetPlayerById(userId);
                    var role = guild.Roles.FirstOrDefault(x => x.Id.ToString() == _data.RolePlayer);
                    await user.AddRoleAsync(role);
                    player.EquipItem("M1");
                    player.EquipItem("O1");
                    player.EquipItem("H1");
                    player.EquipItem("B1");
                    player.EquipItem("L1");
                    player.EquipItem("F1");
                    DataManager.UpdatePlayer(player);
                }
                else
                {
                    await message.Channel.SendMessageAsync("You already have a character.");
                }
            }
            else if (DataManager.GetPlayerById(userId) != null)
            {
                Player player = DataManager.GetPlayerById(userId);
                switch (message.Content.Substring(1))
                {
                    // Get the info about player
                    case "info":
                        await message.Channel.SendMessageAsync(
                        $"```{player.UserName} Lvl. {player.Level}\n" +
                        $"----------\n" +
                        $"Base stats\n" +
                        $"----------\n" +
                        $"HP".PadRight(4) + $": {player.Hitpoints}\n" +
                        $"ATT".PadRight(4) + $": {player.Attack + player.EquipmentAttack}\n" +
                        $"DEF".PadRight(4) + $": {player.Defence + player.EquipmentDefence}\n" +
                        $"SPD".PadRight(4) + $": {player.Agility + player.EquipmentAgility}\n" +
                        $"----------\n" +
                        $"Equipment\n" +
                        $"----------\n" +
                        $"Main-hand".PadRight(10) + ":".PadRight(2) + $"{(player.MainHand != null ? player.MainHand.Name : "nothing")}\n" +
                        $"Off-hand".PadRight(10) + ":".PadRight(2) + $"{(player.OffHand != null ? player.OffHand.Name : "nothing")}\n" +
                        $"Helmet".PadRight(10) + ":".PadRight(2) + $"{(player.Helmet != null ? player.Helmet.Name : "nothing")}\n" +
                        $"Body".PadRight(10) + ":".PadRight(2) + $"{(player.Body != null ? player.Body.Name : "nothing")}\n" +
                        $"Leggings".PadRight(10) + ":".PadRight(2) + $"{(player.Leggings != null ? player.Leggings.Name : "nothing")}\n" +
                        $"Boots".PadRight(10) + ":".PadRight(2) + $"{(player.Boots != null ? player.Boots.Name : "nothing")}\n```"
                        );
                        break;
                    // Delete player
                    case "quit":
                        DataManager.DeletePlayer(userId);
                        var role = guild.Roles.FirstOrDefault(x => x.Id.ToString() == _data.RolePlayer);
                        await user.RemoveRoleAsync(role);
                        break;
                    case "inventory":
                        Dictionary<string, int> materials = DataManager.GetMaterialsByPlayer(player);
                        StringBuilder sb = new StringBuilder();
                        foreach (var material in materials)
                            sb.Append($"{material.Value}x {material.Key}\n");
                        await message.Channel.SendMessageAsync(
                            "```" +
                            "Inventory:\n" +
                            "---------\n" +
                            $"{sb}" +
                            "```"
                        );

                        break;
                    // The command is not known
                    default:
                        await message.Channel.SendMessageAsync("Unknown Command.");
                        break;
                }
            }
            else
            {
                await message.Channel.SendMessageAsync("You can't use commands without creating a character first. To make a character, use the command '!start'");
            }
        }
    }

    // The voice of Bob
    private async Task BobMessageAsync(SocketMessage message)
    {
        // Answer Lilith

        // Ignore all other bots
        if (message.Author.IsBot) return;

        // Answer the player
        if (message.Content == "!start")
        {
            await message.Channel.SendMessageAsync($"Hey {message.Author.GlobalName}! Glad you chose to become an adventurer! Let me guide you to the best of my abilities!");
        }
    }

    // The voice of Lilith
    private async Task LilithMessageAsync(SocketMessage message)
    {
        // Answer Bob

        // Ignore all other bots
        if (message.Author.IsBot) return;

        //Answer the player
        if (message.Content == "!start")
        {
            await Task.Delay(10);
            await message.Channel.SendMessageAsync("Leave me alone, shrimp.");
        }
    }
}

