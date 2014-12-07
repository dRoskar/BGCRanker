using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;

namespace BGCRanker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private String dataPath;
        private List<Game> games;
        private List<String> gameNames;
        private List<Player> players;
        private String selectedGame;

        public MainWindow()
        {
            games = new List<Game>();
            players = new List<Player>();
            gameNames = new List<String>();

            InitializeComponent();

            manageDirectories();
            readData(null);
        }

        private void pathBtn_Click(object sender, RoutedEventArgs e)
        {
            dataPath = selectDataPath(false);
        }

        private void addGameBtn_Click(object sender, RoutedEventArgs e)
        {
            addNewGame();
        }

        private String selectDataPath(bool newUser){
            if (newUser)
            {
                System.Windows.MessageBox.Show("Welcome to BGC Ranker. Please specify a path to the folder that contains all of your gaming data...");
            }

            // prompt user for new data path
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            else return "error";
        }

        private void manageDirectories()
        {
            StreamWriter programFile;

            // check for data path info
            string myDocumentsPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!System.IO.Directory.Exists(myDocumentsPath + @"\BGCRanker"))
            {
                // create program directory
                System.IO.Directory.CreateDirectory(myDocumentsPath + @"\BGCRanker");
            }

            myDocumentsPath = myDocumentsPath + @"\BGCRanker\dataPath.txt";

            if (File.Exists(myDocumentsPath))
            {
                //get the data path
                dataPath = System.IO.File.ReadAllText(myDocumentsPath, Encoding.UTF8);

                if (dataPath.Equals(""))
                {
                    // prompt user to select a data path
                    dataPath = selectDataPath(true);

                    // write dataPath to programFile
                    System.IO.File.WriteAllText(myDocumentsPath, dataPath);
                }
            }
            else
            {
                // create program file
                programFile = System.IO.File.CreateText(myDocumentsPath);
                programFile.Close();

                // prompt user to select a data path
                dataPath = selectDataPath(true);

                // write dataPath to programFile
                System.IO.File.WriteAllText(myDocumentsPath, dataPath);
            }
        }

        private void readData(String newGame)
        {
            // reset collected data
            games.Clear();
            players.Clear();

            // get games
            String[] gameDirectories = Directory.GetDirectories(dataPath);
            foreach (String gameDirectory in gameDirectories)
            {
                String[] splitDir = gameDirectory.Split('\\');
                Game game = new Game(splitDir.ElementAt(splitDir.Length - 1));
                games.Add(game);
                gameNames.Add(game.Name);
            }

            // get players
            foreach(Game game in games){
                // make player profiles folder
                String profilesPath = dataPath + "\\" + game.Name + @"\profiles";
                if (!Directory.Exists(profilesPath))
                {
                    Directory.CreateDirectory(profilesPath);
                }

                // read player list from game folder
                if (File.Exists(dataPath + "\\" + game.Name + @"\players.txt"))
                {
                    String[] lines = System.IO.File.ReadAllLines(dataPath + "\\" + game.Name + @"\players.txt", Encoding.UTF8);

                    foreach (String line in lines)
                    {
                        Player player = new Player(line, game.Name);
                        players.Add(player);

                        // if player doesn't exist in the database yet, give him a profile
                        if (!File.Exists(profilesPath + "\\" + player.Name + ".txt"))
                        {
                            StreamWriter profileWriter = File.CreateText(profilesPath + "\\" + player.Name + ".txt");
                            profileWriter.WriteLine("name=" + player.Name);
                            profileWriter.WriteLine("victories=0");
                            profileWriter.WriteLine("level=1");
                            profileWriter.WriteLine("rank=");
                            profileWriter.Close();
                        }
                    }
                }
            }

            gamesComboBox.Items.Refresh();

            // if new game was just added, switch to it
            if (newGame != null && newGame != "")
            {
                for (int i = 0; i < gamesComboBox.Items.Count; i++)
                {
                    if (gamesComboBox.Items.GetItemAt(i).ToString().Equals(newGame))
                    {
                        gamesComboBox.SelectedItem = gamesComboBox.Items.GetItemAt(i);
                    }
                }
            }
        }

        private void gamesComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            // bind games list to combo box and select first item
            gamesComboBox.ItemsSource = gameNames;
            gamesComboBox.SelectedIndex = 0;
        }

        private void gamesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            comboSelectionChanged();
        }

        public void comboSelectionChanged()
        {
            selectedGame = gamesComboBox.SelectedItem as String;

            // populate players list box
            List<String> playersCurrent = new List<String>();
            playersListBox.ItemsSource = playersCurrent;


            foreach (Player player in players)
            {
                if (player.Game.Equals(selectedGame))
                {
                    playersCurrent.Add(player.Name);
                }
            }

            playersListBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("", System.ComponentModel.ListSortDirection.Ascending));
        }

        private void playersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (playersListBox.SelectedItem != null)
            {
                //display data if the game has a ranking ladder
                if (File.Exists(dataPath + "\\" + selectedGame + "\\" + "rankingLadder.txt"))
                {
                    playerNameLabel.FontSize = 16;
                    playerNameLabel.Content = playersListBox.SelectedItem.ToString();
                }
                else
                {
                    playerNameLabel.FontSize = 12;
                    playerNameLabel.Content = "This game does not have a ranking ladder yet.";
                }
            }
            else{
                clearFields();
            }
        }

        private void clearFields()
        {
            playerNameLabel.Content = "";
        }

        private void addNewGame()
        {
            // prompt user for game name
            gamePrompt gp = new gamePrompt();
            gp.ShowDialog();

            if (gp.getStatus())
            {
                String gameName = gp.getgameName();

                if (gameName != "" && gameName != null)
                {
                    // create a new fodlder for the game in the data path
                    String gamePath = dataPath + "\\" + gameName;
                    System.IO.Directory.CreateDirectory(gamePath);

                    //create players list and profiles folder
                    StreamWriter sw = System.IO.File.CreateText(gamePath + "\\" + "players.txt");
                    sw.Close();

                    Directory.CreateDirectory(gamePath + "\\" + "profiles");

                    // refresh gameList
                    readData(gameName);
                }
            }
        }

        private void addPlayerBtn_Click(object sender, RoutedEventArgs e)
        {
            PlayerPrompt pp = new PlayerPrompt();
            pp.ShowDialog();

            if(pp.getStatus()){
                String playerName = pp.getPlayerName();

                if (playerName != "" && playerName != null)
                {
                    // add player to current games' player list
                    try
                    {
                        // if file is empty, no new line
                        if (System.IO.File.ReadAllText(dataPath + "\\" + selectedGame + "\\players.txt") != "")
                        {
                            System.IO.File.AppendAllText(dataPath + "\\" + selectedGame + "\\players.txt", string.Format("{0}{1}", Environment.NewLine, playerName), Encoding.UTF8);
                        }
                        else
                        {
                            System.IO.File.AppendAllText(dataPath + "\\" + selectedGame + "\\players.txt", playerName, Encoding.UTF8);
                        }
                    }
                    catch (UnauthorizedAccessException uae)
                    {
                        System.Windows.MessageBox.Show(uae.Message);
                    }

                    //re-read the data
                    readData(null);

                    // refresh lists
                    comboSelectionChanged();
                }
            }
        }
    }
}
