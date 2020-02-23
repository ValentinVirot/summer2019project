/*
 * Filename: /Pages/DeleteUserPage.xaml.cs
 * Created by: Valentin Virot, @valentin_vir
 * Description: User creation Page
*/

// Adding dependencies
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Diiage_Summer2019Project.Pages
{
    public sealed partial class DeleteUserPage : Page
    {
        // Global Objects
        BlindtestClass blindtest;
        BTUser selected_user;

        // Constructor
        public DeleteUserPage()
        {
            this.InitializeComponent();
        }

        // Back to previous page
        private void Back_button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserPage), blindtest);
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
                username_label.Text = selected_user.nickname;
                BitmapImage bmpImg = new BitmapImage();
                bmpImg.UriSource = new Uri(selected_user.profile_picture);
                user_Image.Source = bmpImg;
            }
        }

        // Delete User button
        private void DeleteUser_button_Click(object sender, RoutedEventArgs e)
        {
            deleteUser_button.IsEnabled = false;

            // Check if something is missing
            if(passwordBox.Password == "")
            {
                status_label.Text = "You must type your password!";
                deleteUser_button.IsEnabled = true;
            }

            else
            {
                // Calling that dedicated method
                switch(blindtest.deleteUser(blindtest.getSelectedUserIndex(), passwordBox.Password))
                {
                    // Resetting UI elements
                    case 0:
                        status_label.Text = "User deleted successfully!";
                        username_label.Text = "deleted.";
                        BitmapImage bmpImg = new BitmapImage();
                        bmpImg.UriSource = new Uri("ms-appx:///Assets/Users/unselected-user.png");
                        user_Image.Source = bmpImg;
                        break;

                    case 1:
                        status_label.Text = "Invalid Password!";
                        deleteUser_button.IsEnabled = true;
                        break;

                    case 42:
                        status_label.Text = blindtest.getError();
                        deleteUser_button.IsEnabled = true;
                        break;
                }
            }
        }
    }
}

