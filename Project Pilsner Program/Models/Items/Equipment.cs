using LiteDB;

namespace Models
{
    public class Equipment
    {
        public Equipment() { }
        public Equipment(string uniqueCode, string name, string description,ItemRarity rarity, EquipmentType equipmentType, int attack, int defense, int speed, int price) 
        {
            Id = uniqueCode; 
            Name = name;
            Description = description;
            Rarity = rarity;
            EquipmentType = equipmentType;
            Attack = attack;
            Defence = defense;
            Agility = speed;
            Price = price;
        }

        [BsonId]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } 
        public EquipmentType EquipmentType { get; set; }
        public ItemRarity Rarity { get; set; }
        public int Attack { get; set; }
        public int Defence { get; set; }
        public int Agility { get; set; }
        public int Hitpoints { get; set; }
        public int Price { get; set; }
    }
}
