using Discord.WebSocket;
using Discord;
using System;
using System.Threading.Tasks;
using System.Linq;
using Models;

public partial class Program
{
    private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> cacheable, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        // Ensure the reaction is not from a bot
        if (reaction.User.IsSpecified && reaction.User.Value.IsBot)
            return;

        // Retrieve the message
        var message = await cacheable.GetOrDownloadAsync();
        if (message == null)
            return;
        // Check if it's a spawned monster embed
        if (message.Embeds.FirstOrDefault() is Embed embed)
        {
            if (embed.Color == Color.Red)
            {
                string embedTitle = embed.Title;
                switch(reaction.Emote.Name)
                {
                    case "⚔️":
                        await message.DeleteAsync();
                        EngageFight(reaction.UserId, embedTitle, message.Channel);
                        break;
                }
            }
        }
        // Handle the combat mechanics
        if (message.Content.Length > 10)
        {
            if (message.Author.IsBot && message.Content.Substring(3, 10) == "Battle log" && message.Content.Contains(reaction.UserId.ToString()))
            {
                switch (reaction.Emote.Name)
                {
                    case "⚔️":
                        await message.DeleteAsync();
                        PlayerAttacks(reaction.UserId, message.Channel);
                        break;
                    case "👟":
                        await message.DeleteAsync();
                        PlayerRuns(reaction.UserId, message.Channel);
                        break;
                }
            }
        }
        // Handle level up mechanics
        if (message.Content.Length > 10)
        {
            if (message.Author.IsBot && message.Content.Substring(3, 15) == "Congratulations" && message.Content.Contains(reaction.UserId.ToString()))
            {
                Player player = DataManager.GetPlayerById(reaction.UserId);
                LevelUp levelUp = DataManager.GetLevelById(player.DiscordId);
                switch (reaction.Emote.Name)
                {
                    case "⚔️":
                        await message.DeleteAsync();
                        levelUp.TotalPoints--;
                        levelUp.Attack++;
                        DataManager.UpdateLevelUp(levelUp);
                        CheckLevelUp(player, reaction.Channel);
                        break;
                    case "🛡️":
                        await message.DeleteAsync();
                        levelUp.TotalPoints--;
                        levelUp.Defence++;
                        DataManager.UpdateLevelUp(levelUp);
                        CheckLevelUp(player, reaction.Channel);
                        break;
                    case "❤️":
                        await message.DeleteAsync();
                        levelUp.TotalPoints--;
                        levelUp.Hitpoints++;
                        DataManager.UpdateLevelUp(levelUp);
                        CheckLevelUp(player, reaction.Channel);
                        break;
                    case "👟":
                        await message.DeleteAsync();
                        levelUp.TotalPoints--;
                        levelUp.Agility++;
                        DataManager.UpdateLevelUp(levelUp);
                        CheckLevelUp(player, reaction.Channel);
                        break;
                    case "❌":
                        await message.DeleteAsync();
                        levelUp.TotalPoints = 5;
                        levelUp.Attack = 0;
                        levelUp.Defence = 0;
                        levelUp.Hitpoints = 0;
                        levelUp.Agility = 0;
                        DataManager.UpdateLevelUp(levelUp);
                        CheckLevelUp(player, reaction.Channel);
                        break;
                    case "✅":
                        await message.DeleteAsync();
                        player.Level++;
                        player.Attack += levelUp.Attack;
                        player.Defence += levelUp.Defence;
                        player.Hitpoints += levelUp.Hitpoints * 10;
                        player.Agility += levelUp.Agility;
                        DataManager.UpdatePlayer(player);
                        DataManager.DeleteLevelup(levelUp);
                        break;
                }

            }
        }
    }
}
