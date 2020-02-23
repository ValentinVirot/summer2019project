/*
 * Filename: /Pages/CreateUserPage.xaml.cs
 * Created by: Valentin Virot, @valentin_vir
 * Description: User creation Page
*/

// Adding dependencies
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Diiage_Summer2019Project.Pages
{
    public sealed partial class CreateGamePage : Page
    {
        // Global objects
        BlindtestClass blindtest;
        BTUser connected_user;
        BTPlaylist selected_playlist;

        // Global variables
        int connected_user_index, title_score, artist_score, album_score, duration_score;

        // Constructor
        public CreateGamePage()
        {
            this.InitializeComponent();
        }

        // When a playlist is selected in dedicated combobox
        private void BasePlaylist_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(basePlaylist_comboBox.SelectedIndex == -1)
            {
                selected_playlist = new BTPlaylist();
            }

            else
            {
                selected_playlist = blindtest.getPlaylist(connected_user_index, basePlaylist_comboBox.SelectedIndex);
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

                // Get connected user information
                connected_user = blindtest.getConnectedUser();
                connected_user_index = blindtest.getConnectedUserIndex();

                // Loading each playlist created by connected user
                foreach(BTPlaylist playlist in blindtest.getAllPlaylists(connected_user.user_id))
                {
                    basePlaylist_comboBox.Items.Add(playlist.name + " - N° of tracks: " + playlist.tracklist.Count.ToString() + " tracks");
                }
            }
        }

        // Go back to previous page
        private void Back_button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(GameManagementSelectionPage), blindtest);
        }
        
        // When CreateGame button is clicked
        private void CreateGame_button_Click(object sender, RoutedEventArgs e)
        {
            // Disabling UI elements
            createGame_button.IsEnabled = false;
            back_button.IsEnabled = false;

            // Check if given information is empty (textBoxes
            if (gameTitle_textBox.Text == "" || basePlaylist_comboBox.SelectedIndex == -1 || titleScore_textBox.Text == "" || artistScore_textBox.Text == "" || albumScore_textBox.Text == "" || durationScore_textBox.Text == "")
            {
                status_label.Text = "You need to give every information!";
            }

            else
            {
                // If selected playlist doesn't have any track in it
                if(selected_playlist.tracklist.Count == 0)
                {
                    status_label.Text = "This playlist is empty!";
                }

                else
                {
                    // Trying to parse every score
                    try
                    {
                        title_score = Int32.Parse(titleScore_textBox.Text);
                        artist_score = Int32.Parse(artistScore_textBox.Text);
                        album_score = Int32.Parse(albumScore_textBox.Text);
                        duration_score = Int32.Parse(durationScore_textBox.Text);

                        // Create game in database using dedicated method
                        switch (blindtest.createGame(connected_user.user_id, gameTitle_textBox.Text, DateTime.Now.ToString(), blindtest.getPlaylist(connected_user_index, basePlaylist_comboBox.SelectedIndex), title_score, artist_score, album_score, duration_score))
                        {
                            case 0:
                                status_label.Text = "Game created successfully!";
                                break;

                            case 1:
                                status_label.Text = "User not in database.";
                                break;

                            case 2:
                                status_label.Text = "You must give every information!";
                                break;

                            case 3:
                                status_label.Text = "Playlist is empty!";
                                break;

                            case 4:
                                status_label.Text = "Invalid scores!";
                                break;

                            case 5:
                                status_label.Text = "You already gave this game title!";
                                break;

                            case 42:
                                status_label.Text = blindtest.getError();
                                break;
                        }
                    }

                    catch
                    {
                        status_label.Text = "Scores must be integer numbers only!";
                    }
                }
            }

            // Enabling UI Element
            createGame_button.IsEnabled = true;
            back_button.IsEnabled = true;
        }
    }
}
