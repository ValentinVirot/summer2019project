/*
 * Filename: /Pages/CreateUserPage.xaml.cs
 * Created by: Valentin Virot, @valentin_vir
 * Description: User creation Page
*/

// Adding dependencies
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Diiage_Summer2019Project.Pages
{
    public sealed partial class CreateUserPage : Page
    {
        // Global object
        BlindtestClass blindtest;

        // Constructor
        public CreateUserPage()
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
            }
        }

        // Go back to previous page
        private void Back_button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserPage), blindtest);
        }

        // When CreateUser button is clicked
        private void CreateUser_button_Click(object sender, RoutedEventArgs e)
        {
            // Disabling UI elements
            createUser_button.IsEnabled = false;
            back_button.IsEnabled = false;

            // Check if textBox are empty
            if(username_textBox.Text == "" || pp_textBox.Text == "" || passwordBox.Password == "" || passwordBox2.Password == "")
            {
                status_label.Text = "You need to give every information!";
            }

            else
            {
                // Check if passwords are the same
                if(passwordBox.Password == passwordBox2.Password)
                {
                    switch(blindtest.createUser(username_textBox.Text, pp_textBox.Text, passwordBox.Password))
                    {
                        case 0:
                            status_label.Text = "User created successfully!";
                            break;

                        case 1:
                            status_label.Text = "You must give every information.";
                            break;

                        case 2:
                            status_label.Text = "Invalid profile picture URL.";
                            break;

                        case 3:
                            status_label.Text = "Username already gaven.";
                            break;

                        case 42:
                            status_label.Text = blindtest.getError();
                            break;
                    }
                }

                else
                {
                    status_label.Text = "Password aren't the same.";
                }
            }

            // Reenabling UI elements
            createUser_button.IsEnabled = true;
            back_button.IsEnabled = true;
        }
    }
}
