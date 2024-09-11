using Microsoft.UI.Xaml;
using Mislint.Core;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.ViewManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mislint
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow MainWindow = new();
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.UnhandledException += (sender, e) => {
                Debug.WriteLine(e.Message);
            };
            this.InitializeComponent();
            this.AddOtherProvider(new Microsoft.UI.Xaml.XamlTypeInfo.XamlControlsXamlMetaDataProvider());

        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Shared.MisharpApp = new Misharp.App("localhost:5173", token: "PQ7ZKoMXWWzuA8r8LjGwdBGXxGgh0Nuq", Shared.HttpClient, false);
            Shared.I = (await Shared.MisharpApp.IApi.I()).Result;
            Shared.Meta = (await Shared.MisharpApp.MetaApi.Meta()).Result;
            Shared.Emojis = (await Shared.MisharpApp.EmojisApi.Emojis()).Result.Emojis;
            MainWindow.Activate();
        }
    }
}
