using Discord;
using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Program
{
    public async Task SpawnMonster()
    {
        ulong farmlandsId = ulong.Parse(_data.FarmLandsId);
        var farmlands = await _client.GetChannelAsync(farmlandsId) as IMessageChannel;
        while(true)
        {
            int delay = DataManager.GenerateNumber(0, 16);
            List<Monster> monsters = DataManager.GetMonstersByArea(Areas.Farmlands);
            Monster monster = monsters[DataManager.GenerateNumber(0, monsters.Count)];
            monster.Level = DataManager.GetMonsterLevel();
            string level = "";
            for (int i = 0; i < monster.Level; i++)
            {
                level += "💀";
            }
            await Task.Delay(delay*1000);
            var embed = new EmbedBuilder()
                .WithTitle($"{monster.Name}  {level}")
                .WithDescription(monster.Description)
                .WithImageUrl($"~/Project Pilsner Program/Images/{monster.Name}.PNG")
                .WithColor(Color.Red)
                .Build();
            var sentMessage = await farmlands.SendMessageAsync(embed: embed);
            var swordEmoji = new Emoji("⚔️");
            await sentMessage.AddReactionAsync(swordEmoji);
        }
    }
}

