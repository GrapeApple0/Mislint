using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Misharp.Controls;
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

        private string _lastId;
        private bool _loading;
        private bool _last;

        private UsersShowResponse user;
        public UserInfo()
        {
            this.InitializeComponent();
        }

        private async void UserInfo_Loaded(object sender, RoutedEventArgs e)
        {
            this.user = (await Shared.MisharpApp.UsersApi.Show(this.UserId, null)).Result;
            if (!string.IsNullOrEmpty(this.user.BannerUrl))
                this.Banner.Source = new BitmapImage(new Uri(this.user.BannerUrl));
            this.Icon.Url = this.user.AvatarUrl;
            this.name.Inlines.Add(new Run()
            {
                Text = this.user.Name ?? this.user.Username,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
            });
            this.Username.Text = $"@{this.user.Username}@{this.user.Host ?? Shared.MisharpApp.Host}";
            this.Description.Text = this.user.Description;
            var joinedAt = this.user.CreatedAt;
            this.JoinedAt.Text = $"{joinedAt.Year}/{joinedAt.Month}/{joinedAt.Day} {joinedAt.Hour}:{joinedAt.Minute}:{joinedAt.Second} ({Shared.GetTimeSpan(this.user.CreatedAt)})";

            var timeline = new Components.Timeline
            {
                TimelineType = TimelineTypeEnum.User,
                UserId = this.UserId,
                MaxWidth = 700,
            };
            this.Root.ViewChanged += (s, e) =>
            {
                if (s is ScrollViewer scrollViewer)
                    if (!(Math.Abs(scrollViewer.VerticalOffset - scrollViewer.ScrollableHeight) < 1)) return; // on bottom
                if (!this._loading && !this._last) timeline.LoadTimeline();
            };
            this.ContentStack.Children.Add(timeline);
            timeline.Reload();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Dictionary<string, string> parameters)
            {
                this.UserId = parameters["UserId"];
            }
            base.OnNavigatedTo(e);
        }
    }
}
