using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Mislint.Core;
using System;
using System.Collections.Generic;
using static Misharp.Controls.UsersApi;
using static Mislint.Components.Timeline;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mislint.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserInfo : Page
    {
        public string UserId { get; set; }
        private UsersShowResponse user;
        public UserInfo()
        {
            this.InitializeComponent();
        }

        private string AdjustTime(DateTime time)
        {
            Shared.TimeSpanToDateParts(DateTime.UtcNow, time, out var years, out var months, out var days, out var hours, out var minutes, out var seconds);
            if (years > 0)
            {
                return $"{years}年前";
            }
            else if (months > 0)
            {
                return $"{months}ヶ月前";
            }
            else if (days > 0)
            {
                return $"{days}日前";
            }
            else if (hours > 0)
            {
                return $"{hours}時間前";
            }
            else if (minutes > 0)
            {
                return $"{minutes}分前";
            }
            else if (seconds > 0)
            {
                return $"{seconds}秒前";
            }
            return "たった今";
        }

        private async void UserInfo_Loaded(object sender, RoutedEventArgs e)
        {
            this.user = (await Shared.MisharpApp.UsersApi.Show(this.UserId, null)).Result;
            this.banner.Source = new BitmapImage(new Uri(this.user.BannerUrl));
            this.icon.Url = this.user.AvatarUrl;
            this.name.Inlines.Add(new Run()
            {
                Text = this.user.Name ?? this.user.Username,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
            });
            //this.banner.CornerRadius = new CornerRadius(20, 20, 0, 0);
            this.username.Text = $"@{this.user.Username}@{this.user.Host ?? Shared.MisharpApp.Host}";
            this.description.Text = this.user.Description;
            var joinedAt = this.user.CreatedAt;
            this.joinedAt.Text = $"{joinedAt.Year}/{joinedAt.Month}/{joinedAt.Day} {joinedAt.Hour}:{joinedAt.Minute}:{joinedAt.Second} ({AdjustTime(this.user.CreatedAt)})";
            var timeline = new Components.Timeline()
            {
                TimelineType = TimelineTypeEnum.User,
                UserId = this.UserId,
                MaxWidth = 700,
            };
            Grid.SetRow(timeline, 2);
            this.root.Children.Add(timeline);
            timeline.Reload();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameters = e.Parameter as Dictionary<string, string>;
            if (parameters != null)
            {
                this.UserId = parameters["UserId"];
            }
            base.OnNavigatedTo(e);
        }
    }
}
