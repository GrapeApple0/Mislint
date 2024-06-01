using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Mislint.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mislint
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private Misharp.App _app;
        public MainWindow()
        {
            this.InitializeComponent();
            this._app = new Misharp.App("misskey.04.si", token: "");
        }

        private async void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            this.notes.Children.Add(new Note(this._app, (await this._app.NotesApi.Show("9szrws74pa")).Result));
            this.Activated -= Window_Activated;
            //var tl = (await this._app.NotesApi.Timeline()).Result;
            //foreach (var note in tl)
            //{
            //    var noteComponent = new Note(this._app, note);
            //    this.notes.Children.Add(noteComponent);
            //}
        }
    }
}
