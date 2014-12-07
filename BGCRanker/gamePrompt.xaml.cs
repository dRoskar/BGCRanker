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

namespace BGCRanker
{
    /// <summary>
    /// Interaction logic for gamePrompt.xaml
    /// </summary>
    public partial class gamePrompt : Window
    {
        private String gameName;
        private Boolean status;

        public gamePrompt()
        {
            InitializeComponent();
            gamePromptTextBox.Focus();
            status = false;
        }

        private void ganeOkBtn_Click(object sender, RoutedEventArgs e)
        {
            gameName = gamePromptTextBox.Text;
            gameName = gameName.Trim();
            status = true;
            this.Close();
        }

        private void gameCancleBtn_Click(object sender, RoutedEventArgs e)
        {
            status = false;
            this.Close();
        }

        public String getgameName()
        {
            return gameName;
        }

        public Boolean getStatus()
        {
            return status;
        }

        private void newGamePrompt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                if (gamePromptTextBox.Text != "" && gamePromptTextBox.Text != null)
                {
                    if (!gameCancleBtn.IsFocused)
                    {
                        gameName = gamePromptTextBox.Text;
                        gameName = gameName.Trim();
                        status = true;
                        this.Close();
                    }
                    else
                    {
                        status = false;
                        this.Close();
                    }
                }
            }
        }
    }
}
