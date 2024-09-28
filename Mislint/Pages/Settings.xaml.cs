using Microsoft.UI.Xaml.Controls;
using Mislint.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mislint.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox == this.Host)
                {
                    Core.Settings.Instance.Setting.Host = textBox.Text;
                }
                else if (textBox == this.Token)
                {
                    Core.Settings.Instance.Setting.Token = textBox.Text;
                }
            }
        }

        private void Settings_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (Core.Settings.Instance.Setting.Host != null)
            {
                this.Host.Text = Core.Settings.Instance.Setting.Host.ToString();
            }
            if (Core.Settings.Instance.Setting.Token != null)
            {
                this.Token.Text = Core.Settings.Instance.Setting.Token.ToString();
            }
        }

        private async void Save_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Core.Settings.Instance.Save();
            if (Core.Settings.Instance.Setting.Host != null && Core.Settings.Instance.Setting.Token != null)
            {
                Shared.MisharpApp = new Misharp.App(Core.Settings.Instance.Setting.Host, Core.Settings.Instance.Setting.Token, Shared.HttpClient, true);
                Shared.I = (await Shared.MisharpApp.IApi.I()).Result;
                Shared.Meta = (await Shared.MisharpApp.MetaApi.Meta()).Result;
                Shared.Emojis = (await Shared.MisharpApp.EmojisApi.Emojis()).Result.Emojis;
            }
        }
    }
}
