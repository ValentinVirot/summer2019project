/*
 * Filename: /Pages/LibraryPage.xaml.cs
 * Created by: Valentin Virot, @valentin_vir
 * Description: User connection page before managing games
*/

// Adding dependencies
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Diiage_Summer2019Project
{
    public sealed partial class LibraryPage : Page
    {
        // Global Objects
        BlindtestClass blindtest;
        BTUser selected_user = new BTUser();

        // Constructor
        public LibraryPage()
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

                // Filling listView with users according to dataTemplate
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
        private void Back_button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        // Connect button  
        private void Connect_button_Click(object sender, RoutedEventArgs e)
        {
            // If no item was selected in ListView
            if (users_listView.SelectedIndex == -1)
            {
                status_label.Text = "You must select a user first!";
            }

            else
            {
                // Check if password is given
                if (passwordBox.Password == "")
                {
                    status_label.Text = "You must type in your password!";
                }

                else
                {
                    // Check if password is valid, then connect
                    if (blindtest.connectUser(selected_user.user_id, passwordBox.Password, users_listView.SelectedIndex) == true)
                    {
                        status_label.Text = "connected!";
                        blindtest.setSelectedUser(selected_user, users_listView.SelectedIndex);
                        this.Frame.Navigate(typeof(Pages.PlaylistPage), blindtest);
                    }

                    else
                    {
                        status_label.Text = "Error: " + blindtest.getError();
                    }
                }
            }
        }

        // When an item is selected in listView, display info in UI
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
    }
}
