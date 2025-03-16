using LiteDB;
using System;
using System.Collections.Generic;

namespace Models
{
    public class Player
    {
        [BsonId]
        public ulong DiscordId { get; set; }
        public string UserName { get; set; }
        public int Money { get; set; }
        public int Allegiance { get; set; }
        // level
        public int Level { get; set; }
        public int Experience { get; set; }

        // Stats
        private int hitpoints;
        public int Hitpoints 
        {
            get => hitpoints; 
            set
            {
                if(value < 0) 
                    value = 0;
                hitpoints = value;
            } 
        }
        public int Attack { get; set; }
        public int EquipmentAttack { get; set; }
        public int Defence { get; set; }
        public int EquipmentDefence { get; set; }
        public int Agility { get; set; }
        public int EquipmentAgility {  get; set; } 

        // Inventory
        public Dictionary<Equipment, int> Equipment { get; set; } = new Dictionary<Equipment, int>();
        public Dictionary<string, int> Materials { get; set; } = new Dictionary<string, int>();

        // Inventory handling
        public void AddMaterial(Material material, int amount)
        {
            if (Materials.ContainsKey(material.Name))
                Materials[material.Name] += amount;
            else
                Materials[material.Name] = amount;
        }
        public bool RemoveMaterials(Material material, int amount)
        {
            if (Materials.ContainsKey(material.Name))
            {
                if (Materials[material.Name] >= amount) 
                {
                    Materials[material.Name] -= amount;
                    if (Materials[material.Name] == 0)
                        Materials.Remove(material.Name);
                    return true;
                }
            }
            return false;
        }
        public int GetMaterialCount(Material material)
        {
            return Materials.TryGetValue(material.Name, out int count) ? count : 0;
        }

        // Equipment
        [BsonRef("equipment")]
        public Equipment MainHand { get; set; }
        public Equipment OffHand { get; set; }
        public Equipment Helmet { get; set; }
        public Equipment Body { get; set; }
        public Equipment Leggings { get; set; }
        public Equipment Boots { get; set; }

        // Equipment handling
        public void EquipItem(string equipmentCode)
        {
            Equipment equipment = DataManager.GetEquipmentById(equipmentCode);
            EquipmentAttack += equipment.Attack;
            EquipmentDefence += equipment.Defence;
            EquipmentAgility += equipment.Agility;

            switch(equipment.EquipmentType) 
            {
                case EquipmentType.Mainhand:
                    MainHand = equipment;
                    break;
                case EquipmentType.Offhand:
                    OffHand = equipment;
                    break;
                case EquipmentType.Helmet: 
                    Helmet = equipment;
                    break;
                case EquipmentType.Body: 
                    Body = equipment;
                    break;
                case EquipmentType.Leggings: 
                    Leggings = equipment;
                    break;
                case EquipmentType.Boots: 
                    Boots = equipment;
                    break;
            }
        }
        public void UnequipItem(string equipmentCode)
        {
            Equipment equipment = DataManager.GetEquipmentById(equipmentCode);
            EquipmentAttack -= equipment.Attack;
            EquipmentDefence -= equipment.Defence;
            EquipmentAgility -= equipment.Agility;

            switch (equipment.EquipmentType)
            {
                case EquipmentType.Mainhand:
                    MainHand = null;
                    break;
                case EquipmentType.Offhand:
                    OffHand = null;
                    break;
                case EquipmentType.Helmet:
                    Helmet = null;
                    break;
                case EquipmentType.Body:
                    Body = null;
                    break;
                case EquipmentType.Leggings:
                    Leggings = null;
                    break;
                case EquipmentType.Boots:
                    Boots = null;
                    break;
            }
        }
    }
}
