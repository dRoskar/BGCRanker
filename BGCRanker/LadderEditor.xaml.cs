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
using System.Windows.Shapes;
using System.IO;
using System.Globalization;

namespace BGCRanker
{
    /// <summary>
    /// Interaction logic for LadderEditor.xaml
    /// </summary>
    public partial class LadderEditor : Window
    {

        private StreamReader reader;
        private int levels;
        private float formula;
        private bool isCustom;

        public LadderEditor(StreamReader ladderReader)
        {
            InitializeComponent();
            reader = ladderReader;

            // get main properties
            getProperties();
            
            // display main properties
            showProperties();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cancleBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void getProperties(){
            // read data from file
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
            }
        }

        private void showProperties()
        {
            levelsTextBox.Text = levels.ToString();
            formulaTextBox.Text = formula.ToString();
            isCustomCheckBox.IsChecked = isCustom;
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
