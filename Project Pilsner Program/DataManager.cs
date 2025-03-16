using DotNetEnv;
using LiteDB;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class DataManager
{
    private readonly static Random random = new Random();
    private readonly static LiteDatabase db = new LiteDatabase("data.db");
    private readonly static ILiteCollection<Player> players = db.GetCollection<Player>("players");
    private readonly static ILiteCollection<Monster> monsters = db.GetCollection<Monster>("monsters");
    private readonly static ILiteCollection<Fight> fights = db.GetCollection<Fight>("fights");
    private readonly static ILiteCollection<LevelUp> levelUps = db.GetCollection<LevelUp>("levelUps");

    private readonly static ILiteCollection<Equipment> equipment = db.GetCollection<Equipment>("equipment");
    private readonly static ILiteCollection<Material> materials = db.GetCollection<Material>("materials"); 

    // CoreData
    public string WorldToken { get; private set; }
    public string BobToken { get; private set; }
    public string LilithToken { get; private set; }
    public string GuildId { get; private set; }
    public string RolePlayer { get; private set; }
    public string FarmLandsId { get; private set; }
    
    // Get the data out of the .env file
    public void GetCoreData()
    {
        Env.Load();
        WorldToken = Env.GetString("WORLD_TOKEN");
        BobToken = Env.GetString("BOB_TOKEN");
        LilithToken = Env.GetString("LILITH_TOKEN");
        GuildId = Env.GetString("GUILD_ID");
        RolePlayer = Env.GetString("ROLE_PLAYER");
        FarmLandsId = Env.GetString("FARMLANDS_ID");
    }

    // Add data to the database
    public static void SeedData()
    {
        if (equipment.Count() == 0)
            equipment.InsertBulk(equipmentData);
        else
            equipment.Upsert(equipmentData);
        if (monsters.Count() == 0)
            monsters.InsertBulk(monsterData);
        else
            monsters.Upsert(monsterData);
        if (materials.Count() == 0)
            materials.InsertBulk(materialData);
        else
            materials.Upsert(materialData);
    }

    // Generate random number
    public static int GenerateNumber(int minValue, int maxValue)
    {
        return random.Next(minValue, maxValue);
    }

    // Players
    public static void AddPlayer(ulong discordId, string userName)
    {
        var player = new Player
        {
            DiscordId = discordId,
            UserName = userName,
            Level = 1,
            Experience = 0,
            Money = 0,
            Allegiance = 10,

            Hitpoints = 100,
            Attack = 10,
            Defence = 10,
            Agility = 10,

            MainHand = null,
            OffHand = null,
            Helmet = null,
            Body = null,
            Leggings = null,
            Boots = null
        };
        players.Insert(player);
    }
    public static void DeletePlayer(ulong discordId)
    {
        players.DeleteMany(x => x.DiscordId == discordId);
    }

    public static void UpdatePlayer(Player player)
    {
        players.Update(player);
    }

    public static Player GetPlayerById(ulong discordId)
    {
        var player = players.Include(x => x.MainHand)
                    .Include(x => x.OffHand)
                    .Include(x => x.Helmet)
                    .Include(x => x.Body)
                    .Include(x => x.Leggings)
                    .Include(x => x.Boots)
                    .Include(x => x.Materials)
                    .FindOne(x => x.DiscordId == discordId);
        return player;
    }

    // Monsters
    public static Monster GetMonsterByName(string name)
    {
        return monsters.FindOne(x => x.Name == name);
    }
    public static List<Monster> GetMonstersByArea(Areas area)
    {
        return monsters.Find(x => x.Area == area).ToList();
    }
    public static int GetMonsterLevel()
    {
        int index = GenerateNumber(1, 101);
        if (index < 25)
            return 0;
        else if (index < 45)
            return 1;
        else if (index < 60)
            return 2;
        else if (index < 80)
            return 3;
        else if (index < 95)
            return 4;
        else
            return 5;
    }

    // Fights
    public static void AddFight(Player player, Monster monster, int monsterLevel)
    {
        var fight = new Fight()
        {
            FightId = player.DiscordId,
            StartTime = DateTime.Now,
            Player = player,
            Monster = monster,
        };
        fight.Monster.Level = monsterLevel;
        fights.Insert(fight);
    }
    public static void DeleteFight(ulong id)
    {
        fights.DeleteMany(x => x.FightId == id);
    }
    public static void UpdateFight(Fight fight)
    {
        fights.Update(fight);
    }

    public static Fight GetFightById(ulong id)
    {
        var fight = fights.Include(x => x.Player.MainHand)
                    .Include(x => x.Player.OffHand)
                    .Include(x => x.Player.Helmet)
                    .Include(x => x.Player.Body)
                    .Include(x => x.Player.Leggings)
                    .Include(x => x.Player.Boots)
                    .FindOne(x=>x.FightId == id);
        return fight;
    }
    // LevelUps
    public static void AddLevelUp(ulong player)
    {
        var levelUp = new LevelUp
        {
            PlayerId = player,
            TotalPoints = 5,
            Attack = 0,
            Defence = 0,
            Hitpoints = 0,
            Agility = 0
        };
        levelUps.Insert(levelUp);
    }
    public static LevelUp GetLevelById(ulong id)
    {
        return levelUps.FindOne(x => x.PlayerId == id);
    }
    public static LevelUp GetOrCreateLevelById(ulong discordId)
    {
        LevelUp levelUp = GetLevelById(discordId);
        if (levelUp == null)
        {
            AddLevelUp(discordId);
            return GetLevelById(discordId);
        }
        return levelUp;
    }
    public static void UpdateLevelUp(LevelUp levelUp)
    {
        levelUps.Update(levelUp);
    }
    public static void DeleteLevelup(LevelUp levelUp)
    {
        levelUps.DeleteMany(x => x.PlayerId == levelUp.PlayerId);
    }

    // Equipment
    public static Equipment GetEquipmentById(string id)
    {
        return equipment.FindOne(x => x.Id == id);
    }

    // Materials
    public static Material GetMaterialByName(string name)
    {
        return materials.FindOne(x => x.Name == name);  
    }
    public static Dictionary<string, int> GetMaterialsByPlayer(Player player)
    {
        return player.Materials;
    }
    public static ItemRarity CalculateRarity()
    {
        int index = GenerateNumber(1, 101);
        if (index < 50)
            return ItemRarity.Common;
        else if (index < 75)
            return ItemRarity.Uncommon;
        else if (index < 90)
            return ItemRarity.Rare;
        else if (index < 95)
            return ItemRarity.Epic;
        else if (index < 99)
            return ItemRarity.Legendary;
        else
            return ItemRarity.Mythic;
    }

    // Seeding Data
    readonly static List<Equipment> equipmentData = new List<Equipment>
    {
        // Main-hand
        new Equipment ("M1", "wooden sword", "A simple sword, made out of wood. It's not sharp at all.", ItemRarity.Common, EquipmentType.Mainhand, 10, 0, 0, 50),
        new Equipment ("M2", "rusty sword", "A sword made out of iron. It seems pretty old and may have lost most of its sharpness.", ItemRarity.Common, EquipmentType.Mainhand, 15, 0, 0, 100),
        new Equipment ("M3", "iron sword", "A normal sword. It will do it's job, but it's nothing fancy.", ItemRarity.Common, EquipmentType.Mainhand, 20, 0, 0, 200),
        // Off-hand
        new Equipment ("O1", "wooden shield", "A shield that blocks weak attacks. Don't use this in real fights.", ItemRarity.Common, EquipmentType.Offhand, 0, 5, 0, 50),
        new Equipment ("O2", "wooden companion sword", "Just a smaller version of a normal wooden sword, perfect for equiping in your secondary hand.", ItemRarity.Common, EquipmentType.Offhand, 5, 0, 0, 40),
        new Equipment ("O3", "iron shield", "A metal shield, used by the common adventurer.", ItemRarity.Common, EquipmentType.Offhand, 0, 15, 0, 150),
        // Helmet
        new Equipment ("H1", "leather headband", "A normal headband, used to keep your hair out of your eyes during combat.", ItemRarity.Common, EquipmentType.Helmet, 0, 0, 0, 10),
        new Equipment ("H2", "leather coif", "A coif that provides light cover. It's not the best protection, but it doesn't restrain your agility.", ItemRarity.Common, EquipmentType.Helmet, 0, 5, 5, 60),
        new Equipment ("H3", "iron helmet", "A simple metal helmet. It's cheap and ugly, but grands you some protection.", ItemRarity.Common, EquipmentType.Helmet, 0, 10, 0, 5),
        // Body
        new Equipment ("B1", "ragged shirt", "A shirt that makes you look like a peasant. It's actually not even worth a description.", ItemRarity.Common, EquipmentType.Body, 0, 0, 0, 0),
        new Equipment ("B2", "leather body", "A light body, that offers you light protection and doesn't reduce your mobility.", ItemRarity.Common, EquipmentType.Body, 0, 10, 5, 120),
        new Equipment ("B3", "iron chestplate", "A heavy chestplate, that offers good protection. Because it's so heavy, it reduces your mobility.", ItemRarity.Common, EquipmentType.Body, 0, 25, -10, 320),
        // Leggings
        new Equipment ("L1", "ragged shorts", "A rag that just covers your 'tools'.", ItemRarity.Common, EquipmentType.Leggings, 0, 0, 0, 0),
        new Equipment ("L2", "leather leggings", "A light legging, offering basic protection against most wildlife.", ItemRarity.Common, EquipmentType.Leggings, 0, 8, 5, 90),
        new Equipment ("L3", "iron platelegs", "A good protection for your legs, but this protection costs you agility.", ItemRarity.Common, EquipmentType.Leggings, 0, 15, -5, 270),
        // Boots
        new Equipment ("F1", "stained socks", "A pair of socks. You don't want to know what these stains are..", ItemRarity.Common, EquipmentType.Boots, 0, 0, 0, 0),
        new Equipment ("F2", "leather boots", "A pair of boots, that look like the boots in Puss in boots.", ItemRarity.Common, EquipmentType.Boots, 0, 2, 1, 8),
        new Equipment ("F3", "iron boots", "A pair of boots that will protect your feet for sure against the 'feet-fetish-people'.", ItemRarity.Common, EquipmentType.Boots, 0, 5, 0, 200),
    };

    readonly static List<Monster> monsterData = new List<Monster>
    {
        new Monster ("giant rat", "A rat is not supposed to be this big. Kill it before it eats you.", Areas.Farmlands, 15, 10, 10, 150, new List<string> { "bite","scratch" }, new List<string> { "beast bone", "rat tooth" }, 10),
        new Monster ("giant spider", "A creepy spider, as big as a baby. He has a smiling face on his hairy body.", Areas.Farmlands, 20, 5, 20, 100, new List<string> {"bite", "slash"}, new List<string> {"spider silk"}, 10),
        new Monster ("green slime", "A green transparant blob. Nobody knows how they move, or why they're even alive.", Areas.Farmlands, 15, 5, 10, 100, new List<string> {"bounce"}, new List<string>{"green ooze"}, 5)
    };

    readonly static List<Material> materialData = new List<Material>
    {
        new Material ("beast bone", "A small bone, coming from wildlife.", ItemRarity.Common, 5),
        new Material ("rat tooth", "A sharp small tooth. It's looking a little rotten.", ItemRarity.Uncommon, 8),
        new Material ("spider silk", "A soft fabric, coming from a not-so-soft creature.", ItemRarity.Common, 7),
        new Material ("green ooze", "A slimey green substance.", ItemRarity.Common, 5)
    };
}

