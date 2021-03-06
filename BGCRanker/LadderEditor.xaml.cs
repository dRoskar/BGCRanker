﻿using System;
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
using System.Windows.Controls.Primitives;

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

            // get main properties
            getProperties();

            if (hasData)
            {
                // read the data from text file
                readGridData();
            }
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // setup grid
            ranksGrid.ItemsSource = ladder;
            ranksGrid.ColumnFromDisplayIndex(0).IsReadOnly = true;
            ranksGrid.ColumnFromDisplayIndex(0).Width = 55;
            ranksGrid.ColumnFromDisplayIndex(1).Width = 80;
            ranksGrid.ColumnFromDisplayIndex(2).Width = 98;
            ranksGrid.ColumnFromDisplayIndex(3).Width = 95;
            ranksGrid.ColumnFromDisplayIndex(4).Width = 95;

            // display main properties
            showProperties();

            // populate grid
            if (!hasData)
            {
                // generate data from default properties
                generateExampleGridData();

                //write grid data to file
                writeGridDataToFile();
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

        private void generateExampleGridData(){
            // generate example gird data
            ladder.Clear();

            for (int i = 0; i < levels; i++)
            {
                ladder.Add(new Level(i + 1, getRequirement(i + 1), "", "", ""));
            }
        }

        private void writeGridDataToFile()
        {
            StreamWriter writer = new StreamWriter(path, true, Encoding.UTF8);  // appending

            for (int i = 0; i < levels; i++)
            {
                writer.WriteLine("LVL" + (i + 1) + "(" + getRequirement(i + 1) + ")[" + ladder[i].RankName + "][" + ladder[i].ImageUri + "][" + ladder[i].ImageUrl + "]");
            }
            writer.Close();
            hasData = true;
            writeHasData(true);
        }

        private void eraseGridDataFromFile()
        {
            StreamReader sr = new StreamReader(path, Encoding.UTF8);
            String line;
            List<String> lines = new List<String>();

            // read all lines
            while ((line = sr.ReadLine()) != null)
            {
                lines.Add(line);
            }

            // remove grid data lines
            for (int i = lines.Count - 1; i > -1; i--)
            {
                if (lines[i].Contains("LVL"))
                {
                    lines.RemoveAt(i);
                }
            }
            sr.Close();

            // write remaining lines back to file
            StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8);
            foreach (String ln in lines)
            {
                sw.WriteLine(ln);
            }
            sw.Close();
        }

        private void readGridData()
        {
            StreamReader sr = new StreamReader(path, Encoding.UTF8);
            String line;
            int number;
            int requirement;
            String rankName;
            String imageUri;
            String imageUrl;

            // clean up old data
            ladder.Clear();

            while ((line = sr.ReadLine()) != null)
            {
                // parse data
                if (Regex.IsMatch(line, "LVL[0-9]+\\([0-9]+\\)\\[[0-9a-zA-Z -]*\\]\\[[0-9a-zA-Z._+ -]*\\]\\[[0-9a-zA-Z.:/ -+]*\\]"))
                {
                    Match match = Regex.Match(line, "LVL([0-9]+)\\(([0-9]+)\\)\\[([0-9a-zA-Z -]*)\\]\\[([0-9a-zA-Z._+ -]*)\\]\\[([0-9a-zA-Z.:/ -+]*)\\]");

                    // level number
                    int.TryParse(match.Groups[1].Value, out number);

                    // requirement
                    int.TryParse(match.Groups[2].Value, out requirement);

                    // rank name
                    rankName = match.Groups[3].Value;

                    // image URI
                    imageUri = match.Groups[4].Value;

                    // image URL
                    imageUrl = match.Groups[5].Value;

                    // add to grid
                    ladder.Add(new Level(number, requirement, rankName, imageUri, imageUrl));
                }
            }
            sr.Close();
        }

        private int getRequirement(int level)
        {
            int fullReq = 1;
            if (level == 1) { return 0; }
            if (level == 2) { return 1; }

            float requirement = 1.0f;
            for (int i = 0; i < level - 2; i++)
            {
                requirement += requirement * formula;
                fullReq = fullReq + (int)Math.Round(requirement, 0);
            }

            return fullReq;
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
            String sValue = value.ToString(CultureInfo.InvariantCulture);
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path, Encoding.UTF8), "formula=[0-9.]+", "formula=" + sValue));
        }

        private void writeHasData(Boolean value)
        {
            String sValue = value == true ? "1" : "0";
            File.WriteAllText(path, Regex.Replace(File.ReadAllText(path, Encoding.UTF8), "hasData=[01]", "hasData=" + sValue));
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            // refresh the grid
            if (refreshGrid())
            {
                // write basic properties to file
                writeIsCustom(isCustom);
                writeFormula(formula);
                writeLevels(levels);
                writeHasData(hasData);

                // erase old level data from file
                eraseGridDataFromFile();

                // write new level data to file
                writeGridDataToFile();

                this.Close();
            }
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

        private bool refreshGrid()
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
                invalidField = "levels text box";
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
                            ladder.Add(new Level(i + 1, getRequirement(i + 1), "", "", ""));
                        }
                    }
                }

                if (!isCustom)
                {
                    // edit grid data (requirements)
                    formula = tempFormula;
                    foreach (Level lvl in ladder)
                    {
                        lvl.Requirement = getRequirement(lvl.Number);
                    }
                }

                ranksGrid.Items.Refresh();

                return true;
            }
            else
            {
                if (multipleInvalidFields)
                {
                    MessageBox.Show("The values you provided in the " + invalidField + " are invalid");
                    return false;
                }
                else
                {
                    MessageBox.Show("The value you provided in the " + invalidField + " is invalid");
                    return false;
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

        // outside communication
        public int getPlayerLevel(int victories)
        {
            for (int i = 0; i < ladder.Count; i++)
            {
                if (!(victories >= ladder[ladder.Count - 1].Requirement))
                {
                    if (victories >= ladder[i].Requirement && victories < ladder[i + 1].Requirement)
                    {
                        return ladder[i].Number;
                    }
                }
                else
                {
                    // over the maximum
                    return ladder[ladder.Count - 1].Number;
                }
            }

            return -1;
        }

        public String getPlayerRank(int victories)
        {
            int level = getPlayerLevel(victories);

            return ladder[level - 1].RankName;
        }

        public String getPlayerImage(int victories)
        {
            int level = getPlayerLevel(victories);
            return ladder[level - 1].ImageUri;
        }

        private void ranksGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Header.ToString().Equals("ImageUri"))
            {
                e.Cancel = true;

                // show dialog for image selection
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();

                // set file dialog filters
                ofd.Filter = "Image files|*.jpeg;*.png;*.jpg";

                // display open file dialog
                Nullable<bool> result = ofd.ShowDialog();

                // get file name
                if (result == true)
                {
                    String originalFilePath = ofd.FileName;

                    // trim the file name
                    String fileName = originalFilePath.Substring(originalFilePath.LastIndexOf("\\") + 1);
                    String newPath = path.Substring(0, path.LastIndexOf("\\") + 1) + "icons";

                    // check id icon directory exists
                    if (!Directory.Exists(newPath))
                    {
                        // create icons directory
                        Directory.CreateDirectory(newPath);
                    }

                    // copy file to data directory
                    newPath = newPath + "\\" + fileName;
                    try
                    {
                        File.Copy(originalFilePath, newPath);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        MessageBox.Show("Unable to use the file; Restricted access!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    // record file name to data grid
                    ExtensionMethods.GetCell(ranksGrid, e.Row, e.Column.DisplayIndex).Content = fileName;
                    ladder.ElementAt(e.Row.GetIndex()).ImageUri = fileName;
                }
            }
        }
    }


    public class Level
    {
        public int Number { get; set; }
        public int Requirement { get; set; }
        public String RankName { get; set; }
        public String ImageUri { get; set; }
        public String ImageUrl { get; set; }

        public Level(int number, int requirement, String rankName, String imageUri, String imageUrl)
        {
            this.Number = number;
            this.Requirement = requirement;
            this.RankName = rankName;
            this.ImageUri = imageUri;
            this.ImageUrl = imageUrl;
        }
    }

    public static class ExtensionMethods
    {
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        public static DataGridRow GetSelectedRow(this DataGrid grid)
        {
            return (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem);
        }
        public static DataGridRow GetRow(this DataGrid grid, int index)
        {
            DataGridRow row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                // May be virtualized, bring into view and try again.
                grid.UpdateLayout();
                grid.ScrollIntoView(grid.Items[index]);
                row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        public static DataGridCell GetCell(this DataGrid grid, DataGridRow row, int column)
        {
            if (row != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(row);

                if (presenter == null)
                {
                    grid.ScrollIntoView(row, grid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(row);
                }

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                return cell;
            }
            return null;
        }
    }
}
