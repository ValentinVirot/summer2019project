/*
 * Filename: /Pages/SelectedGameScoreboard.xaml.cs
 * Created by: Valentin Virot, @valentin_vir
 * Description: Scoreboard page of selected game
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
    public sealed partial class SelectedGameScoreboard : Page
    {
        // Global objects
        BlindtestClass blindtest;
        BTGame selected_game;
        List<BTScoreboard> scoreboard = new List<BTScoreboard>();

        // Global variables
        int maximum_score = 0, nbr_time_played = 0;

        // Constructor
        public SelectedGameScoreboard()
        {
            this.InitializeComponent();
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

                selected_game = blindtest.getSelectedGame();

                gameInformation_label.Text = "List of tracks - " + selected_game.game_title;

                // Loading every score history entry in dedicated listView, and loading UI elements
                ObservableCollection<DeezerTrack> listItems = new ObservableCollection<DeezerTrack>();

                description_label.Text = "Here is the scoreboard for '" + selected_game.game_title + "' game! Just click on an entry to see all of it's details!";
                gameInformation_label.Text = "Scoreboard - " + selected_game.game_title;
                maximum_score = (selected_game.scores.title * selected_game.tracklist.Count) + (selected_game.scores.artist * selected_game.tracklist.Count) + (selected_game.scores.album * selected_game.tracklist.Count) + (selected_game.scores.duration * selected_game.tracklist.Count);
                maxScore_label.Text = maximum_score.ToString() + " points";

                foreach (BTGameHistory history in selected_game.scores.history)
                {
                    BTScoreboard score = new BTScoreboard();
                    score.username = "Username not found";
                    score.profile_picture = "ms-appx:///Assets/Users/unselected-user.png";
                    score.user_id = history.user_id;

                    foreach (BTUser user in blindtest.getAllUsers())
                    {
                        if (user.user_id == history.user_id)
                        {
                            score.username = user.nickname;
                            score.profile_picture = user.profile_picture;
                        }
                    }

                    score.date = history.date;
                    score.score = history.score.ToString();
                    scoreboard.Add(score);
                }

                ObservableCollection<BTScoreboard> listItems2 = new ObservableCollection<BTScoreboard>();

                foreach (BTScoreboard score in scoreboard)
                {
                    listItems2.Add(score);
                }

                scoreboard_listView.ItemsSource = listItems2;
            }
        }

        // When a scoreboard entry is selected
        private void Scoreboard_listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            nbr_time_played = 0;

            // if it's a correct entry
            if (scoreboard_listView.SelectedIndex != -1)
            {
                // Loading every information in UI
                username_label.Text = scoreboard[scoreboard_listView.SelectedIndex].username;
                date_label.Text = scoreboard[scoreboard_listView.SelectedIndex].date;
                score_label.Text = scoreboard[scoreboard_listView.SelectedIndex].score + " points";

                foreach(BTGameHistory history in selected_game.scores.history)
                {
                    if(history.user_id == scoreboard[scoreboard_listView.SelectedIndex].user_id)
                    {
                        nbr_time_played++;
                    }
                }
                nbrTimePlayed_label.Text = nbr_time_played.ToString() + " times";

                int games_created = 0, games_played = 0;
                BTUser user = blindtest.getUserByUserID(scoreboard[scoreboard_listView.SelectedIndex].user_id);

                foreach (BTGame game in blindtest.getAllGames())
                {
                    if (game.user_id == user.user_id)
                    {
                        games_created++;
                    }

                    foreach (BTGameHistory history in game.scores.history)
                    {
                        if (history.user_id == user.user_id)
                        {
                            games_played++;
                        }
                    }
                }

                if (games_played > 1)
                {
                    nbrGamesPlayed_label.Text = games_played.ToString() + " games";
                }

                else
                {
                    nbrGamesPlayed_label.Text = games_played.ToString() + " game";
                }

                if (games_created > 1)
                {
                    nbrGamesCreated_label.Text = games_created.ToString() + " games";
                }

                else
                {
                    nbrGamesCreated_label.Text = games_created.ToString() + " game";
                }

                //selectedUser_score_label.Text = scoreboard[scoreboard_listView.SelectedIndex].score + " points";
                BitmapImage bmpImg = new BitmapImage();
                bmpImg.UriSource = new Uri(scoreboard[scoreboard_listView.SelectedIndex].profile_picture);
                ppUser_Image.Source = bmpImg;
            }
        }

        // Go back to previous page button
        private void Back_button_Click(object sender, RoutedEventArgs e)
        {
            blindtest.pausePreview();
            this.Frame.Navigate(typeof(PlayGameSelectionPage), blindtest);
        }
    }
}
