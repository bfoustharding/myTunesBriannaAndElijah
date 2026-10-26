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
using System.Xml.Linq;
using Microsoft.Win32;
using WpfMusicRepoDemo;

namespace myTunesBriannaAndElijah
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly RoutedUICommand RemoveCommand = 
            new RoutedUICommand("Remove", "Remove", typeof(MainWindow));

        // creates a MusicRepo which we'll use throughout the program to handle the playlists and songs
        private readonly MusicRepo musicRepo = new MusicRepo();

        // plays the audio files
        private readonly MediaPlayer player = new();
        private bool isPlaying = false;

        public MainWindow()
        {
            InitializeComponent();

            // Moves onto the next song on finish, and show error when fail
            player.MediaEnded += player_MediaEnded;
            player.MediaFailed += player_MediaFailed;

            // fills the ListBox with the playlists from our musicRepo
            playlistListBox.ItemsSource = musicRepo.Playlists;
            // defaults selection to AllMusic playlist, which triggers playlistListBox_SelectionChanged 
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

        // Add Song toolbar button, opens a dialog for user to select audio file, add it to musicRepo
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

        // New Playlist toolbar button, prompts the user for a name and makes the playlist
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

        // About toolbar button, shows the About Dialog box
        private void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new();
            about.ShowDialog();
        }

        // Play is only able to execute when a song is selected
        private void playCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = songsDataGrid.SelectedItem is DataRowView;
        }

        private void playCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PlaySelectedSong();
        }

        // Stop is only able to execute when a song is playing
        private void stopCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isPlaying;
        }

        private void stopCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StopSong();
        }

        // Plays the song currently selected
        // Claude helped with getting the song
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
            CommandManager.InvalidateRequerySuggested();
        }

        private void StopSong()
        {
            player.Stop();
            isPlaying = false;
            CommandManager.InvalidateRequerySuggested();
        }

        // Select a row if a user selects it
        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                row.IsSelected = true;
            }
        }

        // Play a song when a user double clicks on it
        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlaySelectedSong();
        }

        // When a song ends, move to the next song in the playlist
        private void player_MediaEnded(object? sender, EventArgs e)
        {
            int next = songsDataGrid.SelectedIndex + 1;
            if (next < songsDataGrid.Items.Count)
            {
                songsDataGrid.SelectedIndex = next;
                songsDataGrid.ScrollIntoView(songsDataGrid.SelectedItem);
                PlaySelectedSong();
            }
            else
            {
                StopSong();
            }
        }

        private void player_MediaFailed(object? sender, ExceptionEventArgs e)
        {
            StopSong();
            MessageBox.Show("Could not play the song: " + e.ErrorException.Message, "myTunes", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // If the mouse is pressed and a song is selected, start a drag & drop copy operation with the selected song's data
        private void songsDataGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || songsDataGrid.SelectedItem == null) 
                return;
            
            DataObject songData = new DataObject("song", songsDataGrid.SelectedItem);

            DragDrop.DoDragDrop(songsDataGrid, songData, DragDropEffects.Copy);
        }

        // Add the dragged song to the playlist it was dropped on by song ID and playlist name
        private void playlistItem_Drop(object sender, DragEventArgs e)
        {
            int songId = Convert.ToInt32(((DataRowView)e.Data.GetData("song"))["id"]);

            string playlistName = (string)((ListBoxItem)sender).Content;

            if (playlistName == "All Music") 
                return;

            musicRepo.AddSongToPlaylist(songId, playlistName);
        }

        // Check to make sure we actually have a song selected
        private void removeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = songsDataGrid.SelectedItem is DataRowView;

        }

        // If we are on the All Music playlist, check to make sure the user's sure, then call DeleteSong to remove the song from
        // all playlists. Otherwise, call RemoveSongFromPlaylist to just remove this song occurrence from the playlist
        private void removeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DataRowView songRow = (DataRowView)songsDataGrid.SelectedItem;
            int songId = Convert.ToInt32(songRow["id"]);
            string playlist = (string)playlistListBox.SelectedItem;

            if (playlist == "All Music")
            {
                MessageBoxResult answer = MessageBox.Show("Remove from all playlists?", "Remove", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (answer == MessageBoxResult.No)
                    return;

                musicRepo.DeleteSong(songId);
            }
            else
            {
                // Take the position of the song (whether it's 1st, 2nd, etc)
                int position = Convert.ToInt32(songRow["position"]);

                musicRepo.RemoveSongFromPlaylist(position, songId, playlist);
            }
        }

        // When the window closes, save any changes to playlists (to music.xml)
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            musicRepo.Save();
        }

        // To avoid right-clicks on the extra space in the playlist list box opening the Rename/Delete context menu,
        // this boolean keeps track of whether an actual playlist item was clicked on
        private bool clickedOnPlaylist = false;
        private void playlistItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((ListBoxItem)sender).IsSelected = true;
            clickedOnPlaylist = true;
        }
        
        // If the right-click was not on a playlist, prevent the context menu from being opened
        private void playlistListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (!clickedOnPlaylist)
                e.Handled = true;
            else
                clickedOnPlaylist = false;
        }

        // Check to make sure we're not trying to rename All Music. Then, get the new playlist name and,
        // if it's valid, rename the playlist
        private void renamePlaylistMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string oldPlaylistName = (string)playlistListBox.SelectedItem;

            if (oldPlaylistName == "All Music") 
            {
                MessageBox.Show("Can't rename All Music", "myTunes", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PlaylistNameWindow dialog = new();
            if (dialog.ShowDialog() != true)
                return;

            string newPlaylistName = dialog.PlaylistName;

            if (!(musicRepo.RenamePlaylist(oldPlaylistName, newPlaylistName))) {
                MessageBox.Show("Playlist name can't be blank or repeated", "myTunes", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            playlistListBox.ItemsSource = musicRepo.Playlists;
            playlistListBox.SelectedItem = newPlaylistName;
        }

        // Check to make sure the selected playlist isn't all music, then delete playlist, and go to All Music
        private void deletePlaylistMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string name = (string)playlistListBox.SelectedItem;

            if (name == "All Music")
            {
                MessageBox.Show("All Music can't be deleted.", "myTunes", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            musicRepo.DeletePlaylist(name);
            playlistListBox.ItemsSource = musicRepo.Playlists;
            playlistListBox.SelectedItem = "All Music";
        }

        // Get the DataView on the DataGrid in order to filter the songs. If nothing's entered in the search box,
        // show all songs. Otherwise, filter songs by those that have a title, artist, etc. like the search.
        // Claude: how do I use a DataView.RowFilter with a LIKE statement to filter datarows with a specific string
        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DataView? dataView = songsDataGrid.ItemsSource as DataView;

            string searchText = searchTextBox.Text;

            if (searchText == "")
            {
                dataView.RowFilter = "";
            }
            else
            {
                dataView.RowFilter = $"title LIKE '%{searchText}%' OR artist LIKE '%{searchText}%' " +
                                 $"OR album LIKE '%{searchText}%' OR genre LIKE '%{searchText}%'";
            }
        }
    }
}