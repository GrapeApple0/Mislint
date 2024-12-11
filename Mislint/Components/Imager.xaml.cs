using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Mislint.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mislint.Components
{
    public partial class Imager : UserControl
    {
        private List<WebP.WebPFrame> _frames = [];
        private readonly List<ImageSource> _bitmaps = [];
        private int _currentFrame = 0;
        private readonly DispatcherTimer _timer = new();
        private int _webPImageWidth;
        private int _webPImageHeight;
        private readonly ILogger _logger;
        public delegate void ImageLoaded<in T>(T args);
        public event ImageLoaded<EventArgs> ImageLoadedEvent;
        public double Aspect { get; private set; } = 1;
        public Image ImageComponent => this.image;
        private string _url;
        public string Url { 
            get => _url;
            set
            {
                _url = value;
                if (this.IsLoaded) this.Imager_Loaded(this, null);
            }
        }
        public Imager()
        {
            this.InitializeComponent();
            this._logger = Logger.Instance.loggerFactory.CreateLogger("Imager");
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, object e)
        {
            if (this._currentFrame == this._frames.Count - 1) _currentFrame = 0;
            else _currentFrame++;
            this.image.Source = this._bitmaps[_currentFrame];
            this._timer.Interval = new TimeSpan(0, 0, 0, 0, this._frames[_currentFrame].Duration);
        }

        private static async Task<BitmapImage> GetBitmapAsync(byte[] data)
        {
            var bitmapImage = new BitmapImage();

            using var stream = new InMemoryRandomAccessStream();
            using (var writer = new DataWriter(stream))
            {
                writer.WriteBytes(data);
                await writer.StoreAsync();
                await writer.FlushAsync();
                writer.DetachStream();
            }
            stream.Seek(0);
            bitmapImage.SetSource(stream);
            return bitmapImage;
        }

        private async Task UpdateImage(byte[] bytes)
        {
            if (bytes.Length < 4) return;
            var sig = Encoding.UTF8.GetString(bytes[0..4]);
            if (sig is "RIFF" or "WEBP")
            {
                GlobalLock.Instance.LockItems.Add(this.GetHashCode());
                this._logger.LogInformation("Loading WebP image");
                WebP.WebPLoad(bytes, out var result);
                if (result != null && result.Value.Frames.Count > 0)
                {
                    this._frames = result.Value.Frames;
                    this._webPImageWidth = result.Value.Width;
                    this._webPImageHeight = result.Value.Height;
                    if (1 < result.Value.Frames.Count)
                    {
                        for (var i = 0; i < this._frames.Count; i++)
                        {
                            await Task.Run(() =>
                            {
                                DispatcherQueue.TryEnqueue(Callback);
                                return;

                                async void Callback()
                                {
                                    var wb = new WriteableBitmap(result.Value.Width, result.Value.Height);
                                    await wb.PixelBuffer.AsStream().WriteAsync(this._frames[i].Data);
                                    this._bitmaps.Add(wb);
                                    this.image.Source = wb;
                                    //var bitmap = new BitmapImage();
                                    //using var ms = new MemoryStream(this._frames[i].Data);
                                    //bitmap.SetSource(ms.AsRandomAccessStream());
                                    //this.image.Source = bitmap;
                                    //this._bitmaps.Add(bitmap);
                                    //this.Bitmaps.Add(await GetBitmapAsync(this.frames[i].Data));
                                }
                            });
                        }
                        this.image.Source = this._bitmaps[_currentFrame];
                        this._timer.Interval = new TimeSpan(0, 0, 0, 0, this._frames[_currentFrame].Duration);
                        this._timer.Start();
                    }
                    else
                    {
                        await Task.Run(() =>
                        {
                            DispatcherQueue.TryEnqueue(Callback);
                            return;

                            async void Callback()
                            {
                                var wb = new WriteableBitmap(result.Value.Width, result.Value.Height);
                                await wb.PixelBuffer.AsStream().WriteAsync(this._frames[0].Data);
                                this.image.Source = wb;
                                //var bitmap = new BitmapImage();
                                //using var ms = new MemoryStream(this._frames[0].Data);
                                //bitmap.SetSource(ms.AsRandomAccessStream());
                                //this.image.Source = bitmap;
                                //this.Bitmaps.Add(await GetBitmapAsync(this.frames[0].Data));
                            }
                        });
                    }
                    //this.image.Width = this.Width;
                    //this.image.Height = this.Height;
                    this.image.HorizontalAlignment = HorizontalAlignment.Stretch;
                    this.image.VerticalAlignment = VerticalAlignment.Stretch;
                    this.Aspect = (double)this._webPImageWidth / this._webPImageHeight;
                }
                GlobalLock.Instance.LockItems.Remove(this.GetHashCode());
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    using var ms = new MemoryStream(bytes);
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(ms.AsRandomAccessStream());
                    this.image.Source = bitmapImage;
                    this.Aspect = (double)bitmapImage.PixelWidth / bitmapImage.PixelHeight;
                    this.image.HorizontalAlignment = HorizontalAlignment.Stretch;
                    this.image.VerticalAlignment = VerticalAlignment.Stretch;
                });
            }
            if (ImageLoadedEvent != null) this.ImageLoadedEvent(EventArgs.Empty);
        }

        private async void Imager_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Url == null) return;
            this._logger.LogInformation("Imager loaded: {url}", Url);
            await ImageCache.Instance.GetImage(this.Url, UpdateImage);
        }

        private void Imager_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.image.MaxWidth = this.MaxWidth;
        }
    }
}
