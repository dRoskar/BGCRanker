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
    /// Interaction logic for PlayerPrompt.xaml
    /// </summary>
    public partial class PlayerPrompt : Window
    {
        private String playerName;
        private Boolean status;

        public PlayerPrompt()
        {
            InitializeComponent();
            txtBox.Focus();
            status = false;
        }

        private void playerCancleBtn_Click(object sender, RoutedEventArgs e)
        {
            status = false;
            this.Close();
        }

        private void playerOkBtn_Click(object sender, RoutedEventArgs e)
        {
            playerName = txtBox.Text;
            playerName = playerName.Trim();
            status = true;
            this.Close();
        }

        public String getPlayerName()
        {
            return playerName;
        }

        public Boolean getStatus()
        {
            return status;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key.Equals(Key.Enter)){
                if (txtBox.Text != "" && txtBox.Text != null)
                {
                    if (!playerCancleBtn.IsFocused)
                    {
                        playerName = txtBox.Text;
                        playerName = playerName.Trim();
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
