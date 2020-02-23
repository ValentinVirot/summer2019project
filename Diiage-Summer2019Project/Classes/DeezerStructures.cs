/*
 * Filename: /Classes/DeezerStructures.cs
 * Created by: Valentin Virot, @valentin_vir
 * Description: All structures used to store Deezer API's call responses
*/

// Adding dependencies
using System.Collections.Generic;

namespace Diiage_Summer2019Project
{
    // Errors returned by the API
    public struct DeezerRequestError
    {
        public string type { get; set; }
        public string message { get; set; }
        public int code { get; set; }
    }

    // Artist information
    public struct DeezerArtist
    {
        public DeezerRequestError error { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string picture_medium { get; set; }
    }

    // Album information
    public struct DeezerAlbum
    {
        public DeezerRequestError error { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public string cover_medium { get; set; }
        public int nb_tracks { get; set; }
    }

    // Track information
    public struct DeezerTrack
    {
        public DeezerRequestError error { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public string title_short { get; set; }
        public int duration { get; set; }
        public string preview { get; set; }
        public DeezerArtist artist { get; set; }
        public DeezerAlbum album { get; set; }
    }

    // Data returned when searching for a track
    public struct DeezerTrackSearch
    {
        public DeezerRequestError error { get; set; }
        public List<DeezerTrack> data { get; set; }
    }
}