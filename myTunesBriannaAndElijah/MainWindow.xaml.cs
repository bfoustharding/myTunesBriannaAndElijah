using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfMusicRepoDemo;

namespace myTunesBriannaAndElijah
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // creates a MusicRepo which we'll use throughout the program to handle the playlists and songs
        private readonly MusicRepo musicRepo = new MusicRepo();

        public MainWindow()
        {
            InitializeComponent();

            // fills the ListBox with the playlists from our musicRepo
            playlistListBox.ItemsSource = musicRepo.Playlists;
            // defaults selection to AllMusic playlist, which triggers
            // playlistListBox_SelectionChanged 
            playlistListBox.SelectedIndex = 0;
        }
        private void playlistListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (playlistListBox.SelectedItem is string name)
            {
                // updates the songsDataGrid to just show the songs for the selected playlist
                songsDataGrid.ItemsSource = musicRepo.SongsForPlaylist(name).DefaultView;
            }
        }
    }
}