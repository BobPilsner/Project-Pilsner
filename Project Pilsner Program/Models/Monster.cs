using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Monster
    {
        public Monster(string name, string description, Areas area, int attack, int defence, int agility, int hitpoints, List<string> attacks, List<string> drops, int experience) 
        {
            Name = name;
            Description = description;
            Area = area;
            Attack = attack;
            Defence = defence;
            Agility = agility;
            Hitpoints = hitpoints;
            Attacks = attacks;
            Drops = drops;
            Experience = experience;
        }
        [BsonId]
        public string Name { get; set; }
        public string Description { get; set; }
        public int Level {  get; set; }
        public double LevelModifier => Level == 0 ? 1.0 : 1 + Level / 10.0;
        public Areas Area { get; set; }
        public int Attack {  get; set; }
        public int Defence { get; set; }
        public int Agility { get; set; }
        private int hitpoints;
        public int Hitpoints
        {
            get => hitpoints; 
            set
            {
                if(value< 0) 
                    value = 0;
                hitpoints = value;
            }
        }
        public List<string> Attacks { get; set; }
        public List<string> Drops { get; set; }
        public int Experience { get; set; }
    }
}
