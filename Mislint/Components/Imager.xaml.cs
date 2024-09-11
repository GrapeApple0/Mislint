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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mislint.Components
{
    public partial class Imager : UserControl
    {
        private List<WebP.WebPFrame> frames = new();
        private List<ImageSource> Bitmaps = new();
        private int currentFrame = 0;
        private readonly DispatcherTimer timer = new();
        private int width;
        private int height;
        private ILogger logger;
        public delegate void ImageLoaded<T>(T args);
        public event ImageLoaded<EventArgs> ImageLoadedEvent;
        public double Aspect { get; private set; } = 1;
        private string _url;
        public string Url { 
            get { return _url; } 
            set
            {
                _url = value;
                if (this.IsLoaded) this.Imager_Loaded(this, null);
            }
        }
        public Imager()
        {
            this.InitializeComponent();
            this.logger = Logger.Instance.loggerFactory.CreateLogger("Imager");
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, object e)
        {
            if (this.currentFrame == this.frames.Count - 1) currentFrame = 0;
            else currentFrame++;
            this.image.Source = this.Bitmaps[currentFrame];
            this.timer.Interval = new TimeSpan(0, 0, 0, 0, this.frames[currentFrame].Duration);
        }

        private static async Task<BitmapImage> GetBitmapAsync(byte[] data)
        {
            BitmapImage bitmapImage;
            bitmapImage = new BitmapImage();

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
            if (sig == "RIFF" || sig == "WEBP")
            {
                GlobalLock.Instance.LockItems.Add(this.GetHashCode());
                this.logger.LogInformation("Loading WebP image");
                WebP.WebPLoad(bytes, out var result);
                if (result != null && result.Value.frames.Count > 0)
                {
                    this.frames = result.Value.frames;
                    this.width = result.Value.width;
                    this.height = result.Value.height;
                    if (1 < result.Value.frames.Count)
                    {
                        for (int i = 0; i < this.frames.Count; i++)
                        {
                            await Task.Run(() =>
                            {
                                DispatcherQueue.TryEnqueue(async () =>
                                {
                                    var wb = new WriteableBitmap(result.Value.width, result.Value.height);
                                    await wb.PixelBuffer.AsStream().WriteAsync(this.frames[i].Data);
                                    this.Bitmaps.Add(wb);
                                    //this.Bitmaps.Add(await GetBitmapAsync(this.frames[i].Data));
                                });
                            });
                        }
                        this.image.Source = this.Bitmaps[currentFrame];
                        this.timer.Interval = new TimeSpan(0, 0, 0, 0, this.frames[currentFrame].Duration);
                        this.timer.Start();
                    }
                    else
                    {
                        await Task.Run(() =>
                        {
                            DispatcherQueue.TryEnqueue(async () =>
                            {
                                var wb = new WriteableBitmap(result.Value.width, result.Value.height);
                                await wb.PixelBuffer.AsStream().WriteAsync(this.frames[0].Data);
                                this.image.Source = wb;
                                //this.Bitmaps.Add(await GetBitmapAsync(this.frames[0].Data));
                            });
                        });
                    }
                    this.image.Width = this.width;
                    this.image.Height = this.Height;
                    this.Aspect = (double)this.width / this.height;
                }
                GlobalLock.Instance.LockItems.Remove(this.GetHashCode());
            }
            else
            {
                var bitmapImage = new BitmapImage(new Uri(this.Url))
                {
                    DecodePixelHeight = (int)this.Height
                };
                this.image.Source = bitmapImage;
                this.Aspect = (double)bitmapImage.PixelWidth / bitmapImage.PixelHeight;
                this.image.Height = this.Height;
                //this.image.Margin = new Thickness(5,0,5,0);
            }
            if (ImageLoadedEvent != null) this.ImageLoadedEvent(new EventArgs());
        }

        private async void Imager_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Url != null)
            {
                this.logger.LogInformation("Imager loaded: {url}", Url);
                await ImageCache.Instance.GetImage(this.Url, UpdateImage);
            }
        }

        private void Imager_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.image.MaxWidth = this.MaxWidth;
        }
    }
}
