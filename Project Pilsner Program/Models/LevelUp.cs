using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class LevelUp
    {
        [BsonId]
        public ulong PlayerId {  get; set; }
        public int TotalPoints { get; set; }
        public int Attack { get; set; }
        public int Defence { get; set; }
        public int Hitpoints { get; set; }
        public int Agility { get; set; }
    }
}
