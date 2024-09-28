using Microsoft.UI.Xaml;
using Mislint.Core;
using System.Diagnostics;
using System.Threading.Tasks;

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
            this.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedException;
            this.InitializeComponent();
            this.AddOtherProvider(new Microsoft.UI.Xaml.XamlTypeInfo.XamlControlsXamlMetaDataProvider());
        }

        private static void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Trace.WriteLine($"Exception: {e.Exception}\nMessage: {e.Exception.Message}\nTrace: {e.Exception.StackTrace}\nSource: {e.Exception.Source}");
            MainWindow.ShowDialog($"Exception: {e.Exception}\nMessage: {e.Exception.Message}\nTrace: {e.Exception.StackTrace}\nSource: {e.Exception.Source}");
            e.Handled = true;
        }

        private static void OnUnobservedException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Trace.WriteLine($"Exception: {e.Exception}\nMessage: {e.Exception.Message}\nTrace: {e.Exception.StackTrace}\nSource: {e.Exception.Source}");
            MainWindow.ShowDialog($"Exception: {e.Exception}\nMessage: {e.Exception.Message}\nTrace: {e.Exception.StackTrace}\nSource: {e.Exception.Source}");
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (Core.Settings.Instance.Setting.Host != null && Core.Settings.Instance.Setting.Token != null)
            {
                Shared.MisharpApp = new Misharp.App(Core.Settings.Instance.Setting.Host, Core.Settings.Instance.Setting.Token, Shared.HttpClient, true);
                Shared.I = (await Shared.MisharpApp.IApi.I()).Result;
                Shared.Meta = (await Shared.MisharpApp.MetaApi.Meta()).Result;
                Shared.Emojis = (await Shared.MisharpApp.EmojisApi.Emojis()).Result.Emojis;
            }
            MainWindow.Activate();
        }
    }
}
