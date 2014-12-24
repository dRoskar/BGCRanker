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
        private ObservableCollection<Level> ladder;

        public LadderEditor(String ladderPath)
        {
            path = ladderPath;
            ladder = new ObservableCollection<Level>();
            InitializeComponent();
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // setup grid
            ranksGrid.ItemsSource = ladder;
            ranksGrid.ColumnFromDisplayIndex(0).IsReadOnly = true;

            // get main properties
            getProperties();

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
            formulaTextBox.Text = formula.ToString(CultureInfo.InvariantCulture);
            isCustomCheckBox.IsChecked = isCustom;
            formulaTextBox.IsEnabled = !isCustom;
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
            if (level == 1) { return 0; }
            if (level == 2) { return 1; }

            float requirement = 1.0f;
            for (int i = 0; i < level - 2; i++)
            {
                requirement += requirement * formula;
            }

            return (int)Math.Round(requirement, 0);
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

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void levelsTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                refreshGrid();
            }
        }

        private void formulaTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                refreshGrid();
            }
        }

        private void refreshBtn_Click(object sender, RoutedEventArgs e)
        {
            refreshGrid();
        }

        private void refreshGrid()
        {
            // validate fields
            int tempLevels;
            float tempFormula;
            Boolean valid = true;
            String invalidField = null;
            Boolean multipleInvalidFields = false;
            if (!int.TryParse(levelsTextBox.Text, out tempLevels))
            {
                valid = false;
                invalidField = "Levels text box";
            }

            if (!float.TryParse(formulaTextBox.Text, System.Globalization.NumberStyles.Any, NumberFormatInfo.InvariantInfo, out tempFormula))
            {
                valid = false;

                if (invalidField == null)
                {
                    invalidField = "formula text box";
                }
                else{
                    invalidField += "and the formula text box";
                    multipleInvalidFields = true;
                }
            }

            if (valid)
            {
                // edit grid data (levels)
                if (tempLevels != levels)
                {
                    if (tempLevels < levels)
                    {
                        // removing levels
                        levels = tempLevels;
                        for (int i = ladder.Count - 1; i > levels - 1; i--)
                        {
                            ladder.Remove(ladder[i]);
                        }
                    }
                    else
                    {
                        // adding levels
                        levels = tempLevels;
                        for (int i = ladder.Count; i < levels; i++)
                        {
                            ladder.Add(new Level(i + 1, getRequirement(i + 1), ""));
                        }
                    }
                }

                if (!isCustom)
                {
                    // edit grid data (requirements)
                    if (tempFormula != formula)
                    {
                        formula = tempFormula;
                        foreach (Level lvl in ladder)
                        {
                            lvl.Requirement = getRequirement(lvl.Number);
                        }
                    }
                }

                ranksGrid.Items.Refresh();
            }
            else
            {
                if (multipleInvalidFields)
                {
                    MessageBox.Show("The values you provided in the " + invalidField + " are invalid");
                }
                else
                {
                    MessageBox.Show("The value you provided in the " + invalidField + " is invalid");
                }

            }
        }

        private void isCustomCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            isCustom = true;
            formulaTextBox.IsEnabled = false;
        }

        private void isCustomCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            isCustom = false;
            formulaTextBox.IsEnabled = true;
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
