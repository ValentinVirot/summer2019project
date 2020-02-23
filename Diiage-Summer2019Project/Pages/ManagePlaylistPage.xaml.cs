/*
 * Filename: /Pages/ManagePlaylistPage.xaml.cs
 * Created by: Valentin Virot, @valentin_vir
 * Description: Selected playlist management page
*/

// Adding dependencies
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Diiage_Summer2019Project.Pages
{
    public sealed partial class ManagePlaylistPage : Page
    {
        // Global objects
        BlindtestClass blindtest;
        BTPlaylist selected_playlist;
        DispatcherTimer downloadStatusTimer;
        DeezerTrack selected_result_track, selected_track;

        // Global variables
        int connected_user_index, selected_playlist_index;

        // Constructor
        public ManagePlaylistPage()
        {
            this.InitializeComponent();

            // Timer used to check preview download status
            downloadStatusTimerSetup();
        }

        // Timer used to lock play preview button when a song is downloading
        public void downloadStatusTimerSetup()
        {
            downloadStatusTimer = new DispatcherTimer();
            downloadStatusTimer.Tick += downloadStatusTimer_Tick;
            downloadStatusTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            downloadStatusTimer.Start();
        }

        // Called at each tick of timer
        private void downloadStatusTimer_Tick(object sender, object e)
        {
            if (blindtest.getDownloadStatus() == 4)
            {
                readPreview_button.Content = "Play preview";
                readPreview_button.IsEnabled = true;

                selectedTrack_readPreview_button.Content = "Play preview";
                selectedTrack_readPreview_button.IsEnabled = true;
            }
        }

        // Getting BlindtestClass Object from MainPage, then loading list of users
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // If a parameter is given, and if it's a BlindtestClass element
            if (e.Parameter is BlindtestClass)
            {
                // Get main element
                var bt = e.Parameter;
                blindtest = (BlindtestClass)bt;

                connected_user_index = blindtest.getConnectedUserIndex();
                selected_playlist = blindtest.getSelectedPlaylist();
                selected_playlist_index = blindtest.getSelectedPlaylistIndex();

                playlistInformation_label.Text = "List of tracks - " + selected_playlist.name;

                // Filling that listView element with some tracks
                ObservableCollection<DeezerTrack> listItems = new ObservableCollection<DeezerTrack>();
                connected_user_index = blindtest.getConnectedUserIndex();
                foreach (DeezerTrack track in selected_playlist.tracklist)
                {
                    listItems.Add(track);
                }
                tracklist_listView.ItemsSource = listItems;
            }
        }

        // Search track button
        private void SearchTrack_button_Click(object sender, RoutedEventArgs e)
        {
            results_label.Text = "Results:";

            // Checking if keywords has been given
            if(keywords_textBox.Text == "")
            {
                results_label.Text = "Keywords needed!";
            }

            else
            {
                results_label.Text = "Searching...";

                // Making API Call
                DeezerTrackSearch search_result = blindtest.searchTrackOnDeezer(keywords_textBox.Text);

                // if nothing went wrong
                if (search_result.error.type == null)
                {
                    results_comboBox.Items.Clear();

                    // Filling that comboBox
                    foreach (DeezerTrack track in search_result.data)
                    {
                        results_comboBox.Items.Add(track.title + " - " + track.artist.name);
                    }

                    if (results_comboBox.Items.Count > 0)
                    {
                        results_comboBox.SelectedIndex = 0;
                    }

                    results_label.Text = "Results:";
                }

                else
                {
                    // Display error cause
                    switch (search_result.error.code)
                    {
                        case 1:
                            results_label.Text = "Empty keywords!";
                            break;

                        case 2:
                            results_label.Text = "bad HttpResponseMessage code!";
                            break;

                        case 42:
                            results_label.Text = blindtest.getError();
                            break;

                        default:
                            results_label.Text = "See Deezer API documentation. Error code : " + search_result.error.code;
                            break;
                    }
                }
            }
        }

        // Reading preview
        private void ReadPreview_button_Click(object sender, RoutedEventArgs e)
        {
            if(!selected_result_track.Equals(default(DeezerTrack)))
            {
                readPreview_button.IsEnabled = false;
                selectedTrack_readPreview_button.IsEnabled = false;
                blindtest.readPreview(selected_result_track);
                readPreview_button.Content = "Downloading preview...";
            }
        }

        // Stop preview
        private void StopPreview_button_Click(object sender, RoutedEventArgs e)
        {
            blindtest.pausePreview();
        }

        // Adding selected track from previously made API call, and adding it to this playlist
        private void AddTrackToPlaylist_button_Click(object sender, RoutedEventArgs e)
        {
            // If no result has been selected
            if (selected_result_track.Equals(default(DeezerTrack)))
            {
                addTrackToPlaylist_button.Content = "You must select result first!";
            }

            else
            {
                switch (blindtest.addTrackToPlaylist(selected_result_track, blindtest.getConnectedUserIndex(), selected_playlist_index))
                {
                    case 0:
                        // Filling that listView with tracks
                        tracklist_listView.ItemsSource = new ObservableCollection<DeezerTrack>();
                        ObservableCollection<DeezerTrack> listItems = new ObservableCollection<DeezerTrack>();
                        connected_user_index = blindtest.getConnectedUserIndex();
                        foreach (DeezerTrack track in selected_playlist.tracklist)
                        {
                            listItems.Add(track);
                        }
                        tracklist_listView.ItemsSource = listItems;
                        break;

                    case 1:
                        addTrackToPlaylist_button.Content = "selected track is empty!";
                        break;

                    case 2:
                        addTrackToPlaylist_button.Content = "track already in playlist";
                        break;

                    case 42:
                        addTrackToPlaylist_button.Content = "Error: " + blindtest.getError();
                        break;
                }
            }
        }

        // Read preview of selected track
        private void SelectedTrack_readPreview_button_Click(object sender, RoutedEventArgs e)
        {
            if(!selected_track.Equals(default(DeezerTrack)))
            {
                selectedTrack_readPreview_button.IsEnabled = false;
                readPreview_button.IsEnabled = false;
                blindtest.readPreview(selected_track);
                selectedTrack_readPreview_button.Content = "Downloading preview...";
            }
        }

        // Resume preview of selected track
        private void SelectedTrack_resumePreview_button_Click(object sender, RoutedEventArgs e)
        {
            blindtest.resumePreview();
        }

        // Pause preview of selected track
        private void SelectedTrack_pausePreview_button_Click(object sender, RoutedEventArgs e)
        {
            blindtest.pausePreview();
        }

        // Remove selected track from playlist
        private void SelectedTrack_remove_button_Click(object sender, RoutedEventArgs e)
        {
            // If no track is selected
            if (tracklist_listView.SelectedIndex == -1)
            {
                selectedTrack_remove_button.Content = "You must select a track first!";
            }

            else
            {
                switch (blindtest.deleteTrackFromPlaylist(connected_user_index, selected_playlist_index, tracklist_listView.SelectedIndex))
                {
                    case 0:
                        // Filling that listView with some tracks
                        tracklist_listView.ItemsSource = new ObservableCollection<DeezerTrack>();
                        ObservableCollection<DeezerTrack> listItems = new ObservableCollection<DeezerTrack>();
                        connected_user_index = blindtest.getConnectedUserIndex();
                        foreach (DeezerTrack track in selected_playlist.tracklist)
                        {
                            listItems.Add(track);
                        }
                        tracklist_listView.ItemsSource = listItems;
                        BitmapImage bmpImg = new BitmapImage();
                        bmpImg.UriSource = new Uri("ms-appx:///Assets/default-track.png");
                        selectedTrack_Image.Source = bmpImg;
                        selectedTrack_title_label.Text = "deleted";
                        selectedTrack_album_label.Text = "deleted";
                        selectedTrack_artist_label.Text = "deleted";
                        selectedTrack_remove_button.Content = "Track deleted successfully!";
                        break;

                    case 42:
                        selectedTrack_remove_button.Content = blindtest.getError();
                        break;
                }
            }
        }

        // Rename playlist button
        private void RenamePlaylist_button_Click(object sender, RoutedEventArgs e)
        {
            // Check if user has typed new name
            if(newName_textBox.Text == "")
            {
                renamePlaylist_button.Content = "You need to type name first!";
            }

            else
            {
                switch(blindtest.renamePlaylist(connected_user_index, selected_playlist_index, newName_textBox.Text))
                {
                    case 0:
                        // Reloading UI elements
                        renamePlaylist_button.Content = "Playlist renamed successfully!";
                        selected_playlist = blindtest.getPlaylist(connected_user_index, selected_playlist_index);
                        blindtest.setSelectedPlaylist(selected_playlist);
                        playlistInformation_label.Text = "List of tracks - " + selected_playlist.name;
                        break;

                    case 1:
                        renamePlaylist_button.Content = "name or user index wrong!";
                        break;

                    case 42:
                        renamePlaylist_button.Content = blindtest.getError();
                        break;
                }
            }
        }

        // Delete playlist button
        private void DeletePlaylist_button_Click(object sender, RoutedEventArgs e)
        {
            // Check if user has typed his password
            if (passwordBox.Password == "")
            {
                deletePlaylist_button.Content = "You must type in your password!";
            }

            else
            {
                // Delete playlist using dedicated method
                switch(blindtest.deletePlaylist(blindtest.getSelectedUser(), passwordBox.Password, selected_playlist_index))
                {
                    case 0:
                        this.Frame.Navigate(typeof(PlaylistPage), blindtest);
                        break;

                    case 1:
                        deletePlaylist_button.Content = "User doesn't exist!";
                        break;

                    case 2:
                        deletePlaylist_button.Content = "Invalid password!";
                        break;

                    case 42:
                        deletePlaylist_button.Content = blindtest.getError();
                        break;
                }
            }
        }

        // When a result has been selected in comboBox, loading information in UI
        private void Results_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(results_comboBox.SelectedIndex != -1)
            {
                selected_result_track = blindtest.getTrack(results_comboBox.SelectedIndex);

                if (selected_result_track.Equals(default(DeezerTrack)))
                {
                    title_label.Text = "Error";
                    artist_label.Text = "Error";
                    album_label.Text = "Error";
                    BitmapImage bmpImg = new BitmapImage();
                    bmpImg.UriSource = new Uri("ms-appx:///Assets/default-track.png");
                    track_Image.Source = bmpImg;
                }

                else
                {
                    title_label.Text = selected_result_track.title;
                    artist_label.Text = selected_result_track.artist.name;
                    album_label.Text = selected_result_track.album.title;
                    BitmapImage bmpImg = new BitmapImage();
                    bmpImg.UriSource = new Uri(selected_result_track.album.cover_medium);
                    track_Image.Source = bmpImg;
                }
            }
        }

        // When a track has been selected in listView, loading elements
        private void Tracklist_listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(tracklist_listView.SelectedIndex == -1) {
                BitmapImage bmpImg = new BitmapImage();
                bmpImg.UriSource = new Uri("ms-appx:///Assets/default-track.png");
                selectedTrack_Image.Source = bmpImg;
                selectedTrack_title_label.Text = "Select track first!";
                selectedTrack_album_label.Text = "Select track first!";
                selectedTrack_artist_label.Text = "Select track first!";
            }

            else
            {
                selected_track = selected_playlist.tracklist[tracklist_listView.SelectedIndex];

                BitmapImage bmpImg = new BitmapImage();
                bmpImg.UriSource = new Uri(selected_track.album.cover_medium);
                selectedTrack_Image.Source = bmpImg;
                selectedTrack_title_label.Text = selected_track.title;
                selectedTrack_album_label.Text = selected_track.album.title;
                selectedTrack_artist_label.Text = selected_track.artist.name;
            }
        }

        // Go back to previous page
        private void Back_button_Click(object sender, RoutedEventArgs e)
        {
            blindtest.pausePreview();
            this.Frame.Navigate(typeof(PlaylistPage), blindtest);
        }
    }
}
