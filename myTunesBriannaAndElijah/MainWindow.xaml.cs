using Microsoft.Win32;
using System.Data;
using System.Runtime.CompilerServices;
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

        private readonly MediaPlayer player = new();
        private bool isPlaying = false;

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

        private void addSongButton_Click(object sender, RoutedEventArgs e)
        {
            // Claude helped write the filter
            OpenFileDialog dialog = new() 
            {
                Title = "Add Song",
                Filter = "Audio files|*.mp3;*.m4a;*.wma;*.wav|All files|*.*",
            };

            if (dialog.ShowDialog() != true) { return; }

            Song song;
            try
            {
                song = musicRepo.AddSong(dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not add song " + ex.Message, "myTunes", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Refresh the grid, so the new song is visible
            playlistListBox.SelectedIndex = 0;
            songsDataGrid.ItemsSource = musicRepo.SongsForPlaylist("All Music").DefaultView;

            //Claude helped write selecting the new song
            foreach (DataRowView item in songsDataGrid.Items)
            {
                if (item["id"].ToString() == song.Id.ToString()) 
                {
                    songsDataGrid.SelectedItem = item;
                    songsDataGrid.ScrollIntoView(item);
                    break;
                }
            }
        }

        private void newPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            PlaylistNameWindow dialog = new();

            if (dialog.ShowDialog() != true) { return; }

            string name = dialog.PlaylistName;

            if (name == "All Music" || !musicRepo.AddPlaylist(name))
            {
                MessageBox.Show($"A playlist named \"{name}\" already exists.", "myTunes", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            playlistListBox.ItemsSource = musicRepo.Playlists;
            playlistListBox.SelectedItem = name;
        }

        private void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new();
            about.ShowDialog();
        }

        private void playCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = songsDataGrid.SelectedItem is DataRowView;
        }

        private void playCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PlaySelectedSong();
        }

        private void stopCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isPlaying;
        }

        private void stopCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StopSong();
        }

        private void PlaySelectedSong()
        {
            if (songsDataGrid.SelectedItem is not DataRowView row) { return; }

            Song? song = musicRepo.GetSong(Convert.ToInt32(row["id"]));

            if (song == null || !System.IO.File.Exists(song.Filename))
            {
                MessageBox.Show("The song file could not be found:\n" + song?.Filename, "myTunes", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            player.Open(new Uri(song.Filename));
            player.Play();
            isPlaying = true;
        }

        private void StopSong() { 
            player.Stop();
            isPlaying = false;
        }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                row.IsSelected = true;
            }
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlaySelectedSong();
        }

        //TODO: When a song ends, move to the next song in the playlist
    }
}