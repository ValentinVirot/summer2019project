/*
 * Filename: /Pages/MainPage.xaml.cs
 * Created by: Valentin Virot, @valentin_vir
 * Description: MainPage of the app
*/

// Adding dependencies
using Diiage_Summer2019Project.Pages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Diiage_Summer2019Project
{
    // Constructor
    public sealed partial class MainPage : Page
    {
        // Global objects
        BlindtestClass blindtest = new BlindtestClass();

        // Constructor
        public MainPage()
        {
            this.InitializeComponent();
        }

        // Music Library Button
        private void Library_button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LibraryPage), blindtest);
        }

        // User management Button
        private void Users_button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserPage), blindtest);
        }

        // Game Management Button
        private void GameSettings_button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(GamePage), blindtest);
        }

        // Play game Button
        private void PlayGame_button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PlayGamePage), blindtest);
        }
    }
}
