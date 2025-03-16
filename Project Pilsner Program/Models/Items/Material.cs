using LiteDB;

namespace Models 
{
    public class Material
    {
        public Material(string name, string description, ItemRarity rarity, int price) 
        {
            Name = name;
            Description = description;
            Rarity = rarity;
            Price = price;
        }

        [BsonId]
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemRarity Rarity { get; set; }
        public int Price { get; set; }
    }
}
