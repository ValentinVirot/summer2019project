/*
 * Filename: /Pages/PlaySelectedGamePage.xaml.cs
 * Created by: Valentin Virot, @valentin_vir
 * Description: Real game page, were user plays
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
    public sealed partial class PlaySelectedGamePage : Page
    {
        // Global objects
        BlindtestClass blindtest;
        BTUser connected_user;
        BTGame selected_game;
        BTAnswer current_answer;
        DeezerTrack current_track = new DeezerTrack();
        List<DeezerTrack> tracklist_src = new List<DeezerTrack>();
        DispatcherTimer downloadStatusTimer;
        List<BTScoreboard> scoreboard = new List<BTScoreboard>();
        List<BTAnswer> all_answers = new List<BTAnswer>();
        ObservableCollection<BTAnswer> answer_listItems = new ObservableCollection<BTAnswer>();

        // Global variables
        bool is_title_correct = false, is_artist_correct = false, is_album_correct = false, is_duration_correct = false;
        int score = 0, nbr_track = 0, total_tracks = 0, current_track_int = 0, total_correct_answer = 0;
        string answer = "";
        
        // Constructor
        public PlaySelectedGamePage()
        {
            this.InitializeComponent();
            downloadStatusTimerSetup();
        }

        // Timer used to lock play preview button when a song is downloading
        public void downloadStatusTimerSetup()
        {
            downloadStatusTimer = new DispatcherTimer();
            downloadStatusTimer.Tick += downloadStatusTimer_Tick;
            downloadStatusTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            downloadStatusTimer.Start();
        }

        // Called every tick of that timer
        private void downloadStatusTimer_Tick(object sender, object e)
        {
            if (blindtest.getDownloadStatus() == 4)
            {
                playPreview_button.Content = "Play preview!";
                playPreview_button.IsEnabled = true;
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

                // Loading UI elements and filling scoreboard listView
                connected_user = blindtest.getConnectedUser();
                selected_game = blindtest.getSelectedGame();

                correctAnswers_label.Text = "Correct Answers - " + selected_game.game_title;
                gameStatus_label.Text = "Game in progress...";

                foreach (BTGameHistory history in selected_game.scores.history)
                {
                    BTScoreboard score = new BTScoreboard();
                    score.username = "Username not found";
                    score.profile_picture = "ms-appx:///Assets/Users/unselected-user.png";

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

                // Launching the game
                is_album_correct = false;
                is_artist_correct = false;
                is_duration_correct = false;
                is_title_correct = false;
                score = 0;

                // Loading tracklist
                foreach(DeezerTrack track in selected_game.tracklist)
                {
                    tracklist_src.Add(track);
                }

                nbr_track = tracklist_src.Count;
                total_tracks = tracklist_src.Count;
                current_track_int = 1;
                songStatus_label.Text = "Track " + current_track_int.ToString() + " of " + total_tracks.ToString();

                Random random = new Random();

                int random_int = random.Next(0, nbr_track - 1);
                current_track = tracklist_src[random_int];
                tracklist_src.Remove(current_track);

                gameStatus_label.Text = "Game in progress... - score : " + score.ToString() + " points";
                blindtest.readPreview(current_track);
                total_correct_answer = 0;
            }
        }

        // Answer button
        private void Answer_button_Click(object sender, RoutedEventArgs e)
        {
            current_answer = new BTAnswer();

            // Getting answer by user
            answer = answer_textBox.Text.ToLower();

            // Checking if answer correspond to a track info
            if (answer == current_track.title.ToLower() || answer == current_track.title_short.ToLower())
            {
                if (is_title_correct == false)
                {
                    current_answer.image = "ms-appx:///Assets/correct-answer.png";
                    current_answer.is_correct = "correct answer (title, track " + current_track_int.ToString() + ")!";
                    all_answers.Add(current_answer);

                    title_label.Text = current_track.title;
                    score = score + selected_game.scores.title;
                    total_correct_answer++;
                    is_title_correct = true;
                }

                else
                {
                    answer_button.Content = "You already gave the correct title!";
                }
            }

            if (answer == current_track.artist.name.ToLower())
            {
                if (is_artist_correct == false)
                {
                    current_answer.image = "ms-appx:///Assets/correct-answer.png";
                    current_answer.is_correct = "correct answer (artist, track " + current_track_int.ToString() + ")!";
                    all_answers.Add(current_answer);

                    artist_label.Text = current_track.artist.name;
                    score = score + selected_game.scores.artist;
                    total_correct_answer++;
                    is_artist_correct = true;
                }

                else
                {
                    answer_button.Content = "You already gave the correct artist!";
                }
            }

            if (answer == current_track.album.title.ToLower())
            {
                if (is_album_correct == false)
                {
                    current_answer.image = "ms-appx:///Assets/correct-answer.png";
                    current_answer.is_correct = "correct answer (album, track " + current_track_int.ToString() + ")!";
                    all_answers.Add(current_answer);
                    album_label.Text = current_track.album.title;
                    score = score + selected_game.scores.album;
                    total_correct_answer++;
                }

                else
                {
                    answer_button.Content = "You already gave the correct album!";
                }
            }

            try
            {
                if (Int32.Parse(answer) == current_track.duration)
                {
                    if (is_duration_correct == false)
                    {
                        current_answer.image = "ms-appx:///Assets/correct-answer.png";
                        current_answer.is_correct = "correct answer (duration, track " + current_track_int.ToString() + ")!";
                        all_answers.Add(current_answer);

                        duration_label.Text = current_track.duration.ToString();
                        score = score + selected_game.scores.duration;
                        total_correct_answer++;
                        is_duration_correct = true;
                    }

                    else
                    {
                        answer_button.Content = "You already gave correct duration!";
                    }
                }
            }

            catch
            {

            }

            // If all answers of that track have been found, showing track's album cover
            if (total_correct_answer == 4)
            {
                BitmapImage bmpImg = new BitmapImage();
                bmpImg.UriSource = new Uri(current_track.album.cover_medium);
                selectedTrack_Image.Source = bmpImg;
            }

            // Loading correct tracks in that listView
            gameStatus_label.Text = "Game in progress... - score : " + score.ToString() + " points";
            answer_listItems = new ObservableCollection<BTAnswer>();
            foreach (BTAnswer answers_src in all_answers)
            {
                answer_listItems.Add(answers_src);
            }

            answerList_listView.ItemsSource = answer_listItems;
        }

        // Next song button
        private void NextSong_button_Click(object sender, RoutedEventArgs e)
        {
            // stop previous track preview
            blindtest.pausePreview();

            // Resetting ui elements and local info
            total_correct_answer = 0;
            title_label.Text = "not found yet!";
            artist_label.Text = "not found yet!";
            album_label.Text = "not found yet!";
            duration_label.Text = "not found yet!";
            BitmapImage bmpImg = new BitmapImage();
            bmpImg.UriSource = new Uri("ms-appx:///Assets/default-track.png");
            selectedTrack_Image.Source = bmpImg;
            current_track_int++;
            is_album_correct = false;
            is_artist_correct = false;
            is_duration_correct = false;
            is_title_correct = false;

            // If there is still tracks to play
            if (current_track_int <= total_tracks)
            {
                // Editing status label
                songStatus_label.Text = "Track " + current_track_int.ToString() + " of " + total_tracks.ToString();
                nbr_track--;

                // Selecting random track in the list
                Random random = new Random();
                int random_int = random.Next(0, nbr_track - 1);
                current_track = tracklist_src[random_int];
                tracklist_src.Remove(current_track);

                // Reading preview
                gameStatus_label.Text = "Game in progress... - score : " + score.ToString() + " points";
                blindtest.readPreview(current_track);
                total_correct_answer = 0;
            }

            // When all tracks have been played
            else
            {
                // Showing final score to user
                gameStatus_label.Text = "Game finished! - Final score : " + score.ToString() + " points";

                // Creating history entry
                BTGameHistory history_entry = new BTGameHistory();
                history_entry.user_id = connected_user.user_id;
                history_entry.score = score;
                DateTime time = DateTime.Now;
                history_entry.date = time.ToString();

                // Adding score entry to database
                blindtest.addScore(selected_game, history_entry);
                
                // Locking UI elements
                answer_button.IsEnabled = false;
                answer_textBox.IsEnabled = false;
                nextSong_button.IsEnabled = false;
                downloadStatusTimer.Stop();
                playPreview_button.IsEnabled = false;
                stopPreview_button.IsEnabled = false;

                // Clear scoreboard listview
                ObservableCollection<BTScoreboard> listItems2 = new ObservableCollection<BTScoreboard>();
                scoreboard_listView.ItemsSource = listItems2;

                // Reloading scoreboard in listView
                BTScoreboard scoreboard_entry = new BTScoreboard();
                scoreboard_entry.date = history_entry.date;
                scoreboard_entry.profile_picture = connected_user.profile_picture;
                scoreboard_entry.score = history_entry.score.ToString();
                scoreboard_entry.username = connected_user.nickname;

                scoreboard.Add(scoreboard_entry);

                foreach (BTScoreboard score in scoreboard)
                {
                    listItems2.Add(score);
                }

                scoreboard_listView.ItemsSource = listItems2;
            }
        }

        // Go back to previous page
        private void Back_button_Click(object sender, RoutedEventArgs e)
        {
            blindtest.pausePreview();
            this.Frame.Navigate(typeof(PlayGameSelectionPage), blindtest);
        }

        // Play preview button
        private void PlayPreview_button_Click(object sender, RoutedEventArgs e)
        {
            playPreview_button.IsEnabled = false;
            blindtest.readPreview(current_track);
            playPreview_button.Content = "Downloading preview...";
        }

        // Stop preview button
        private void StopPreview_button_Click(object sender, RoutedEventArgs e)
        {
            blindtest.pausePreview();
        }
    }
}
