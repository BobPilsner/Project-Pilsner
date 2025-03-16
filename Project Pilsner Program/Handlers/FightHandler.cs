using Discord;
using Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;



public partial class Program
{
    public async void EngageFight(ulong playerId, string npcString, IMessageChannel channel)
    {
        var swordEmoji = new Emoji("⚔️");
        var shoeEmoji = new Emoji("👟");
        int firstSpace = npcString.IndexOf("  ");
        string npcName;
        int npcLevel;
        if (firstSpace != -1)
        {
            npcName = npcString.Substring(0, firstSpace);
            npcLevel = npcString.Substring(firstSpace + 2).Length;
        }   
        else
        {
            npcName = npcString;
            npcLevel = 0;
        }
        Monster monster = DataManager.GetMonsterByName(npcName);
        Player player = DataManager.GetPlayerById(playerId);

        // Check if the player has an ongoing battle
        if(DataManager.GetFightById(player.DiscordId) == null)  
            DataManager.AddFight(player, monster, npcLevel);
        else
        {
            DataManager.DeleteFight(player.DiscordId);
            DataManager.AddFight(player, monster, npcLevel);
        }

        Fight fight = DataManager.GetFightById(playerId);
        // Check who may go first
        if (player.Agility + player.EquipmentAgility >= monster.Agility)
        {
            var message = await channel.SendMessageAsync(
                $"```" +
                $"Battle log: {player.UserName} vs {monster.Name}\n" +
                $"ID: {player.DiscordId}\n" +
                $"--------------------------------------------\n" +
                $"You are quicker! You may make the first move!" +
                $"```"            
            );
            try
            {
                await message.AddReactionAsync(swordEmoji);
                await message.AddReactionAsync(shoeEmoji);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add emoji reaction: {ex.Message}");
            }

            DataManager.UpdateFight(fight);
        }
        else
        {

            // Calculate the monsters damage
            string usedAttack = monster.Attacks[DataManager.GenerateNumber(0, monster.Attacks.Count)];
            int damageMonster = DamageMonster(fight.Player.Defence, fight.Player.EquipmentDefence, fight.Monster.Attack, fight.Monster.LevelModifier);
            fight.Player.Hitpoints -= damageMonster;
            var message = await channel.SendMessageAsync(
                $"```" +
                $"Battle log: {player.UserName} vs {monster.Name}\n" +
                $"ID: {player.DiscordId}\n" +
                $"--------------------------------------------\n" +
                $"The {monster.Name} was quicker. It uses {usedAttack} and does {damageMonster} damage!\n\n" +
                $"{(fight.Player.Hitpoints > 0 ? $"You took a big hit! How will you react?" : "Oh no! You died!")}\n" +
                $"--------------------------------------------\n" +
                $"{fight.Player.UserName}: {fight.Player.Hitpoints}/{player.Hitpoints}".PadRight(25) +
                $"{fight.Monster.Name}: {fight.Monster.Hitpoints}/{monster.Hitpoints}" +
                $"```"
            );
            if (fight.Player.Hitpoints <= 0)
            {
                DataManager.DeleteFight(fight.FightId);
                await Task.Delay(10000);
                await message.DeleteAsync();
                await message.Channel.SendMessageAsync($"{fight.Player.UserName} lost against a lvl. {fight.Monster.Level} {fight.Monster.Name}");
            }
            else
            {
                DataManager.UpdateFight(fight);
                try
                {
                    await message.AddReactionAsync(swordEmoji);
                    await message.AddReactionAsync(shoeEmoji);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to add emoji reaction: {ex.Message}");
                }

            }
        }
    }
    public async void PlayerAttacks(ulong playerId, IMessageChannel channel)
    {
        // Get the objects
        Fight fight = DataManager.GetFightById(playerId);
        Player player = DataManager.GetPlayerById(playerId);
        Monster monster = DataManager.GetMonsterByName(fight.Monster.Name);
        // Generate battle data
        int damagePlayer = DamagePlayer(fight.Player.Attack, fight.Player.EquipmentAttack, fight.Monster.Defence, fight.Monster.LevelModifier);
        int damageMonster = DamageMonster(fight.Player.Defence, fight.Player.EquipmentDefence, fight.Monster.Attack, fight.Monster.LevelModifier);
        string usedAttack = fight.Monster.Attacks[DataManager.GenerateNumber(0, fight.Monster.Attacks.Count)];
        fight.Monster.Hitpoints -= damagePlayer;
        if (fight.Monster.Hitpoints > 0)
            fight.Player.Hitpoints -= damageMonster;
        // Get the react emojis
        var swordEmoji = new Emoji("⚔️");
        var shoeEmoji = new Emoji("👟");

        var message = await channel.SendMessageAsync(
            $"```" +
            $"Battle log: {fight.Player.UserName} vs {fight.Monster.Name}\n" +
            $"ID: {fight.Player.DiscordId}\n" +
            $"--------------------------------------------\n" +
            $"You charge at the monster with your {fight.Player.MainHand.Name} and deal {damagePlayer} damage!\n\n" +
            // Check if the monster is death
            $"{(fight.Monster.Hitpoints > 0 ? $"The monster counters you with {usedAttack} and deals {damageMonster} damage{(fight.Player.Hitpoints > 0 ? "! How will you react?" : ", enough to kill you!")}\n" : "Congratulations, the monster died and you win!\n")}" +
            $"--------------------------------------------\n" +
            $"{fight.Player.UserName}: {fight.Player.Hitpoints}/{player.Hitpoints}".PadRight(25) +
            $"{fight.Monster.Name}: {fight.Monster.Hitpoints}/{monster.Hitpoints}" +
            $"```"
        );

        if (fight.Player.Hitpoints <= 0 || fight.Monster.Hitpoints <= 0)
        {            
            await Task.Delay(10000);
            await message.DeleteAsync();
            if (fight.Player.Hitpoints <= 0)
                // Player loses
                await message.Channel.SendMessageAsync($"{fight.Player.UserName} lost against a lvl. {fight.Monster.Level} {fight.Monster.Name}!");
            else
            {
                // Player wins
                Material drop = CalculateDrop(fight.Monster);
                int amount = CalculateDropAmount(fight.Monster.LevelModifier);
                player.AddMaterial(drop, amount);
                double experience = monster.Experience * fight.Monster.LevelModifier;
                player.Experience += (int)experience;
                DataManager.UpdatePlayer(player);
                await message.Channel.SendMessageAsync($"{fight.Player.UserName} defeated a {(fight.Monster.Level > 0 ? $"lvl. {fight.Monster.Level}" : "")} {fight.Monster.Name}!");
                await message.Channel.SendMessageAsync(
                    "```" +
                    $"You won!\n" +
                    $"-------\n" +
                    $"Drop: {amount}x {drop.Name}\n" +
                    $"Experience: {experience}" +
                    "```"
                ); 
                CheckLevelUp(player, channel);
            }
            DataManager.DeleteFight(fight.FightId);
        }
        else
        {
            DataManager.UpdateFight(fight);
            try
            {
                await message.AddReactionAsync(swordEmoji);
                await message.AddReactionAsync(shoeEmoji);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add emoji reaction: {ex.Message}");
            }           
        }
    }
    public async void PlayerRuns(ulong playerId, IMessageChannel channel)
    {
        // Add fail to get away later
        Fight fight = DataManager.GetFightById(playerId);
        DataManager.DeleteFight(playerId);
        await channel.SendMessageAsync($"{fight.Player.UserName} has fled from a {fight.Monster.Name}.");
    }

    public int DamagePlayer(int playerAttack, int playerEquipmentAttack, int monsterDefence, double monsterLevelModifier)
    {
        int damageDone = DataManager.GenerateNumber(playerAttack * 2 /3, playerAttack) + playerEquipmentAttack;
        int damageBlocked = (int)(DataManager.GenerateNumber(0, monsterDefence / 3) * monsterLevelModifier);
        int totalDamage = damageDone - damageBlocked;
        if (totalDamage > 0)
            return totalDamage;
        else
            return 0;
    }

    public int DamageMonster(int playerDefence, int playerEquipmentDefence, int monsterAttack, double monsterLevelModifier)
    {
        int damageDone = (int)(DataManager.GenerateNumber(monsterAttack * 2 / 3 , monsterAttack) * monsterLevelModifier);
        int damageBlocked = (int)(DataManager.GenerateNumber(0, playerDefence + playerEquipmentDefence / 3) * (2.0 - monsterLevelModifier));
        int totalDamage = damageDone - damageBlocked;
        if (totalDamage > 0)
            return totalDamage;
        else
            return 0;
    }

    public Material CalculateDrop(Monster monster)
    {
        List<Material> allDrops = new List<Material>();
        List<Material> possibleDrops = new List<Material>();
        foreach (string drop in monster.Drops)
        {
            allDrops.Add(DataManager.GetMaterialByName(drop));
        }
        ItemRarity rarity = DataManager.CalculateRarity();
        foreach (Material drop in allDrops)
        {
            int index = 0;
            while (possibleDrops.Count == 0)
            {
                if (drop.Rarity == rarity - index)
                    possibleDrops.Add(drop);
                index++;
            }
            
        }
        return possibleDrops[DataManager.GenerateNumber(0, possibleDrops.Count)];
    }

    public int CalculateDropAmount(double modifier)
    {
        int index = (int)(DataManager.GenerateNumber(1, 101) * modifier);
        if (index < 60)
            return 1;
        else if (index < 90)
            return 2;
        else 
            return 3;
    }

    public async void CheckLevelUp(Player player, IMessageChannel channel)
    {
        Emoji sword = new Emoji("⚔️");
        Emoji shield = new Emoji("🛡️");
        Emoji heart = new Emoji("❤️");
        Emoji shoe = new Emoji("👟");
        Emoji cross = new Emoji("❌");
        Emoji checkMark = new Emoji("✅");
        double neededExp = Math.Pow(player.Level + 9.0, 2.0);
        if(player.Experience >= (int)neededExp)
        {
            LevelUp levelUp = DataManager.GetOrCreateLevelById(player.DiscordId);
            var message = await channel.SendMessageAsync(
            "```" +
            $"Congratulations {player.UserName}, you leveled up!\n" +
            $"ID: {player.DiscordId}\n" +
            $"----------\n" +
            $"You can allocate {levelUp.TotalPoints} more {(levelUp.TotalPoints != 1 ? "points" : "point")} in one of your stats:\n" +
            $" ⚔️".PadRight(3) + $" Attack: {levelUp.Attack}\n" +
            $" 🛡️".PadRight(3) + $" Defence: {levelUp.Defence}\n" +
            $" ❤️".PadRight(3) + $" Hitpoints: {levelUp.Hitpoints * 10}\n" +
            $" 👟".PadRight(3) + $" Speed: {levelUp.Agility}\n" +
            $"----------\n" +
            $"React with what stat you want to allocate a point to. Press ❌ to reset." +
            "```"
            );
            try
            {
                if( levelUp.TotalPoints > 0)
                {
                    await message.AddReactionAsync(sword);
                    await message.AddReactionAsync(shield);
                    await message.AddReactionAsync(heart);
                    await message.AddReactionAsync(shoe);
                }
                else
                {
                    await message.AddReactionAsync(checkMark);
                }
                await message.AddReactionAsync(cross);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add emoji reaction: {ex.Message}");
            }
        }
    }
}

