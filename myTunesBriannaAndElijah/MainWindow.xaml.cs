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

            // Claude: how do i show just the title property of each item in a ListBox
            // sets the playlistListBox to display the title property of each item
            playlistListBox.DisplayMemberPath = "title";
            // makes the playlistListBox actually display the titles
            playlistListBox.ItemsSource = musicRepo.SongTable.DefaultView;

            // makes the songsDataGrid display the songs from our musicRepo
            songsDataGrid.ItemsSource = musicRepo.SongTable.DefaultView;
        }
    }
}