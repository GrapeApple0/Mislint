using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Mislint.Core;
using System.Collections.Generic;
using System;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.IO;
using Misharp.Model;
using Misharp.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using System.Collections.Specialized;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mislint.Components
{
    public sealed partial class PostForm : UserControl
    {
        private readonly ILogger logger;
        private readonly ObservableCollection<DriveFile> _files = new();
        private NotesApi.CreateVisibilityEnum _visibility = NotesApi.CreateVisibilityEnum.Public;

        public PostForm()
        {
            this.InitializeComponent();
            this.logger = Logger.Instance.loggerFactory.CreateLogger("PostForm");
            this.logger.LogInformation("PostForm initialized");
            this._files.CollectionChanged += this.Files_CollectionChanged;
        }

        private void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //this.attachedFiles.Children.Add(new Grid()
                //{
                //    Background = new ImageBrush()
                //    {
                //        Stretch = Stretch.UniformToFill,
                //        AlignmentX = AlignmentX.Center,
                //        AlignmentY = AlignmentY.Center,
                //        ImageSource = new BitmapImage(new System.Uri((this._files[^1]).Url)),
                //    },
                //    CornerRadius = new CornerRadius(4),
                //    Width = 60,
                //});
                this.AttachedFiles.Children.Add(new Grid()
                {
                    CornerRadius = new CornerRadius(10),
                    Width = 60,
                    Height = 60,
                    Margin = new Thickness(0, 0, 4, 0),
                    Children = {
                        new Image()
                        {
                            Source = new BitmapImage(new Uri(this._files[^1].Url)),
                            Width = 60,
                            Height = 60,
                            Stretch = Stretch.UniformToFill,
                        }
                    }
                });
            }
        }

        private async void PostForm_Loaded(object sender, RoutedEventArgs e)
        {
            Shared.I ??= (await Shared.MisharpApp.IApi.I()).Result;
            this.UserIcon.Url = Shared.I.AvatarUrl;
        }

        private async void PostButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            List<string> fileIds = null;
            if (this._files.Count > 0)
            {
                fileIds = this._files.ToList().ConvertAll(f => f.Id);
            }
            var result = (await Shared.MisharpApp.NotesApi.Create(text: this.content.Text, fileIds: fileIds, visibility: _visibility));

            this.content.Text = "";
        }

        private void content_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.PostButton.IsEnabled = this.content.Text.Length != 0;
        }

        private async void UploadButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker();
            var window = App.MainWindow;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add("*");
            IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();
            if (files.Count <= 0) return;
            foreach (var file in files)
            {
                await using var fs = File.OpenRead(file.Path);
                var driveFile = (await Shared.MisharpApp.DriveApi.FilesApi.Create(fs, name: file.Name, force: true)).Result;
                this._files.Add(driveFile);
                this.AttachedFiles.Visibility = Visibility.Visible;
            }
            //Parallel.ForEach(files, async (file) =>
            //{
            //    using FileStream fs = System.IO.File.OpenRead(file.Path);
            //    var driveFile = (await Shared.MisharpApp.DriveApi.FilesApi.Create(fs, name: file.Name)).Result;
            //    this._files.Add(driveFile);
            //    Debug.WriteLine(driveFile);
            //});
        }

        private void VisibilitySelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this._visibility = (NotesApi.CreateVisibilityEnum)(((ComboBox)sender).SelectedIndex);
        }
    }
}
