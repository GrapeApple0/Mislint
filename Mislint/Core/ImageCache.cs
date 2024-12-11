using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Mislint.Core
{
    public class ImageCache
    {
        public static ImageCache Instance { get; } = new ImageCache();
        private readonly string cacheDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\mislint\\cache\\";
        private readonly ILogger logger;
        private readonly MemoryCache _memoryCache;
        private ImageCache()
        {
            logger = Logger.Instance.loggerFactory.CreateLogger("ImageCache");
            this._memoryCache = new MemoryCache(new MemoryCacheOptions());
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
                this.All();
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
            path = null;
            if (this._memoryCache.TryGetValue(url, out var res))
            {
                path = res.ToString();
                logger.LogInformation("Cache hit: {path}", path);
                if (!File.Exists(path))
                {
                    logger.LogInformation("Cache file not found: {path}", path);
                    return false;
                }
                return true;
            }
            return false;
        }

        public void Add(string url, string filename)
        {
            try
            {
                this._memoryCache.Set(url, filename, new DateTimeOffset(DateTime.Now + TimeSpan.FromHours(1)));
            }
            catch
            {
                throw;
            }
        }

        public void Delete(string url)
        {
            this._memoryCache.Remove(url);
        }

        public void Clear()
        {
            
        }

        public void All()
        {
            //foreach (var key in this._memoryCache.GetKeys())
            //    Debug.WriteLine($"Key: '{key}', Key type: '{key.GetType()}'");
        }
    }
}
