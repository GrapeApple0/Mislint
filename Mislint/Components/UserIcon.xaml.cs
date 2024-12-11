using Microsoft.UI.Xaml.Controls;
using Misharp.Models;
using System.Diagnostics;
using System.Numerics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mislint.Components
{
    public sealed partial class UserIcon : UserControl
    {
        private UserLiteModel _user;
        public UserLiteModel User { 
            get
            {
                return _user;
            }
            set
            {
                _user = value;
                int size = (int)this.Height;//(int)this.ActualWidth;
                if (size <= 0) size = 50;
                DispatcherQueue.TryEnqueue(() =>
                {
                    foreach (var avatarDecoration in this._user.AvatarDecorations)
                    {
                        
                        Imager imager = new ()
                        {
                            Url = avatarDecoration.Url.ToString(),
                            Width = size,
                            Height = size,
                            Scale = new Vector3(2.0f, 2.0f, 1.0f),
                            Translation = new Vector3(-(size / 2) + ((float)avatarDecoration.OffsetX * size * 2), -(size / 2) + ((float)avatarDecoration.OffsetY * size * 2), 0.0f),  
                        };
                        imager.ImageComponent.RenderTransform = new Microsoft.UI.Xaml.Media.CompositeTransform()
                        {
                            CenterX = (size / 2),
                            CenterY = (size / 2),
                            Rotation = (180 / 0.5) * (double)avatarDecoration.Angle,
                        };
                        this.Root.Children.Add(imager);
                    }
                });
                this.Icon.Width = size;
                this.Icon.Height = size;
                this.RoundCornerBorder.Width = size;
                this.RoundCornerBorder.Height = size;
                this.Icon.Url = this._user.AvatarUrl.ToString();
            }
        }

        public UserIcon()
        {
            this.InitializeComponent();
        }

        private void UserIcon_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }
    }
}
