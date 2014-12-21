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

namespace BGCRanker
{
    /// <summary>
    /// Interaction logic for LadderEditor.xaml
    /// </summary>
    public partial class LadderEditor : Window
    {

        public LadderEditor(StreamReader ladderReader)
        {
            InitializeComponent();
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
}
