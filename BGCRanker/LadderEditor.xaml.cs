using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BGCRanker
{
    /// <summary>
    /// Interaction logic for LadderEditor.xaml
    /// </summary>
    public partial class LadderEditor : Window
    {

        private String path;
        private int levels;
        private float formula;
        private bool isCustom;
        private bool hasData;
        private List<float> requirements;
        private ObservableCollection<Level> ladder;

        public LadderEditor(String ladderPath)
        {
            path = ladderPath;
            ladder = new ObservableCollection<Level>();
            InitializeComponent();
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // bind data
            ranksGrid.ItemsSource = ladder;

            // get main properties
            getProperties();

            // calculate level requirements
            calculateRequirements();

            // display main properties
            showProperties();

            // populate grid
            if (!hasData)
            {
                // generate data from properties and write to text file
                generateGridData();
            }

            // read the data from text file
            readGridData();
        }

        private void getProperties(){
            // read basic properties from file
            StreamReader reader = new StreamReader(path, Encoding.UTF8);
            String line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("isCustom="))
                {
                    isCustom = line.Substring(line.IndexOf("=") + 1) == "1" ? true : false;
                }
                else if(line.Contains("formula=")){
                    float.TryParse(line.Substring(line.IndexOf("=") + 1), System.Globalization.NumberStyles.Any, NumberFormatInfo.InvariantInfo, out formula);
                }
                else if (line.Contains("levels="))
                {
                    int.TryParse(line.Substring(line.IndexOf("=") + 1), out levels);
                }
                else if (line.Contains("hasData="))
                {
                    hasData = line.Substring(line.IndexOf("=") + 1) == "1" ? true : false;
                }
            }

            reader.Close();
        }

        private void showProperties()
        {
            levelsTextBox.Text = levels.ToString();
            formulaTextBox.Text = formula.ToString();
            isCustomCheckBox.IsChecked = isCustom;
        }

        private void generateGridData(){
            // write levels data to file
            StreamWriter writer = new StreamWriter(path, true, Encoding.UTF8);  // appending

            for (int i = 0; i < levels; i++)
            {
                writer.WriteLine("LVL" + (i + 1) + "(" + getRequirement(i + 1) + ")[]");
            }
            writer.Close();
            hasData = true;
            writeHasData(true);
        }

        private void readGridData()
        {
            StreamReader sr = new StreamReader(path, Encoding.UTF8);
            String line;
            int number;
            int requirement;
            String rankName;

            while ((line = sr.ReadLine()) != null)
            {
                // parse data
                if(Regex.IsMatch(line, "LVL[0-9]+\\([0-9]+\\)\\[[a-zA-z]*\\]")){
                    Match match = Regex.Match(line, "LVL([0-9]+)\\(([0-9]+)\\)\\[([a-zA-z]*)\\]");

                    // level number
                    int.TryParse(match.Groups[1].Value, out number);

                    // requirement
                    int.TryParse(match.Groups[2].Value, out requirement);

                    // rankName
                    rankName = match.Groups[3].Value;

                    // add to grid
                    ladder.Add(new Level(number, requirement, rankName));
                }
            }
        }

        private int getRequirement(int level)
        {
            return (int) Math.Round(requirements[level - 1], 0);
        }

        private void calculateRequirements()
        {
            requirements = new List<float>();
            requirements.Add(0.0f);
            requirements.Add(1.0f);
            float requirement = requirements[1];
            for (int i = 0; i < levels - 2; i++)
            {
                requirement += requirement * formula;
                requirements.Add(requirement);
            }
        }

        private void writeIsCustom(Boolean value)
        {
            String sValue = value == true ? "1" : "0";
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path, Encoding.UTF8), "isCustom=[01]", "isCustom=" + sValue));
        }

        private void writeLevels(int value)
        {
            String sValue = value.ToString();
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path, Encoding.UTF8), "levels=[0-9]+", "levels=" + sValue));
        }

        private void writeFormula(float value)
        {
            String sValue = value.ToString();
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path, Encoding.UTF8), "formula=[0-9.]+", "formula=" + sValue));
        }

        private void writeHasData(Boolean value)
        {
            String sValue = value == true ? "1" : "0";
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path, Encoding.UTF8), "hasData=[01]", "hasData=" + sValue));
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cancleBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class Level
    {
        public int Number { get; set; }
        public int Requirement { get; set; }
        public String RankName { get; set; }

        public Level(int number, int requirement, String rankName)
        {
            this.Number = number;
            this.Requirement = requirement;
            this.RankName = rankName;
        }
    }
}
