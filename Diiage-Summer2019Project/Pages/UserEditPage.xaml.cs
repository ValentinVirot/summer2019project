/*
 * Filename: UserEditPage.xaml.cs
 * Created by: Valentin Virot, @valentin_vir
 * Description: User editing page
*/

// Adding dependencies
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Diiage_Summer2019Project.Pages
{
    public sealed partial class UserEditPage : Page
    {
        // Global Objects
        BlindtestClass blindtest;
        BTUser selected_user;

        // Global variables
        int games_played = 0, games_created = 0, playlists_created = 0;

        // Constructor
        public UserEditPage()
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

                // Get that selected user
                selected_user = blindtest.getSelectedUser();

                // Show user's info on page
                userID_label.Text = selected_user.user_id.ToString();
                username_label.Text = selected_user.nickname;
                newUser_textBox.Text = selected_user.nickname;
                BitmapImage bmpImg = new BitmapImage();
                bmpImg.UriSource = new Uri(selected_user.profile_picture);
                profilePicture_Image.Source = bmpImg;
                profilePicture_textBox.Text = selected_user.profile_picture;
                newPP_textBox.Text = selected_user.profile_picture;

                foreach(BTGame game in blindtest.getAllGames())
                {
                    if(game.user_id == selected_user.user_id)
                    {
                        games_created++;
                    }

                    foreach(BTGameHistory history in game.scores.history)
                    {
                        if(history.user_id == selected_user.user_id)
                        {
                            games_played++;
                        }
                    }
                }

                if(games_played > 1)
                {
                    gamesPlayed_label.Text = games_played.ToString() + " games";
                }

                else
                {
                    gamesPlayed_label.Text = games_played.ToString() + " game";
                }

                if (games_created > 1)
                {
                    gamesCreated_label.Text = games_created.ToString() + " games";
                }

                else
                {
                    gamesCreated_label.Text = games_created.ToString() + " game";
                }

                playlists_created = blindtest.getAllPlaylists(selected_user.user_id).Count;

                if (playlists_created > 1)
                {
                    playlistsCreated_label.Text = playlists_created.ToString() + " playlists";
                }

                else
                {
                    playlistsCreated_label.Text = playlists_created.ToString() + " playlist";
                }
            }
        }

        // Go back to previous page
        private void Back_button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserPage), blindtest);
        }

        // Edit user Page
        private void EditUser_button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Disabling UI elements
            editUser_button.IsEnabled = false;
            back_button.IsEnabled = false;
            newUser_textBox.IsEnabled = false;
            newPP_textBox.IsEnabled = false;
            passwordBox.IsEnabled = false;

            editUser_button.Content = "Editing user...";

            // Checking if some info is missing
            if (newUser_textBox.Text == "" || newPP_textBox.Text == "" || passwordBox.Password == "")
            {
                editUser_button.Content = "You must give every info!";
            }

            else
            {
                // Edit user with that dedicated method, then reloading ui elements
                switch (blindtest.editUser(blindtest.getSelectedUserIndex(), newUser_textBox.Text, newPP_textBox.Text, passwordBox.Password))
                {
                    case 0:
                        editUser_button.Content = "User edited successfully!";
                        selected_user = blindtest.getUserByUserID(selected_user.user_id);
                        blindtest.setSelectedUser(selected_user, blindtest.getSelectedUserIndex());
                        username_label.Text = selected_user.nickname;
                        BitmapImage bmpImg = new BitmapImage();
                        bmpImg.UriSource = new Uri(selected_user.profile_picture);
                        profilePicture_Image.Source = bmpImg;
                        profilePicture_textBox.Text = selected_user.profile_picture;
                        break;

                    case 1:
                        editUser_button.Content = "You must give every info!";
                        break;

                    case 2:
                        editUser_button.Content = "Invalid PP URL!";
                        break;

                    case 3:
                        editUser_button.Content = "Invalid password!";
                        break;

                    case 42:
                        editUser_button.Content = blindtest.getError();
                        break;
                }
            }

            // Reenabling UI elements
            editUser_button.IsEnabled = true;
            back_button.IsEnabled = true;
            newUser_textBox.IsEnabled = true;
            newPP_textBox.IsEnabled = true;
            passwordBox.IsEnabled = true;
        }
    }
}
