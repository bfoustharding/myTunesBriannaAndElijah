using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace myTunesBriannaAndElijah
{
    /// <summary>
    /// Interaction logic for PlaylistNameWindow.xaml
    /// </summary>
    public partial class PlaylistNameWindow : Window
    {
        // property of the name the user typed
        public string PlaylistName { get; private set; } = "";
        
        
        public PlaylistNameWindow(string title = "New Playlist", string initialName = "")
        {
            InitializeComponent();
            Title = title;
            nameTextBox.Text = initialName;
            nameTextBox.SelectAll();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            PlaylistName = nameTextBox.Text.Trim();
            if (PlaylistName == "")
            {
                MessageBox.Show("Please enter a playlist name.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
        }
    }
}
