using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace InstallerStudio.Providers
{
    public static class CacheProvider
    {
        private static readonly ConcurrentDictionary<string, BitmapImage> _iconCache = new();

        public static async Task<BitmapImage> GetCachedIconOrDefaultAsync(string filePath)
        {
            if (_iconCache.TryGetValue(filePath, out var bitmap))
            {
                return bitmap;
            }

            bitmap = new BitmapImage();

            try
            {
                var storageFile = await StorageFile.GetFileFromPathAsync(filePath);
                var icon = await storageFile.GetThumbnailAsync(ThumbnailMode.ListView);

                bitmap.SetSource(icon);

                _iconCache.TryAdd(filePath, bitmap);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while getting the thumbnail: {ex.Message}");
            }

            return bitmap;
        }
    }
}
