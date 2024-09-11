using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions.Generated;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Mislint.Core;
using Windows.UI;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mislint.Components
{
    public partial class Note : UserControl
    {
        public string noteId;
        private Misharp.Model.Note note;
        private Misharp.Model.Note renote = null;
        private bool reacted = false;
        private readonly DispatcherTimer timer = new();
        public Note(Misharp.Model.Note note)
        {
            this.InitializeComponent();
            this.note = note;
            this.timer.Interval = new TimeSpan(0, 0, 1);
            this.timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, object e)
        {
            AdjustTime(this.note.CreatedAt, out var str);
            this.createdAt.Text = str;
            if (this.renote != null)
            {
                AdjustTime(this.renote.CreatedAt, out str);
                this.renoteCreatedAt.Text = str;
            }
        }

        private void AdjustTime(DateTime time, out string str)
        {
            Shared.TimeSpanToDateParts(DateTime.UtcNow, time, out var years, out var months, out var days, out var hours, out var minutes, out var seconds);
            if (years > 0)
            {
                str = $"{years}年前";
                this.timer.Stop();
            }
            else if (months > 0)
            {
                str = $"{months}ヶ月前";
                this.timer.Stop();
            }
            else if (days > 0)
            {
                str = $"{days}日前";
                this.timer.Stop();
            }
            else if (hours > 0)
            {
                str = $"{hours}時間前";
                this.timer.Interval = new TimeSpan(0, 0, 45);
            }
            else if (minutes > 0)
            {
                str = $"{minutes}分前";
                this.timer.Interval = new TimeSpan(0, 0, 45);
            }
            else if (seconds > 0)
            {
                str = $"{seconds}秒前";
                this.timer.Interval = new TimeSpan(0, 0, 5);
            }
            else
            {
                str = "たった今";
                this.timer.Interval = new TimeSpan(0, 0, 5);
            }
        }

        private async void Note_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.note.Renote != null)
            {
                this.renote = this.note;
                this.note = this.note.Renote;
                this.renoteUserIcon.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage()
                    {
                        UriSource = new Uri(this.renote.User.AvatarUrl),
                    },
                };
                this.renoteName.Text = this.renote.User.Name + "がリノートしました。";
                this.renoteVisibility.Glyph = this.renote.Visibility switch
                {
                    //"public" => "\uE774",
                    "home" => "\uE80F",
                    "followers" => "\uE72E",
                    "specified" => "\uE715",
                    _ => "",
                };
                this.renoteInfo.Visibility = Visibility.Visible;
            }
            this.content.Text = this.note.Text;
            this.visibility.Glyph = this.note.Visibility switch
            {
                //"public" => "\uE774",
                "home" => "\uE80F",
                "followers" => "\uE72E",
                "specified" => "\uE715",
                _ => "",
            };
            FlyoutBase.SetAttachedFlyout(this.reactionButton, ReactionDeck.Instance.GetReactionDeckFlyout());
            // user
            this.name.Text = this.note.User.Name ?? this.note.User.Username;
            this.username.Text = $"@{this.note.User.Username}@{this.note.User.Host ?? Shared.MisharpApp.Host}";
            this.icon.Url = this.note.User.AvatarUrl;
            this.timer.Start();
            #region instance ticker
            var baseColor = (System.Drawing.Color)new System.Drawing.ColorConverter().ConvertFromString(this.note.User.Instance != null ? this.note.User.Instance.ThemeColor : (Shared.Meta != null ? Shared.Meta.ThemeColor ?? "#86b300" : "#86b300"));
            var startColor = Color.FromArgb(255, baseColor.R, baseColor.G, baseColor.B);
            var endColor = Color.FromArgb(0, baseColor.R, baseColor.G, baseColor.B);
            this.instanceTicker.Background = new LinearGradientBrush()
            {
                GradientStops = new GradientStopCollection()
                {
                    new GradientStop()
                    {
                        Color = startColor,
                        Offset = 0,
                    },
                    new GradientStop()
                    {
                        Color = endColor,
                        Offset = 0.9,
                    },
                },
                StartPoint = new Windows.Foundation.Point(0, 0),
                EndPoint = new Windows.Foundation.Point(1, 0),
            };
            this.instanceIcon.Source = new BitmapImage(new Uri(this.note.User.Instance != null ?
                                                this.note.User.Instance.IconUrl : (Shared.Meta != null ? Shared.Meta.IconUrl ?? "http://127.0.0.1:5173/favicon.ico" : "http://127.0.0.1:5173/favicon.ico")));
            this.instanceName.Text = this.note.User.Instance != null
                ? this.note.User.Instance.Name 
                : Shared.Meta != null ? Shared.Meta.Name ?? "Misskey" : "Misskey";
            #endregion
            #region files
            var files = note.Files;
            if (files != null)
            {
                var linearGradientBrush = new LinearGradientBrush();
                for (int i = 1; i < 100 + 1; i++)
                {
                    if (i % 2 == 0)
                    {
                        linearGradientBrush.GradientStops.Add(new GradientStop()
                        {
                            Color = new Color()
                            {
                                A = 255,
                                R = 34,
                                G = 34,
                                B = 34,
                            },
                            Offset = i / 100.0,
                        });
                        linearGradientBrush.GradientStops.Add(new GradientStop()
                        {
                            Color = Colors.Black,
                            Offset = i / 100.0,
                        });
                    }
                    else
                    {
                        linearGradientBrush.GradientStops.Add(new GradientStop()
                        {
                            Color = Colors.Black,
                            Offset = i / 100.0,
                        });
                        linearGradientBrush.GradientStops.Add(new GradientStop()
                        {
                            Color = new Color()
                            {
                                A = 255,
                                R = 34,
                                G = 34,
                                B = 34,
                            },
                            Offset = i / 100.0,
                        });
                    }
                }
                foreach (var file in files)
                {
                    var image = new Image()
                    {
                        Source = new BitmapImage(new Uri(file.Url)),
                    };
                    image.Tapped += async (_, _) =>
                    {
                        ContentDialog dialog = new ContentDialog
                        {
                            XamlRoot = this.XamlRoot,
                            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                            Content = new Image()
                            {
                                Source = new BitmapImage(new Uri(file.Url)),
                            },
                        };
                        dialog.PointerEntered += (_, _) =>
                        {
                            Debug.WriteLine("entered");
                        };
                        var result = await dialog.ShowAsync();
                    };
                    var width = files.Count == 1 || this.rightSideRoot.ActualWidth < 360 ? this.rightSideRoot.ActualWidth - 20 : (this.rightSideRoot.ActualWidth - 20) / 2;
                    if (double.IsNaN(width) || width < 150) width = 150;
                    var border = new Border()
                    {
                        BorderBrush = new SolidColorBrush(Colors.Transparent),
                        BorderThickness = new Thickness(0),
                        Background = linearGradientBrush,
                        CornerRadius = new CornerRadius(10),
                        Margin = new Thickness(5),
                        Width = width,
                        MinWidth = 150,
                        MinHeight = 150,
                        Child = image,
                        HorizontalAlignment = HorizontalAlignment.Stretch,                        
                    };
                    this.SizeChanged += (sender, e) =>
                    {
                        border.Width = files.Count == 1 || this.rightSideRoot.ActualWidth < 360 ? 
                            this.rightSideRoot.ActualWidth - 20 : (this.rightSideRoot.ActualWidth - 20) / 2;
                        var height = this.attachedFiles.ActualHeight - this.content.ActualHeight - 100;
                        if (height < 200 || double.IsNaN(height))
                            height = 200;
                        border.MaxHeight = height;
                    };
                    this.attachedFiles.Children.Add(border);
                }
                this.attachedFiles.Visibility = Visibility.Visible;
            }
            #endregion
            #region reactions
            var reactions = note.Reactions.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            var reactionEmojis = note.ReactionEmojis;
            if (reactions != null && reactionEmojis != null)
            {
                foreach (var reaction in reactions)
                {
                    DispatcherQueue.TryEnqueue(() => {
                        var reactionName = reaction.Key;
                        var emojiUrl = "";
                        bool isRemote;
                        if (reactionName.StartsWith(':'))
                        {
                            reactionName = reactionName.Remove(reactionName.Length - 1, 1).Remove(0, 1);
                            emojiUrl = $"http://{Shared.MisharpApp.Host}/emoji/{reactionName}.webp";
                            isRemote = !reactionName.EndsWith("@.");
                        }
                        else
                        {
                            //var emojis = Twemoji.Parse(reactionName, Twemoji.ImageType.Png);
                            //if (emojis.Length > 0) emojiUrl = emojis[0];
                            isRemote = false;
                        }
                        if (emojiUrl != "")
                        {
                            var grid = new Grid() {
                                Margin = new Thickness(5),
                            };
                            grid.ColumnDefinitions.Add(new ColumnDefinition());
                            grid.ColumnDefinitions.Add(new ColumnDefinition());
                            grid.ColumnDefinitions.Add(new ColumnDefinition());
                            var emoji = new Imager()
                            {
                                Url = emojiUrl,
                                Height = 15,
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                MaxWidth = 150,
                                Margin = new Thickness(0,5,0,5),
                            };
                            grid.Children.Add(emoji);
                            Grid.SetColumn(emoji, 0);
                            var count = new TextBlock { 
                                Text = $":{reaction.Value}",
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(0,0,5,0),
                            };
                            Grid.SetColumn(count, 1);
                            grid.Children.Add(count);
                            emoji.ImageLoadedEvent += (sender) =>
                            {
                                emoji.Width = emoji.Aspect * emoji.Height + count.ActualWidth;
                            };
                            if (!isRemote)
                            {
                                grid.CornerRadius = new CornerRadius(5);
                                grid.Background = new SolidColorBrush(new Color()
                                {
                                    A = 125,
                                    R = 50,
                                    G = 50,
                                    B = 50,
                                });
                                grid.PointerEntered += (sender, e) =>
                                {
                                    grid.Background = new SolidColorBrush(new Color()
                                    {
                                        A = 125,
                                        R = 10,
                                        G = 10,
                                        B = 10,
                                    });
                                };
                                grid.PointerExited += (sender, e) =>
                                {
                                    grid.Background = new SolidColorBrush(new Color()
                                    {
                                        A = 125,
                                        R = 50,
                                        G = 50,
                                        B = 50,
                                    });
                                };
                            } 
                            if (reaction.Key == note.MyReaction)
                            {
                                grid.Background = new SolidColorBrush(new Color()
                                {
                                    A = 125,
                                    R = 240,
                                    G = 88,
                                    B = 124,
                                });
                                reacted = true;
                                (reactionButton.Child as FontIcon).Glyph = "\uE949";
                            }
                            #region Tooltip
                            var tooltip = new ToolTip();
                            var tooltipGrid = new Grid();
                            tooltip.Opened += async (sender, e) =>
                            {
                                var progressRing = new ProgressRing();
                                tooltipGrid.Children.Add(progressRing);
                                var reactions = (await Shared.MisharpApp.NotesApi.Reactions(this.note.Id, reaction.Key)).Result;
                                var tooltipEmoji = new Imager()
                                {
                                    Url = emojiUrl,
                                    Height = 35,
                                    VerticalAlignment = VerticalAlignment.Top,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    MaxWidth = 75,
                                    Margin = new Thickness(0, 5, 0, 5),
                                };
                                var tooltipReactionName = new TextBlock()
                                {
                                    Text = reaction.Key,
                                    MaxWidth = 75,
                                    TextWrapping = TextWrapping.Wrap,
                                    VerticalAlignment = VerticalAlignment.Top,
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                };
                                var tooltipReactionInfo = new StackPanel()
                                {
                                    Orientation = Orientation.Vertical,
                                    Children = {
                                        tooltipEmoji,
                                        tooltipReactionName,
                                    },
                                };
                                var tooltipReactionUsers = new StackPanel()
                                {
                                    Orientation = Orientation.Vertical,
                                };
                                GlobalLock.Instance.LockItems.Add(this.GetHashCode());
                                foreach (var reactionItem in reactions)
                                {
                                    var reactionUser = new StackPanel()
                                    {
                                        Orientation = Orientation.Horizontal,
                                        Children =
                                        {
                                            new Border()
                                            {
                                                BorderBrush = new SolidColorBrush(Colors.Transparent),
                                                BorderThickness = new Thickness(0),
                                                CornerRadius = new CornerRadius(100),
                                                Child = new Imager()
                                                {
                                                    Url = reactionItem.User.AvatarUrl,
                                                    Width = 20,
                                                    Height = 20,
                                                }
                                            },
                                            new TextBlock(){
                                                Text = reactionItem.User.Name ?? reactionItem.User.Username,
                                                Margin = new Thickness(2),
                                                MaxWidth = 75,
                                                TextTrimming = TextTrimming.CharacterEllipsis,
                                            }
                                        }
                                    };
                                    tooltipReactionUsers.Children.Add(reactionUser);
                                }
                                GlobalLock.Instance.LockItems.Remove(this.GetHashCode());
                                tooltipGrid.Children.Add(tooltipReactionInfo);
                                var separator = new Border()
                                {
                                    Width = 1,
                                    Background = new SolidColorBrush(Colors.Gray),
                                    Margin = new Thickness(5),
                                };
                                tooltipGrid.Children.Remove(progressRing);
                                tooltipGrid.Children.Add(separator);
                                tooltipGrid.Children.Add(tooltipReactionUsers);
                                tooltipGrid.ColumnDefinitions.Add(new ColumnDefinition());
                                tooltipGrid.ColumnDefinitions.Add(new ColumnDefinition());
                                tooltipGrid.ColumnDefinitions.Add(new ColumnDefinition());
                                Grid.SetColumn(tooltipReactionInfo, 0);
                                Grid.SetColumn(separator, 1);
                                Grid.SetColumn(tooltipReactionUsers, 2);
                            };

                            tooltip.Content = tooltipGrid;
                            ToolTipService.SetToolTip(grid, tooltip);
                            #endregion
                            grid.Tag = reaction.Key;
                            this.reactions.Children.Add(grid);
                        }
                    });
                }
            }
            #endregion
        }

        private void ActionPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is StackPanel stackPanel)
            {
                foreach (var item in stackPanel.Children)
                {
                    FontIcon fontIcon = default;
                    if (item is FontIcon icon1) fontIcon = icon1;
                    else if (item is Border button && button.Child is FontIcon icon2) fontIcon = icon2;
                    fontIcon.Foreground = new SolidColorBrush(new Color() { A = 255, R = 0xA0, G = 0xA0, B = 0xA0 });
                }
            }
        }

        private void ActionPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is StackPanel stackPanel)
            {
                foreach (var item in stackPanel.Children)
                {
                    FontIcon fontIcon = default;
                    if (item is FontIcon icon1) fontIcon = icon1;
                    else if (item is Border button && button.Child is FontIcon icon2) fontIcon = icon2;
                    fontIcon.Foreground = new SolidColorBrush(new Color() { A = 255, R = 0x40, G = 0x40, B = 0x40 });
                }
            }
        }

        private async void ReactionButton_Tapped(object sender, TappedRoutedEventArgs _)
        {
            if (!reacted)
            {
                ReactionDeck.Instance.Reacted += ReactionDeck_Reacted;
                ReactionDeck.Instance.GetReactionDeckFlyout().Closing += ReactionDeck_Closing;
                ReactionDeck.Instance.CurrentNoteId = this.note.Id;
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
            else
            {
                foreach (var reaction in this.reactions.Children)
                {
                    if (reaction is Grid grid && grid.Tag.ToString() == this.note.MyReaction)
                    {
                        this.reactions.Children.Remove(grid);
                        break;
                    }
                }
                (reactionButton.Child as FontIcon).Glyph = "\uE710";
                this.reacted = false;
                await Shared.MisharpApp.NotesApi.ReactionsApi.Delete(this.note.Id);
            }
        }

        private async void ReactionDeck_Reacted(object sender, ReactionDeck.ReactedEventArgs e)
        {
            var contained = false;
            await Shared.MisharpApp.NotesApi.ReactionsApi.Create(this.note.Id, $":{e.Emoji}:");
            foreach (var reaction in this.reactions.Children)
            {
                if (reaction is Grid grid)
                {
                    Debug.WriteLine(grid.Tag.ToString());
                    contained = true;
                    break;
                }
            }
            if (!contained)
            {

            }
            this.reacted = true;
        }

        private void ReactionDeck_Closing(object sender, FlyoutBaseClosingEventArgs e)
        {
            ReactionDeck.Instance.CurrentNoteId = null;
            ReactionDeck.Instance.Reacted -= ReactionDeck_Reacted;
            ReactionDeck.Instance.GetReactionDeckFlyout().Closing -= ReactionDeck_Closing;
        }

        private void RenoteButton_Tapped(object sender, TappedRoutedEventArgs _)
        {

        }
    }
}
