using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Misharp;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mislint.Components
{
    public partial class Note : UserControl
    {
        public string noteId;
        private Misharp.App _app;
        private readonly TimeZoneInfo timezone;
        private Misharp.Model.Note note;
        private Misharp.Model.Note renote = null;

        public Note(Misharp.App app, Misharp.Model.Note note)
        {
            this.InitializeComponent();
            this._app = app;
            this.note = note;
        }

        private async void Note_Loaded(object sender, RoutedEventArgs e)
        {
            this.content.Text = this.note.Text;
            // user
            this.username.Text = this.note.User.Name;
            this.icon.Background = new ImageBrush()
            {
                ImageSource = new BitmapImage()
                {
                    UriSource = new System.Uri(this.note.User.AvatarUrl),
                },
            };
            // reactions
            var reactions = JsonSerializer.Deserialize<Dictionary<string, int>>(note.Reactions.ToString());
            var reactionEmojis = JsonSerializer.Deserialize<Dictionary<string, string>>(note.ReactionEmojis.ToString());
            if (reactions != null && reactionEmojis != null)
            {
                foreach (var reaction in reactions)
                {
                    var reactionName = reaction.Key;
                    var emojiUrl = "";
                    if (reactionName.StartsWith(':'))
                    {
                        reactionName = reactionName.Remove(reactionName.Length - 1, 1).Remove(0, 1);
                        emojiUrl = $"https://misskey.04.si/emoji/{reactionName}.webp";
                        byte[] buff;
                        using (var hc = new HttpClient())
                            buff = await hc.GetByteArrayAsync(emojiUrl);
                    }
                    //else
                    //{
                    //    var emojis = Twemoji.Parse(reactionName, Twemoji.ImageType.Png);
                    //    if (emojis.Count > 0) emojiUrl = emojis[0].url;
                    //}
                    var grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition()
                    {
                        Width = GridLength.Auto,
                    });
                    grid.ColumnDefinitions.Add(new ColumnDefinition()
                    {
                        Width = GridLength.Auto,
                    });
                    var emoji = new Image()
                    {
                        Source = new BitmapImage()
                        {
                            UriSource = new Uri(emojiUrl),
                        },
                        Height = 25,
                    };
                    grid.Children.Add(emoji);
                    Grid.SetColumn(emoji, 0);
                    var count = new TextBlock { Text = $":{reaction.Value}" };
                    Grid.SetColumn(count, 1);
                    grid.Children.Add(count);
                    this.reactions.Children.Add(grid);
                }
            }
        }
    }
}
