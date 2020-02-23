/*
 * Filename: /Pages/ManageGamePage.xaml.cs
 * Created by: Valentin Virot, @valentin_vir
 * Description: Selected game management page
*/

// Adding dependencies
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Diiage_Summer2019Project.Pages
{
    public sealed partial class ManageGamePage : Page
    {
        // Global Objects
        BlindtestClass blindtest;
        BTUser connected_user;
        BTGame selected_game;
        DispatcherTimer downloadStatusTimer;
        DeezerTrack selected_track;
        List<BTScoreboard> scoreboard = new List<BTScoreboard>();

        // Constructor
        public ManageGamePage()
        {
            this.InitializeComponent();

            // Initializing timer (used to check preview download status)
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

                // Getting user and game information
                connected_user = blindtest.getConnectedUser();
                selected_game = blindtest.getSelectedGame();

                // loading game info in UI, and filling listView with tracks according to datatemplate
                gameInformation_label.Text = "List of tracks - " + selected_game.game_title;

                ObservableCollection<DeezerTrack> listItems = new ObservableCollection<DeezerTrack>();
                foreach (DeezerTrack track in selected_game.tracklist)
                {
                    listItems.Add(track);
                }
                tracklist_listView.ItemsSource = listItems;

                // Loading scoreboard for this game
                foreach(BTGameHistory history in selected_game.scores.history)
                {
                    BTScoreboard score = new BTScoreboard();
                    score.username = "Username not found";
                    score.profile_picture = "ms-appx:///Assets/Users/unselected-user.png";

                    foreach (BTUser user in blindtest.getAllUsers())
                    {
                        if(user.user_id == history.user_id)
                        {
                            score.username = user.nickname;
                            score.profile_picture = user.profile_picture;
                        }
                    }

                    score.date = history.date;
                    score.score = history.score.ToString();
                    scoreboard.Add(score);
                }

                // Filling scoreboard listView with scores loaded before according to dataTemplate
                ObservableCollection<BTScoreboard> listItems2 = new ObservableCollection<BTScoreboard>();
                foreach (BTScoreboard score in scoreboard)
                {
                    listItems2.Add(score);
                }
                scoreboard_listView.ItemsSource = listItems2;
            }
        }

        // Read preview of that selected track
        private void SelectedTrack_readPreview_button_Click(object sender, RoutedEventArgs e)
        {
            if (!selected_track.Equals(default(DeezerTrack)))
            {
                selectedTrack_readPreview_button.IsEnabled = false;
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

        // When a track is selected in dedicated listView
        private void Tracklist_listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Loading UI element with correct information according to what has been selected by user
            if (tracklist_listView.SelectedIndex == -1)
            {
                BitmapImage bmpImg = new BitmapImage();
                bmpImg.UriSource = new Uri("ms-appx:///Assets/default-track.png");
                selectedTrack_Image.Source = bmpImg;
                selectedTrack_title_label.Text = "Select track first!";
                selectedTrack_album_label.Text = "Select track first!";
                selectedTrack_artist_label.Text = "Select track first!";
            }

            else
            {
                selected_track = selected_game.tracklist[tracklist_listView.SelectedIndex];

                BitmapImage bmpImg = new BitmapImage();
                bmpImg.UriSource = new Uri(selected_track.album.cover_medium);
                selectedTrack_Image.Source = bmpImg;
                selectedTrack_title_label.Text = selected_track.title;
                selectedTrack_album_label.Text = selected_track.album.title;
                selectedTrack_artist_label.Text = selected_track.artist.name;
            }
        }

        // Rename Game button
        private void RenameGame_button_Click(object sender, RoutedEventArgs e)
        {
            // Check if new name has been given
            if (newName_textBox.Text == "")
            {
                renameGame_button.Content = "You need to type name first!";
            }

            else
            {
                // Calling that dedicated method
                switch (blindtest.renameGame(selected_game.game_id, connected_user.user_id, newName_textBox.Text))
                {
                    case 0:
                        renameGame_button.Content = "Game renamed successfully!";
                        selected_game.game_title = newName_textBox.Text;
                        blindtest.setSelectedGame(selected_game);
                        gameInformation_label.Text = "List of tracks - " + selected_game.game_title;
                        break;

                    case 1:
                        renameGame_button.Content = "name or user index wrong!";
                        break;

                    case 2:
                        renameGame_button.Content = "This user is not in database!";
                        break;

                    case 3:
                        renameGame_button.Content = "This game isn't created by you!";
                        break;

                    case 42:
                        renameGame_button.Content = blindtest.getError();
                        break;
                }
            }
        }

        // Delete game button
        private void DeleteGame_button_Click(object sender, RoutedEventArgs e)
        {
            // Check if password has been typed by user
            if (passwordBox.Password == "")
            {
                deleteGame_button.Content = "You must type in your password!";
            }

            else
            {
                // Calling that dedicated method
                switch (blindtest.deleteGame(blindtest.getSelectedUser(), blindtest.getSelectedGame(), passwordBox.Password))
                {
                    case 0:
                        this.Frame.Navigate(typeof(GameManagementSelectionPage), blindtest);
                        break;

                    case 1:
                        deleteGame_button.Content = "User doesn't exist in DB!";
                        break;

                    case 2:
                        deleteGame_button.Content = "Game doesn't exist in DB!";
                        break;

                    case 3:
                        deleteGame_button.Content = "Game isn't created by this user!";
                        break;

                    case 4:
                        deleteGame_button.Content = "Invalid password!";
                        break;

                    case 42:
                        deleteGame_button.Content = blindtest.getError();
                        break;
                }
            }
        }

        // When a scoreboard entry has been selected by user, loading UI element
        private void Scoreboard_listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(scoreboard_listView.SelectedIndex != -1)
            {
                selectedUser_username_label.Text = scoreboard[scoreboard_listView.SelectedIndex].username;
                selectedUser_date_label.Text = scoreboard[scoreboard_listView.SelectedIndex].date;
                selectedUser_score_label.Text = scoreboard[scoreboard_listView.SelectedIndex].score + " points";
                BitmapImage bmpImg = new BitmapImage();
                bmpImg.UriSource = new Uri(scoreboard[scoreboard_listView.SelectedIndex].profile_picture);
                ppUser_Image.Source = bmpImg;
            }
        }

        // Go back to previous page
        private void Back_button_Click(object sender, RoutedEventArgs e)
        {
            blindtest.pausePreview();
            this.Frame.Navigate(typeof(GameManagementSelectionPage), blindtest);
        }
    }
}
