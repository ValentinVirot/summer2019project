/*
 * Filename: /Classes/BlindtestDB.cs
 * Created by: Valentin Virot, @valentin_vir
 * Description: structures used as a database, filled at startup by json file saved.
*/

// Adding dependencies
using System.Collections.Generic;

namespace Diiage_Summer2019Project
{
    // Playlist Object, composed of a list of tracks
    public struct BTPlaylist
    {
        public int user_id { get; set; }
        public string name { get; set; }
        public List<DeezerTrack> tracklist { get; set; }  
    }

    // User profile, with his playlists
    public struct BTUser
    {
        public int user_id { get; set; }
        public string nickname { get; set; }
        public string profile_picture { get; set; }
        public string password { get; set; }
        public List<BTPlaylist> playlists { get; set; }
    }

    // Score record of a given player in a game
    public struct BTGameHistory
    {
        public int user_id { get; set; }
        public int score { get; set; }
        public string date { get; set; }
    }

    // Definition of score points and list of BTGameHistory elements to store game history
    public struct BTGameScores
    {
        public int title { get; set; }
        public int artist { get; set; }
        public int album { get; set; }
        public int duration { get; set; }
        public List<BTGameHistory> history { get; set; }
    }

    // Game element
    public struct BTGame
    {
        public int game_id { get; set; }
        public int user_id { get; set; }
        public string game_title { get; set; }
        public string created { get; set; }
        public List<DeezerTrack> tracklist { get; set; }
        public BTGameScores scores { get; set; }
    }

    // Global database structure
    public struct BTDatabase
    {
        public List<BTUser> user { get; set; }
        public List<BTGame> game { get; set; }
    }
    // Struct used localy in page to show game's scoreboard
    public struct BTScoreboard
    {
        public int user_id { get; set; }
        public string username { get; set; }
        public string profile_picture { get; set; }
        public string score { get; set; }
        public string date { get; set; }
    }

    // Struct used locally in PlaySelectedGamePage to display correct answers in dedicated ListView
    public struct BTAnswer
    {
        public string image { get; set; }
        public string is_correct { get; set; }
    }
}