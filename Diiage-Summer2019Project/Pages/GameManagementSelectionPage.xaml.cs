/*
 * Filename: /Pages/GameManagementSelectionPage.xaml.cs
 * Created by: Valentin Virot, @valentin_vir
 * Description: Page where you can select a game to edit
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
    public sealed partial class GameManagementSelectionPage : Page
    {
        // Global Objects
        BlindtestClass blindtest;
        BTUser connected_user;
        BTGame selected_game;
        List<BTGame> all_games = new List<BTGame>();

        // Constructor
        public GameManagementSelectionPage()
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

                // Getting connected user
                connected_user = blindtest.getConnectedUser();

                // Initializing listView items (according to dataTemplate)
                ObservableCollection<BTGame> listItems = new ObservableCollection<BTGame>();

                // Loading user information in page header
                BitmapImage bmpImg = new BitmapImage();
                bmpImg.UriSource = new Uri(connected_user.profile_picture);
                userPP_Image.Source = bmpImg;

                // Filling listView with all games
                all_games = blindtest.getGames(connected_user.user_id);
                foreach (BTGame game in all_games)
                {
                    listItems.Add(game);
                }
                games_listView.ItemsSource = listItems;
                username_label.Text = connected_user.nickname;

                // User information (nbr game played etc)
                int games_created = 0, games_played = 0, playlists_created = 0;
                foreach (BTGame game in blindtest.getAllGames())
                {
                    if (game.user_id == connected_user.user_id)
                    {
                        games_created++;
                    }

                    foreach (BTGameHistory history in game.scores.history)
                    {
                        if (history.user_id == connected_user.user_id)
                        {
                            games_played++;
                        }
                    }
                }
                if (games_played > 1)
                {
                    nbrPlayedGames_label.Text = games_played.ToString() + " games";
                }
                else
                {
                    nbrPlayedGames_label.Text = games_played.ToString() + " game";
                }
                if (games_created > 1)
                {
                    nbrGames_label.Text = games_created.ToString() + " games";
                }
                else
                {
                    nbrGames_label.Text = games_created.ToString() + " game";
                }

                playlists_created = blindtest.getAllPlaylists(connected_user.user_id).Count;
                if (playlists_created > 1)
                {
                    nbrPlaylists_label.Text = playlists_created.ToString() + " playlists";
                }
                else
                {
                    nbrPlaylists_label.Text = playlists_created.ToString() + " playlist";
                }
            }
        }

        // Go back to previous page
        private void Back_button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(GamePage), blindtest);
        }

        // When a game is selected in listView
        private void Games_listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(games_listView.SelectedIndex != -1)
            {
                selected_game = all_games[games_listView.SelectedIndex];
                gamesInformation_label.Text = "Game information - Selected game: " + selected_game.game_title;
            }

            else
            {
                selected_game = new BTGame();
                gamesInformation_label.Text = "Game information - Selected game: none";
            }
        }

        // Create Game button
        private void CreateGame_button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CreateGamePage), blindtest);
        }

        // Manage Game button
        private void ManageGame_button_Click(object sender, RoutedEventArgs e)
        {
            if (games_listView.SelectedIndex == -1)
            {
                manageGame_button.Content = "Select game first!";
            }

            else
            {
                blindtest.setSelectedGame(all_games[games_listView.SelectedIndex]);
                this.Frame.Navigate(typeof(ManageGamePage), blindtest);
            }
        }
    }
}
