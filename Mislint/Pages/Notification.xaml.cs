using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Mislint.Core;
using System.Diagnostics;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mislint.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Notification : Page
    {
        private bool _loading = false;
        private string? _lastId = null;
        public Notification()
        {
            this.InitializeComponent();
        }

        private async void NotificationPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadNotifications();
        }

        public async Task LoadNotifications()
        {
            this._loading = true;
            var notifications = (await Shared.MisharpApp.IApi.Notifications(limit: 10, untilId: this._lastId)).Result;
            foreach (var notification in notifications)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    Debug.WriteLine(notification);
                });
            }
            this._lastId = notifications[^1].Id;
            this._loading = false;
        }
    }
}
