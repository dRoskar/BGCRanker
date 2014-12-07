using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGCRanker
{
    class Game
    {

        public String Name { get; set; }
        public Boolean HasRankingLadder { get; set; }
        private List<Player> players;
        private List<String> playerNames;

        public Game()
        {
            players = new List<Player>();
            playerNames = new List<String>();
        }

        public Game(String name)
        {
            this.Name = name;

            players = new List<Player>();
            playerNames = new List<String>();
        }

        public Game(String name, Boolean hasRankingLadder)
        {
            this.Name = name;
            this.HasRankingLadder = hasRankingLadder;

            players = new List<Player>();
            playerNames = new List<String>();
        }

        public void addPlayer(Player player)
        {
            players.Add(player);

            // add player to list of names
            playerNames.Add(player.Name);
        }

        public void setPlayers(List<Player> players)
        {

            this.players = players;

            // add players to list of names
            playerNames.Clear();
            foreach(Player player in players){
                playerNames.Add(player.Name);
            }
        }

        public List<Player> getPlayers()
        {
            return players;
        }

        public List<String> getPlayerNames()
        {
            return playerNames;
        }
    }
}
