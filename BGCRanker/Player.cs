using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGCRanker
{
    class Player
    {
        public String Name { get; set; }
        public String Game { get; set; }
        public int Victories { get; set; }
        public int Level { get; set; }
        public String Rank { get; set; }

        public Player()
        {

        }

        public Player(String name)
        {
            this.Name = name;
        }

        public Player(String name, String game)
        {
            this.Name = name;
            this.Game = game;
        }
    }
}
