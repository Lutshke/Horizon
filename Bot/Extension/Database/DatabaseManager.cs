using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Horizon.Extensions.Database
{
    public class DatabaseManager
    {
        private PlaylistDatabase Database { get; set; }
        private UserDatabase User { get; set; }

        public DatabaseManager()
        {
            var playlists = File.ReadAllText("Saves/PlaylistSaves.json");
            Database = JsonSerializer.Deserialize<PlaylistDatabase>(playlists);
            var user = File.ReadAllText("Saves/UserSaves.json");
            User = JsonSerializer.Deserialize<UserDatabase>(user);
        }

        /// <summary>
        /// Gets an playlist by id
        /// </summary>
        /// <param name="id">The identifer of the playlist</param>
        /// <returns>True if playlist was found, false otherwise</returns>
        public bool TryGetPlaylist(string id, out DBPlaylist playlist)
        {
            if (id is null || !Database.Playlists.ContainsKey(id))
            {
                playlist = null;
                return false;
            }

            playlist = Database.Playlists[id];
            return true;
        }

        /// <summary>
        /// Gets an playlist by id
        /// </summary>
        /// <param name="id">The identifer of the playlist</param>
        /// <returns>The playlist with given id, null if id wasnt found</returns>
        public DBPlaylist GetPlaylist(string id)
        {
            if (id is null || !Database.Playlists.ContainsKey(id)) return null;
            return Database.Playlists[id];
        }

        /// <summary>
        /// Creates a new Playlist
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns>The identifer of the created Playlist</returns>
        public string AddPlaylist(DBPlaylist playlist)
        {
            var userId = playlist.Author.ToString();
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            Database.Playlists.Add(guid, playlist);
            Database.Count = Database.Playlists.Count;

            if (User.Users.ContainsKey(userId))
                User.Users[userId].Add(guid);
            else
                User.Users.Add(userId, new() { guid });

            return guid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">The identifier of the Playlist</param>
        /// <returns>True if editing was successful</returns>
        public bool RemovePlaylist(string id)
        {
            return Database.Playlists.Remove(id);
        }

        /// <summary>
        /// Saves Playlists to file.
        /// </summary>
        /// <returns>True if saving was successful</returns>
        public async Task<bool> SavePlaylist()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true, };
                var Databasejson = JsonSerializer.Serialize(Database, options);
                var userJson = JsonSerializer.Serialize(User, options);
                await File.WriteAllTextAsync("Saves/PlaylistSaves.json", Databasejson);
                await File.WriteAllTextAsync("Saves/UserSaves.json", userJson);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns all playlists of a certain user
        /// </summary>
        /// <param name="id">the users identifier</param>
        /// <returns>An string array which contains all playlist ids</returns>
        public string[] GetUserPlaylists(string id)
        {
            return User.Users[id].ToArray();
        }
    }
}