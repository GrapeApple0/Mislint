using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Mislint.Core
{
    public class ImageCache
    {
        public static ImageCache Instance { get; } = new ImageCache();
        private readonly SqliteConnection connection;
        private readonly string cacheDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\mislint\\cache\\";
        private readonly ILogger logger;

        private ImageCache()
        {
            logger = Logger.Instance.loggerFactory.CreateLogger("ImageCache");
            try
            {
                var connectionString = new SqliteConnectionStringBuilder()
                {
                    DataSource = "cache.db"
                }.ToString();
                connection = new SqliteConnection(connectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE IF NOT EXISTS cache (url TEXT PRIMARY KEY, filename TEXT, created_at INTEGER, last_view_at INTEGER)";
                command.ExecuteNonQuery();
                if (!Directory.Exists(cacheDir)) Directory.CreateDirectory(cacheDir);
            }
            catch
            {
                throw;
            }
            logger.LogInformation("ImageCache initialized: {cacheDir}", cacheDir);
        }

        private async Task<byte[]> Download(string url)
        {
            logger.LogInformation("Downloading: {url}", url);
            var filename = Guid.NewGuid().ToString();
            var path = $"{cacheDir}{filename}";
            try
            {
                var bytes = await Shared.HttpClient.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(path, bytes);
                Add(url, filename);
                return bytes;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to download with Error: {exception}/url: {url}", e.Message, url);
                return [];
            }
        }

        public async Task GetImage(string url, Func<byte[], Task> callback)
        {
            if (IsExists(url, out var path))
            {
                await callback(await File.ReadAllBytesAsync(path));
            }
            else
            {
                await callback(await Download(url));
            }
        }

        public bool IsExists(string url, out string path)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT filename FROM cache WHERE url = @url";
            command.Parameters.AddWithValue("@url", url);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                path = cacheDir + reader.GetString(0);
                logger.LogInformation("Cache hit: {path}", path);
                if (!File.Exists(path))
                {
                    logger.LogInformation("Cache file not found: {path}", path);
                    Delete(url);
                    path = null;
                    return false;
                }
                return true;
            }
            path = null;
            return false;
        }

        public void Add(string url, string filename)
        {
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO cache (url, filename, created_at, last_view_at) VALUES (@url, @filename, @created_at, @last_view_at)";
                command.Parameters.AddWithValue("@url", url);
                command.Parameters.AddWithValue("@filename", filename);
                command.Parameters.AddWithValue("@created_at", DateTimeOffset.Now.ToUnixTimeSeconds());
                command.Parameters.AddWithValue("@last_view_at", DateTimeOffset.Now.ToUnixTimeSeconds());
                command.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
        }

        public void Delete(string url)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"DELETE FROM cache WHERE url = @{nameof(url)}";
            command.Parameters.AddWithValue("@url", url);
            command.ExecuteNonQuery();
        }

        public void All()
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM cache";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                Debug.WriteLine("url: {url}, filename: {filename}, created_at: {created_at}, last_view_at: {last_view_at}", reader.GetString(0), reader.GetString(1), reader.GetInt64(2), reader.GetInt64(3));
            }
        }
    }
}
