/*
 * Filename: /Pages/PlaylistPage.xaml.cs
 * Created by: Valentin Virot, @valentin_vir
 * Description: Playlist selection page
*/
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Diiage_Summer2019Project.Pages
{
    public sealed partial class PlaylistPage : Page
    {
        // Global Objects
        BlindtestClass blindtest;
        BTUser connected_user;

        // Global variables
        int connected_user_index = -1;

        // Constructor
        public PlaylistPage()
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

                // Filling listView with playlists, and loading UI elements
                ObservableCollection<BTPlaylist> listItems = new ObservableCollection<BTPlaylist>();

                connected_user = blindtest.getConnectedUser();
                connected_user_index = blindtest.getConnectedUserIndex();

                BitmapImage bmpImg = new BitmapImage();
                bmpImg.UriSource = new Uri(connected_user.profile_picture);
                userPP_Image.Source = bmpImg;

                foreach (BTPlaylist playlist in blindtest.getAllPlaylists(connected_user.user_id))
                {
                    listItems.Add(playlist);
                }

                playlists_listView.ItemsSource = listItems;

                username_label.Text = connected_user.nickname;

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
            this.Frame.Navigate(typeof(LibraryPage), blindtest);
        }

        // Create playlist button
        private void CreatePlaylist_button_Click(object sender, RoutedEventArgs e)
        {
            switch(blindtest.createPlaylist(connected_user_index, newPlaylistName_textBox.Text))
            {
                // Reloading listView if playlist has been created successfully
                case 0:
                    playlists_listView.ItemsSource = new ObservableCollection<BTPlaylist>();
                    ObservableCollection<BTPlaylist> listItems = new ObservableCollection<BTPlaylist>();
                    foreach (BTPlaylist playlist in blindtest.getAllPlaylists(connected_user.user_id))
                    {
                        listItems.Add(playlist);
                    }
                    playlists_listView.ItemsSource = listItems;
                    createPlaylist_button.Content = "Playlist created successfully!";
                    break;

                case 1:
                    createPlaylist_button.Content = "name or user index wrong!";
                    break;

                case 2:
                    createPlaylist_button.Content = "name already given!";
                    break;

                case 42:
                    createPlaylist_button.Content = blindtest.getError();
                    break;
            }
        }

        // ManagePlaylist button
        private void ManagePlaylist_button_Click(object sender, RoutedEventArgs e)
        {
            if(playlists_listView.SelectedIndex == -1)
            {
                managePlaylist_button.Content = "Select playlist first!";
            }

            else
            {
                blindtest.setSelectedPlaylist(blindtest.getPlaylist(blindtest.getSelectedUserIndex(), playlists_listView.SelectedIndex));
                blindtest.setSelectedPlaylistIndex(playlists_listView.SelectedIndex);
                this.Frame.Navigate(typeof(ManagePlaylistPage), blindtest);
            }
        }
    }
}
