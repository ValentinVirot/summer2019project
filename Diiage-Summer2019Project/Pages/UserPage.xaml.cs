/*
 * Filename: UserPage.xaml.cs
 * Created by: Valentin Virot, @valentin_vir
 * Description: User management page
*/

// Adding dependencies
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Diiage_Summer2019Project
{
    public sealed partial class UserPage : Page
    {
        // Global Objects
        BlindtestClass blindtest = new BlindtestClass();
        BTUser selected_user = new BTUser();

        // Constructor
        public UserPage()
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

                ObservableCollection<BTUser> listItems = new ObservableCollection<BTUser>();

                foreach (BTUser user in blindtest.getAllUsers())
                {
                    listItems.Add(user);
                }

                users_listView.ItemsSource = listItems;

                selected_user = blindtest.getSelectedUser();

                if (!selected_user.Equals(default(BTUser)))
                {
                    // Check if user still exist
                    if (blindtest.checkIfUserExist(selected_user) == true)
                    {
                        userInformation_label.Text = "User information - Selected User : " + selected_user.nickname;
                        BitmapImage bmpImg = new BitmapImage();
                        bmpImg.UriSource = new Uri(selected_user.profile_picture);
                        profilePicture_Image.Source = bmpImg;
                        users_listView.SelectedIndex = blindtest.getSelectedUserIndex();
                    }
                }
            }
        }

        // Go back to previous page
        private void Back_button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        // When a user is selected in dedicated listView, loading UI elements
        private void Users_listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (users_listView.SelectedIndex != -1)
            {
                selected_user = (BTUser)users_listView.SelectedItem;
                userInformation_label.Text = "User information - Selected User : " + selected_user.nickname;
                BitmapImage bmpImg = new BitmapImage();
                bmpImg.UriSource = new Uri(selected_user.profile_picture);
                profilePicture_Image.Source = bmpImg;
            }

            else
            {
                selected_user = new BTUser();
                BitmapImage bmpImg = new BitmapImage();
                bmpImg.UriSource = new Uri("ms-appx:///Assets/Users/unselected-user.png");
                profilePicture_Image.Source = bmpImg;
            }
        }

        // Edit user button
        private void EditUser_button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (users_listView.SelectedIndex != -1)
            {
                blindtest.setSelectedUser(selected_user, users_listView.SelectedIndex);

                this.Frame.Navigate(typeof(Pages.UserEditPage), blindtest);
            }

            else
            {
                editUser_button.Content = "You must select a user first!";
            }
        }

        // Create user button
        private void CreateUser_button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Pages.CreateUserPage), blindtest);
        }

        // Delete user button
        private void DeleteUser_button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (users_listView.SelectedIndex != -1)
            {
                blindtest.setSelectedUser(selected_user, users_listView.SelectedIndex);

                this.Frame.Navigate(typeof(Pages.DeleteUserPage), blindtest);
            }

            else
            {
                deleteUser_button.Content = "You must select a user first!";
            }
        }
    }
}
