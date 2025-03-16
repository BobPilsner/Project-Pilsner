using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Fight
    {
        [BsonId]
        public ulong FightId { get; set; }
        public DateTime StartTime { get; set; }
        public Player Player {  get; set; }
        public Monster Monster { get; set; }
    }
}
