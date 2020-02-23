/*
 * Filename: /Classes/BlindtestClass.cs
 * Created by: Valentin Virot, @valentin_vir
 * Description: Blind test class, makes the magic happen.
*/

// Adding dependencies
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace Diiage_Summer2019Project
{
    public class BlindtestClass
    {
        // Global variables
        string error = "";
        string deezer_api_search_parameters = "search?q=";
        const string deezer_api_base_url = "http://api.deezer.com/";
        int download_status = 0, selected_user_index = -1, connected_user_index = -1, selected_playlist_index = -1, selected_game_index = -1;

        // Global objects
        BTDatabase database = new BTDatabase();
        BTUser selected_user = new BTUser(), connected_user = new BTUser();
        BTPlaylist selected_playlist;
        BTGame selected_game;
        DeezerTrackSearch search_results = new DeezerTrackSearch();
        StorageFolder app_folder = ApplicationData.Current.LocalFolder;
        StorageFile database_file, track_file;
        HttpClient client = new HttpClient();
        MediaElement playMusic = new MediaElement();
        DownloadOperation dlOperation;
        BackgroundDownloader bDownloader = new BackgroundDownloader();

        // Constructor
        // Automatically reads database.json file if it exists in Local Appdata folder
        // If it doesn't, database.json file is generated
        public BlindtestClass()
        {
            initDatabase();
            importDatabaseFileContent();
        }

        /* =================================================================================================================== */
        /*                                                         Init part                                                   */
        /* =================================================================================================================== */

        // Creates database.json file if it doesn't exists
        private async void initDatabase()
        {
            // Check if database.json file exists
            try
            {
                // File stored in local ApplicationData folder
                database_file = await app_folder.GetFileAsync("database.json");
            }

            // If it doesn't
            catch
            {
                // Creating empty struct to initialize database object
                database.user = new List<BTUser>();
                database.game = new List<BTGame>();

                // Create JSON file from database object
                database_file = await app_folder.CreateFileAsync("database.json", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(database_file, JsonConvert.SerializeObject(database));
            }
        }

        // Load database.json file content
        private async void importDatabaseFileContent()
        {
            try
            {
                // Getting file
                database_file = await app_folder.GetFileAsync("database.json");

                // Reading all content into this string
                string json_file_content = await FileIO.ReadTextAsync(database_file);

                // Deserializing everything into database object
                database = JsonConvert.DeserializeObject<BTDatabase>(json_file_content);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
        }

        // Saving database object into database.json file
        private async void saveDatabase()
        {
            database_file = await app_folder.GetFileAsync("database.json");
            await FileIO.WriteTextAsync(database_file, JsonConvert.SerializeObject(database));
        }

        // Return error message (stored here)
        public string getError()
        {
            return error;
        }

        /* =================================================================================================================== */
        /*                                                         User part                                                   */
        /* =================================================================================================================== */

        // Connect User
        // Used to connect user with given credentials
        // Returns true if all credentials are correct, false if something goes wrong
        // If false is returned, user can use getError() to know what happend
        public bool connectUser(int userID, string password, int user_index)
        {
            bool status = false;
            bool user_found = false;

            try
            {
                // Getting user object in database
                foreach (BTUser user_in_db in database.user)
                {
                    // If user is in DB (according to userID
                    if (user_in_db.user_id == userID)
                    {
                        user_found = true;

                        // Check if password is correct (SHA-256 hash)
                        if (user_in_db.password == hashPassword(password))
                        {
                            // Stores connected user locally
                            connected_user = user_in_db;
                            connected_user_index = user_index;
                            status = true;
                        }

                        else
                        {
                            error = "Wrong password";
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                error = ex.Message;
            }

            if (user_found == false)
            {
                error = "User not found in database";
            }

            return status;
        }

        // Returns connected user (stored in this class)
        public BTUser getConnectedUser()
        {
            return connected_user;
        }

        // Returns connected user index (stored in this class)
        public int getConnectedUserIndex()
        {
            return connected_user_index;
        }

        // Check if users exists in database (by given BTUser Object)
        // Returns true if user exists, false if it doesn't
        public bool checkIfUserExist(BTUser user_src)
        {
            bool status = false;

            // Check if user exists according to userID
            foreach (BTUser user in database.user)
            {
                if (user.user_id == user_src.user_id)
                {
                    status = true;
                }
            }

            return status;
        }

        // Check if users exists in database (by given BTUser Object)
        // Returns true if user exists, false if it doesn't
        public bool checkIfUserExist(string username)
        {
            bool status = false;

            foreach (BTUser user in database.user)
            {
                if (user.nickname == username)
                {
                    status = true;
                }
            }

            return status;
        }

        // Set selected user (in this class)
        public void setSelectedUser(BTUser user_selected, int user_index)
        {
            selected_user = user_selected;
            selected_user_index = user_index;
        }

        // Returns selected user
        public BTUser getSelectedUser()
        {
            return selected_user;
        }

        // Returns selected user index
        public int getSelectedUserIndex()
        {
            return selected_user_index;
        }

        // Create new user
        // 0 : no error
        // 1 : name, password or URL empty
        // 2 : invalid PP URL
        // 3 : username already gaven
        // 42: Get error string to know what happend
        public int createUser(string src_name, string src_profilepicture_url, string password)
        {
            int status = 0;

            try
            {
                // checks if source strings are empty
                if (src_name == "" || src_profilepicture_url == "" || password == "")
                {
                    status = 1;
                }

                // If they aren't empty
                else
                {
                    // Check if source PP URL is valid
                    if (Uri.IsWellFormedUriString(src_profilepicture_url, UriKind.Absolute))
                    {
                        // Default userID is equal to 0 (first one)
                        int userID = 0;

                        // if users already exists, generating random userID between 0 and 10000
                        if (database.user.Count != 0)
                        {
                            List<int> userID_list = new List<int>();

                            // Getting every existing userID
                            foreach (BTUser user in database.user)
                            {
                                userID_list.Add(user.user_id);
                            }

                            // To generate random int between 0 and 10000
                            Random random_int = new Random();

                            bool loop_state = false;
                            while (loop_state == false)
                            {
                                // Generating userID
                                userID = random_int.Next(0, 10000);

                                // Checking if it exists
                                foreach (int i in userID_list)
                                {
                                    // If userID not attributed, exit the loop
                                    if (userID != i)
                                    {
                                        loop_state = true;
                                    }

                                    else
                                    {
                                        loop_state = false;
                                    }
                                }
                            }
                        }

                        // Creating new user
                        BTUser new_user = new BTUser();
                        new_user.user_id = userID;
                        new_user.nickname = src_name;
                        new_user.profile_picture = src_profilepicture_url;
                        new_user.playlists = new List<BTPlaylist>();

                        // Hashing password  
                        new_user.password = hashPassword(password);

                        // Check if username is already given to someone in db
                        if(checkIfUserExist(new_user.nickname) == true)
                        {
                            status = 3;
                        }

                        else
                        {
                            // Adding user to database
                            database.user.Add(new_user);

                            // Saving changes to database.json file
                            saveDatabase();
                        }
                    }

                    else
                    {
                        status = 2;
                    }
                }
            }

            catch (Exception ex)
            {
                status = 42;
                error = ex.Message;
            }

            return status;
        }

        // Hash a given string using SHA-256 algorithm
        private string hashPassword(string passwd)
        {
            string password = "";

            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(passwd));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                password = builder.ToString();
            }

            return password;
        }

        // Delete user
        // 0 : no error
        // 1 : wrong password
        // 42: Get error string to know what happend
        public int deleteUser(int src_user_index, string password)
        {
            int status = 0;

            try
            {
                // Check if user's password in database (according to user index) is the same as given password
                if(database.user[src_user_index].password != hashPassword(password))
                {
                    status = 1;
                }

                else
                {
                    // Removing user from database
                    database.user.Remove(database.user[src_user_index]);

                    // Saving changes
                    saveDatabase();
                }
            }

            // In case of other error
            catch (Exception ex)
            {
                status = 42;
                error = ex.Message;
            }

            return status;
        }

        // Edit user
        // 0 : no error
        // 1 : name, password or URL empty
        // 2 : invalid PP URL
        // 3 : Invalid password
        // 42: Get error string to know what happend
        public int editUser(int user_index, string src_name, string src_profilepicture_url, string password)
        {
            int status = 0;

            try
            {
                // checks if source strings are empty
                if (src_name == "" || src_profilepicture_url == "" || password == "")
                {
                    status = 1;
                }

                // If they aren't empty
                else
                {
                    // Check if source PP URL is valid
                    if (Uri.IsWellFormedUriString(src_profilepicture_url, UriKind.Absolute))
                    {
                        // Creating new user
                        BTUser edited_user = database.user[user_index];
                        edited_user.nickname = src_name;
                        edited_user.profile_picture = src_profilepicture_url;
                        
                        // Check if password is the same as user's one
                        if(edited_user.password == hashPassword(password))
                        {
                            // Editing user in database
                            database.user[user_index] = edited_user;

                            // Saving changes to database.json file
                            saveDatabase();
                        }

                        else
                        {
                            status = 3;
                        }
                    }

                    else
                    {
                        status = 2;
                    }
                }
            }

            catch (Exception ex)
            {
                status = 42;
                error = ex.Message;
            }

            return status;
        }

        // Get specified user by userIndex
        public BTUser getUser(int userIndex)
        {
            return database.user[userIndex];
        }

        // Get specified user by userID
        public BTUser getUserByUserID(int userID)
        {
            BTUser user_to_return = new BTUser();
            user_to_return.nickname = "INVALID";

            // Check if user exists in database
            foreach(BTUser user in database.user)
            {
                if(user.user_id == userID)
                {
                    user_to_return = user;
                }
            }

            return user_to_return;
        }

        // Returns all users stored in database
        public List<BTUser> getAllUsers()
        {
            return database.user;
        }

        /* =================================================================================================================== */
        /*                                                     Playlist part                                                   */
        /* =================================================================================================================== */

        // Set selected playlist stored here
        public void setSelectedPlaylist(BTPlaylist playlist)
        {
            selected_playlist = playlist;
        }

        // Set selected playlist index stored here
        public void setSelectedPlaylistIndex(int index)
        {
            selected_playlist_index = index;
        }

        // Returns selected playlist (stored here)
        public BTPlaylist getSelectedPlaylist()
        {
            return selected_playlist;
        }

        // Returns selected playlist index (stored here)
        public int getSelectedPlaylistIndex()
        {
            return selected_playlist_index;
        }

        // Returns all playlists present in database (according to given userID)
        public List<BTPlaylist> getAllPlaylists(int userID)
        {
            List<BTPlaylist> playlists_list = new List<BTPlaylist>();

            // Checking if user exists
            foreach(BTUser user in database.user)
            {
                if(user.user_id == userID)
                {
                    playlists_list = user.playlists;
                }
            }

            return playlists_list;
        }

        // Create new playlist
        // 0 : no error
        // 1 : name or URL empty
        // 2 : Name already used
        // 42: Get error string to know what happend
        public int createPlaylist(int user_index, string src_name)
        {
            int status = 0;

            try
            {
                // checks if source string and int are empty
                if (src_name == "")
                {
                    status = 1;
                }

                // If they aren't empty
                else
                {
                    bool is_name_used_by_user = false;

                    foreach(BTPlaylist playlist_in_db in database.user[user_index].playlists)
                    {
                        if(playlist_in_db.name == src_name)
                        {
                            is_name_used_by_user = true;
                        }
                    }

                    if(is_name_used_by_user == false)
                    {
                        // Creating new user
                        BTPlaylist playlist = new BTPlaylist();
                        playlist.user_id = database.user[user_index].user_id;
                        playlist.name = src_name;
                        playlist.tracklist = new List<DeezerTrack>();

                        // Adding playlist to database
                        database.user[user_index].playlists.Add(playlist);

                        // Saving changes to database.json file
                        saveDatabase();
                    }
                    
                    else
                    {
                        status = 2;
                    }
                }
            }

            catch (Exception ex)
            {
                status = 42;
                error = ex.Message;
            }

            return status;
        }

        // Rename playlist
        // 0 : no error
        // 1 : name or user index empty
        // 42: Get error string to know what happend
        public int renamePlaylist(int user_index, int playlist_index, string src_name)
        {
            int status = 0;

            try
            {
                // checks if source string is empty
                if (src_name == "")
                {
                    status = 1;
                }

                // If they aren't empty
                else
                {
                    // get playlist first
                    BTPlaylist playlist = database.user[user_index].playlists[playlist_index];

                    // Change values
                    playlist.name = src_name;

                    // Editing playlist in database
                    database.user[user_index].playlists[playlist_index] = playlist;

                    // Saving changes to database.json file
                    saveDatabase();
                }
            }

            catch (Exception ex)
            {
                status = 42;
                error = ex.Message;
            }

            return status;
        }

        // Delete playlist
        // 0 : no error
        // 1 : user doesn't exist in database
        // 2 : invalid password
        // 42: Get error string to know what happend
        public int deletePlaylist(BTUser user_src, string password, int playlist_index)
        {
            int status = 0;
            bool user_exists = false;
            BTUser user_selected = new BTUser();

            try
            {
                // First, check if user exists in db (using userID)
                foreach (BTUser user in database.user)
                {
                    if (user.user_id == user_src.user_id)
                    {
                        user_exists = true;
                        user_selected = user;
                    }
                }

                // If user doesn't exists
                if (user_exists == false)
                {
                    status = 1;
                }

                else
                {
                    // Check if password is incorrect
                    if (hashPassword(password) != user_selected.password)
                    {
                        status = 2;
                    }

                    else
                    {
                        foreach (BTUser user in database.user)
                        {
                            if (user.user_id == user_selected.user_id)
                            {
                                // Removing playlist from database
                                user.playlists.Remove(user.playlists[playlist_index]);

                                // Saving changes
                                saveDatabase();
                            }
                        }
                    }
                }
            }

            // In case of other error
            catch (Exception ex)
            {
                status = 42;
                error = ex.Message;
            }

            return status;
        }

        // Search songs on Deezer
        // Make API Call using keywords given by user
        // Return DeezerTrackSearch objects
        // In error.code :
        // 0 : no error
        // 1 : empty keywords
        // 2 : bad HttpResponseMessage
        // 42: exception message included
        // Other code: see DeezerAPI Error info on their website
        public DeezerTrackSearch searchTrackOnDeezer(string keywords)
        {
            DeezerRequestError errors = new DeezerRequestError();
            errors.code = 0;

            // Checking if keywords are given by user
            if (keywords == "")
            {
                errors.code = 1;
                errors.message = "Empty keywords";
            }

            else
            {
                try
                {
                    // Making HTTP GET Call
                    HttpResponseMessage api_response = client.GetAsync(deezer_api_base_url + deezer_api_search_parameters + keywords).Result;

                    // If it's a success
                    if (api_response.IsSuccessStatusCode)
                    {
                        // Stores JSON response in string format
                        var response = api_response.Content.ReadAsStringAsync();
                        string json = response.Result.ToString();

                        // Deserializing into structures
                        search_results = JsonConvert.DeserializeObject<DeezerTrackSearch>(json);
                    }

                    // If answer is incorrect
                    else
                    {
                        errors.code = 2;
                        errors.message = "Bad HttpResponseMessage status code";
                    }
                }

                // If anything else happend
                catch (Exception ex)
                {
                    errors.code = 42;
                    errors.message = ex.Message;
                }
            }

            if (errors.code != 0)
            {
                search_results.error = errors;
            }

            return search_results;
        }

        // Returns track by index
        public DeezerTrack getTrack(int index)
        {
            try
            {
                return search_results.data[index];
            }

            catch
            {
                DeezerTrack bad_result = new DeezerTrack();
                return bad_result;
            }
        }

        // Returns playlist by index
        public BTPlaylist getPlaylist(int user_index, int playlist_index)
        {
            try
            {
                return database.user[user_index].playlists[playlist_index];
            }

            catch
            {
                BTPlaylist bad_result = new BTPlaylist();
                return bad_result;
            }
        }

        // Adding track to a playlist
        // 0 : no errors
        // 1 : default DeezerTrack object
        // 2 : track already in playlist
        // 42: get string error to have message
        public int addTrackToPlaylist(DeezerTrack src_track, int user_index, int playlist_index)
        {
            int status = 0;

            try
            {
                // Check if given track object is not just initialized
                if(src_track.Equals(default(DeezerTrack)))
                {
                    status = 1;
                }

                else
                {
                    // Check if track isn't already in the playlist
                    bool is_track_already_here = false;

                    foreach (DeezerTrack track in database.user[user_index].playlists[playlist_index].tracklist)
                    {
                        if(src_track.id == track.id)
                        {
                            is_track_already_here = true;
                        }
                    }

                    // If track doesn't exists in playlist
                    if(is_track_already_here == false)
                    {
                        // Adding it
                        database.user[user_index].playlists[playlist_index].tracklist.Add(src_track);
                        
                        // Saving changes in database
                        saveDatabase();
                    }

                    else
                    {
                        status = 2;
                    }
                }
            }
            
            // If something else happend
            catch(Exception ex)
            {
                error = ex.Message;
                status = 42;
            }

            return status;
        }

        // Delete track from Playlist
        // 0 : no error
        // 42: Get error string to know what happend
        public int deleteTrackFromPlaylist(int src_user_index, int src_playlist_index, int src_track_index)
        {
            int status = 0;

            try
            {
                // Removing user from database
                database.user[src_user_index].playlists[src_playlist_index].tracklist.Remove(database.user[src_user_index].playlists[src_playlist_index].tracklist[src_track_index]);

                // Saving changes
                saveDatabase();
            }

            // In case of other error
            catch (Exception ex)
            {
                status = 42;
                error = ex.Message;
            }

            return status;
        }
            
        // Reads track preview if it exists
        public async void readPreview(DeezerTrack track)
        {
            try
            {
                // Check if given track is not just initialized
                if(track.Equals(default(DeezerTrack)))
                {
                    error = "Given track is only initialized (default value)";
                }

                else
                {
                    string track_preview_file = track.id.ToString() + ".mp3";

                    try
                    {
                        track_file = await ApplicationData.Current.LocalFolder.GetFileAsync(track_preview_file);
                        download_status = 4;
                        playMusic.Stop();
                        playMusic.SetSource(await track_file.OpenAsync(FileAccessMode.Read), track_file.ContentType);
                        playMusic.Play();
                    }

                    catch
                    {
                        // Checking if the folder is valid
                        if (ApplicationData.Current.LocalFolder != null)
                        {
                            // file object
                            track_file = await ApplicationData.Current.LocalFolder.CreateFileAsync(track_preview_file, CreationCollisionOption.ReplaceExisting);

                            // starts the download
                            dlOperation = bDownloader.CreateDownload(new Uri(track.preview), track_file);

                            // used to display download progression
                            Progress<DownloadOperation> progression = new Progress<DownloadOperation>(progressChanged);

                            CancellationTokenSource cancellationToken = new CancellationTokenSource();

                            try
                            {
                                download_status = 2;
                                await dlOperation.StartAsync().AsTask(cancellationToken.Token, progression);
                            }

                            catch (Exception ex)
                            {
                                download_status = 3;
                                await dlOperation.ResultFile.DeleteAsync();
                                error = ex.Message;
                                dlOperation = null;
                            }
                        }

                        else
                        {
                            download_status = 1;
                        }
                    }
                }
            }
            
            catch(Exception ex)
            {
                error = ex.Message;
            }
        }

        // When Download progress changes
        private void progressChanged(DownloadOperation dlOperation)
        {
            int progress = (int)(100 * ((double)dlOperation.Progress.BytesReceived / (double)dlOperation.Progress.TotalBytesToReceive));
            switch (dlOperation.Progress.Status)
            {
                case BackgroundTransferStatus.Error:
                    {
                        error = "An error occured while downloading.";
                        download_status = 3;
                        break;
                    }
            }

            // if download is finished
            if (progress >= 100)
            {
                dlOperation = null;
                download_status = 4;
            }
        }

        // get download status
        // 0: just initialized
        // 1: can't access directory
        // 2: download started
        // 3: getError();
        // 4: download finished
        public int getDownloadStatus()
        {
            return download_status;
        }

        // Pause track preview listening
        public void pausePreview()
        {
            playMusic.Pause();
        }

        // Resume track preview listening
        public void resumePreview()
        {
            playMusic.Play();
        }

        /* =================================================================================================================== */
        /*                                                         Game part                                                   */
        /* =================================================================================================================== */

        // Rename game
        // 0 : no error
        // 1 : name or user index empty
        // 2 : Game ID not in DB
        // 3 : Game is not created by given user_id
        // 42: Get error string to know what happend
        public int renameGame(int game_id, int user_id, string src_name)
        {
            int status = 0;
            BTGame game_edited = new BTGame();

            try
            {
                // checks if source string is empty
                if (src_name == "")
                {
                    status = 1;
                }

                // If they aren't empty
                else
                {
                    // Check if game is in db and if it's by good user
                    bool is_game_in_db = false;
                    bool is_game_by_user = false;

                    foreach(BTGame game in database.game)
                    {
                        if(game.game_id == game_id)
                        {
                            is_game_in_db = true;
                            if(game.user_id == user_id)
                            {
                                is_game_by_user = true;
                                game_edited = game;
                            }
                        }
                    }

                    // if user in is database
                    if(is_game_in_db == true)
                    {
                        // If game is created by this user
                        if(is_game_by_user == true)
                        {
                            // Rename it
                            game_edited.game_title = src_name;

                            // Saving game
                            for(int i = 0; i < database.game.Count; i++)
                            {
                                if(database.game[i].game_id == game_edited.game_id)
                                {
                                    database.game[i] = game_edited;
                                }
                            }

                            // Saving changes to database.json file
                            saveDatabase();
                        }
                        
                        else
                        {
                            status = 3;
                        }
                    }

                   else
                    {
                        status = 2;
                    } 
                }
            }

            // If something else happend
            catch (Exception ex)
            {
                status = 42;
                error = ex.Message;
            }

            return status;
        }

        // Set selected game (stored here)
        public void setSelectedGame(BTGame game)
        {
            selected_game = game;
        }

        // Set selected game index (stored here)
        public void setSelectedGameIndex(int index)
        {
            selected_game_index = index;
        }

        // Returns selected game (stored here)
        public BTGame getSelectedGame()
        {
            return selected_game;
        }

        // Returns selected game index (stored here)
        public int getSelectedGameIndex()
        {
            return selected_game_index;
        }

        // Return game by game index
        public BTGame getGame(int game_index)
        {
            return database.game[game_index];
        }

        // Return list of games created by given userID
        // If an error happens, it returns an empty list
        public List<BTGame> getGames(int userID)
        {
            try
            {
                List<BTGame> games_by_user = new List<BTGame>();
                
                // For each game stored in db, if it was created by given userID, add it to this list
                foreach(BTGame game in database.game)
                {
                    if(game.user_id == userID)
                    {
                        games_by_user.Add(game);
                    }
                }

                return games_by_user;
            }
            catch
            {
                List<BTGame> bad_result = new List<BTGame>();
                return bad_result;
            }
        }

        // Delete game
        // 0 : no error
        // 1 : user doesn't exist in database
        // 2 : game doesn't exist in database
        // 3 : game isn't created by given user!
        // 4 : invalid password
        // 42: Get error string to know what happend
        public int deleteGame(BTUser user_src, BTGame game_src, string password)
        {
            int status = 0;
            bool user_exists = false, game_exists = false;
            BTUser user_selected = new BTUser();
            BTGame game_selected = new BTGame();

            try
            {
                // First, check if user exists in db (using userID)
                foreach (BTUser user in database.user)
                {
                    if (user.user_id == user_src.user_id)
                    {
                        user_exists = true;
                        user_selected = user;
                    }
                }

                // If user doesn't exists
                if (user_exists == false)
                {
                    status = 1;
                }

                else
                {
                    // Check if game exists in database
                    foreach(BTGame game in database.game)
                    {
                        if(game.game_id == game_src.game_id)
                        {
                            game_selected = game;
                            game_exists = true;
                        }
                    }

                    // if game exists
                    if(game_exists == true)
                    {
                        // Check if game is made by given user
                        if(user_src.user_id == game_selected.user_id)
                        {
                            // Check if password is incorrect
                            if (hashPassword(password) != user_selected.password)
                            {
                                status = 4;
                            }

                            else
                            {
                                // Delete game in database
                                database.game.Remove(game_selected);

                                // Save changes
                                saveDatabase();
                            }
                        }

                        else
                        {
                            status = 3;
                        }
                    }

                    else
                    {
                        status = 2;
                    }
                }
            }

            // In case of other error
            catch (Exception ex)
            {
                status = 42;
                error = ex.Message;
            }

            return status;
        }

        // Create new game
        // 0 : no error
        // 1 : userID is not in database
        // 2 : game_title or created is Empty
        // 3 : Playlist is just bearly initialized
        // 4 : invalid scores
        // 5 : name already given
        // 42: Get error string to know what happend
        public int createGame(int userID, string game_title, string created, BTPlaylist playlist, int title_score, int artist_score, int album_score, int duration_score)
        {
            int status = 0;
            bool temp_status = false;

            try
            {
                // Check if userID is correct
                foreach(BTUser user in database.user)
                {
                    if(user.user_id == userID)
                    {
                        temp_status = true;
                    }
                }

                if(temp_status == false)
                {
                    status = 1;
                }

                else
                {
                    // Check if game_title and created are empty
                    if(game_title == "" || created == "")
                    {
                        status = 2;
                    }

                    else
                    {
                        // Check if given Playlist Object isn't just initialized
                        if(playlist.Equals(default(BTPlaylist)))
                        {
                            status = 3;
                        }

                        else
                        {
                            // Check if scores are initialized
                            if(title_score >= 0 && artist_score >= 0 && album_score >= 0 && duration_score >= 0)
                            {
                                // Let's create that game!
                                // Default gameID is 0
                                int gameID = 0;

                                // If games already exists
                                if(database.game.Count > 0)
                                {
                                    // Let's generate that game
                                    // To generate random int between 0 and 10000
                                    Random random_int = new Random();
                                    temp_status = true;

                                    while (temp_status == true)
                                    {
                                        // Generating gameID
                                        gameID = random_int.Next(0, 10000);

                                        // Checking if it exists
                                        foreach (BTGame game in database.game)
                                        {
                                            // If gameID not attributed, exit the loop
                                            if (game.game_id != gameID)
                                            {
                                                temp_status = false;
                                            }

                                            else
                                            {
                                                temp_status = true;
                                            }
                                        }
                                    }
                                }

                                // Now that gameID is generated, let's create that game!
                                BTGame game_to_create = new BTGame();
                                game_to_create.game_id = gameID;
                                game_to_create.user_id = userID;
                                game_to_create.game_title = game_title;
                                DateTime created_date = DateTime.Parse(created);
                                game_to_create.created = created_date.ToString();
                                game_to_create.tracklist = playlist.tracklist;
                                BTGameScores scores_to_assign = new BTGameScores();
                                scores_to_assign.title = title_score;
                                scores_to_assign.artist = artist_score;
                                scores_to_assign.album = album_score;
                                scores_to_assign.duration = duration_score;
                                scores_to_assign.history = new List<BTGameHistory>();
                                game_to_create.scores = scores_to_assign;

                                // Check if game title is not already given
                                bool is_game_title_already_given = false;
                                foreach(BTGame game in database.game)
                                {
                                    if(game.user_id == userID)
                                    {
                                        if(game.game_title == game_title)
                                        {
                                            is_game_title_already_given = true;
                                        }
                                    }
                                }

                                if(is_game_title_already_given == true)
                                {
                                    status = 5;
                                }
                                
                                else
                                {
                                    // Add game to database
                                    database.game.Add(game_to_create);

                                    // Save changes
                                    saveDatabase();
                                }
                            }

                            else
                            {
                                status = 4;
                            }
                        }
                    }
                }
            }

            // If something else happend
            catch (Exception ex)
            {
                status = 42;
                error = ex.Message;
            }

            return status;
        }

        // Add score to a game history
        // 0 : no error
        // 1 : game or score is just initialized
        // 2 : game not in db
        // 42 : check error
        public int addScore(BTGame game, BTGameHistory score)
        {
            int status = 0;

            // Check if both Game and GameHistory objects aren't just initialized
            if(game.Equals(default(BTGame)) || score.Equals(default(BTGameHistory)))
            {
                status = 1;
            }

            else
            {
                bool temp_status = false;
                // Check if game exists in database
                foreach(BTGame game_in_db in database.game)
                {
                    if(game_in_db.game_id == game.game_id)
                    {
                        temp_status = true;
                    }
                }

                // If it exists
                if(temp_status == true)
                {
                    try
                    {
                        // Adding score to database
                        for (int i = 0; i < database.game.Count; i++)
                        {
                            if (database.game[i].game_id == game.game_id)
                            {
                                database.game[i].scores.history.Add(score);
                            }
                        }

                        // Saving changes
                        saveDatabase();
                    }
                    
                    // If something else happend
                    catch(Exception ex)
                    {
                        error = ex.Message;
                        status = 42;
                    }
                }

                else
                {
                    status = 2;
                }
            }

            return status;
        }

        // Return all games in a List of BTGame elements
        public List<BTGame> getAllGames()
        {
            return database.game;
        }
    }
}