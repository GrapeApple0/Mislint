using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Mislint.Core;
using Mislint.Pages;
using System.Collections.Generic;
using Windows.UI;
using Misharp.Models;

namespace Mislint.Components
{
    public partial class Note : UserControl
    {
        private NoteModel _note;
        private NoteModel _renote = null;
        private bool _reacted = false;
        private readonly DispatcherTimer _timer = new();
        private Flyout _renoteFlyout = null;
        private ILogger _logger;
        public Note(NoteModel note)
        {
            this.InitializeComponent();
            this._note = note;
            this._timer.Interval = new TimeSpan(0, 0, 1);
            this._timer.Tick += Timer_Tick;
            this._logger = Logger.Instance.loggerFactory.CreateLogger("Note");
        }

        private void Timer_Tick(object sender, object e)
        {
            this.CreatedAt.Text = AdjustTime(this._note.CreatedAt ?? DateTime.Now);
            if (this._renote != null)
            {
                this.RenoteCreatedAt.Text = AdjustTime(this._renote.CreatedAt ?? DateTime.Now);
            }
        }

        private string AdjustTime(DateTime time)
        {
            Shared.TimeSpanToDateParts(DateTime.UtcNow, time, out var years, out var months, out var days, out var hours, out var minutes, out var seconds);
            if (years > 0)
            {
                this._timer.Stop();
                return $"{years}年前";
            }
            else if (months > 0)
            {
                this._timer.Interval = new TimeSpan(0, 45, 0);
                return $"{months}ヶ月前";
            }
            else if (days > 0)
            {
                this._timer.Interval = new TimeSpan(0, 45, 0);
                return $"{days}日前";
            }
            else if (hours > 0)
            {
                this._timer.Interval = new TimeSpan(0, 0, 45);
                return $"{hours}時間前";
            }
            else if (minutes > 0)
            {
                this._timer.Interval = new TimeSpan(0, 0, 45);
                return $"{minutes}分前";
            }
            else if (seconds > 0)
            {
                this._timer.Interval = new TimeSpan(0, 0, 5);
                return $"{seconds}秒前";
            }
            else
            {
                this._timer.Interval = new TimeSpan(0, 0, 5);
                return "たった今";
            }
        }

        private Color GetInstanceColor()
        {
            var colorCode = "#86b300";
            if (this._note.User.Instance?.ThemeColor != null)
                colorCode = this._note.User.Instance.ThemeColor;
            else if (Shared.Meta != null && Shared.Meta.ThemeColor != null)
                colorCode = Shared.Meta.ThemeColor;
            var baseColor = (System.Drawing.Color)new System.Drawing.ColorConverter().ConvertFromString(colorCode)!;
            return Color.FromArgb(255, baseColor.R, baseColor.G, baseColor.B);
        }

        private string GetInstanceIcon()
        {
            var iconUrl = $"http{(Shared.MisharpApp.UseHttps ? "s" : "")}://{Shared.MisharpApp.Host}/favicon.ico";
            if (this._note.User.Instance?.IconUrl != null)
                iconUrl = this._note.User.Instance.IconUrl;
            else if (Shared.Meta != null && Shared.Meta.IconUrl != null)
                iconUrl = Shared.Meta.IconUrl;
            return iconUrl;
        }

        private void Note_Loaded(object sender, RoutedEventArgs e)
        {
            if (this._note.Renote != null)
            {
                this._renote = this._note;
                this._note = this._note.Renote;
                this.RenoteUserIcon.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage()
                    {
                        UriSource = new Uri(this._renote.User.AvatarUrl.ToString()),
                    },
                };
                this.RenoteName.Text = this._renote.User.Name + "がリノートしました";
                this.RenoteVisibility.Glyph = this._renote.Visibility switch
                {
                    //"public" => "\uE774",
                    NoteVisibilityEnum.Home => "\uE80F",
                    NoteVisibilityEnum.Followers => "\uE72E",
                    NoteVisibilityEnum.Specified => "\uE715",
                    _ => "",
                };
                this.RenoteInfo.Visibility = Visibility.Visible;
            }
            if (this._note.Cw != null && this._note.Cw != "")
            {
                this.content.Visibility = Visibility.Collapsed;
                this.Cw.Visibility = Visibility.Visible;
                this.CwText.Text = this._note.Cw;
                this.CwButton.Content = new TextBlock()
                {
                    Text = $"もっと見る({(this._note.Text != null ? this._note.Text.Count() : "0")}文字)",
                    HorizontalTextAlignment = TextAlignment.Center,
                    FontSize = 12,
                };
            }
            this.Text.Text = this._note.Text;
            if (0 < this._note.RepliesCount)
            {
                this.ReplyCount.Text = this._note.RepliesCount.ToString();
                this.ReplyCount.Visibility = Visibility.Visible;
            }

            if (this._renote != null)
            {
                this.RenoteVisibility.Glyph = this._renote.Visibility switch
                {
                    //"public" => "\uE774",
                    NoteVisibilityEnum.Home => "\uE80F",
                    NoteVisibilityEnum.Followers => "\uE72E",
                    NoteVisibilityEnum.Specified => "\uE715",
                    _ => "",
                };
            }
            FlyoutBase.SetAttachedFlyout(this.ReactionButton, ReactionDeck.Instance.GetReactionDeckFlyout());
            // user
            this.DisplayName.Text = this._note.User.Name ?? this._note.User.Username;
            this.Username.Text = $"@{this._note.User.Username}@{this._note.User.Host ?? Shared.MisharpApp.Host}";
            this.Icon.User = this._note.User;
            this._timer.Start();

            var baseColor = GetInstanceColor();
            var startColor = baseColor;
            var endColor = baseColor;
            endColor.A = 0;
            this.InstanceTicker.Background = new LinearGradientBrush()
            {
                GradientStops =
                [
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
                ],
                StartPoint = new Windows.Foundation.Point(0, 0),
                EndPoint = new Windows.Foundation.Point(1, 0),
            };
            this.InstanceIcon.Source = new BitmapImage(new Uri(GetInstanceIcon()));
            this.InstanceName.Text = this._note.User.Instance != null
                ? this._note.User.Instance.Name
                : Shared.Meta != null ? Shared.Meta.Name ?? "Misskey" : "Misskey";
            
            var files = this._note.Files;
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
                        Source = new BitmapImage(new Uri(file.Url.ToString())),
                    };
                    image.Tapped += (_, _) =>
                    {
                        App.MainWindow.ShowPopup(new Image()
                        {
                            Source = new BitmapImage(new Uri(file.Url.ToString())),
                            VerticalAlignment = VerticalAlignment.Stretch,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Stretch = Stretch.Uniform,
                        });
                    };
                    var width =this.RightSideRoot.ActualWidth - 20;
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
                        border.Width =   this.RightSideRoot.ActualWidth - 20;
                        var height = this.AttachedFiles.ActualHeight - this.Text.ActualHeight - 100;
                        if (height < 200 || double.IsNaN(height))
                            height = 200;
                        border.MaxHeight = height;
                    };
                    this.AttachedFiles.Children.Add(border);
                }
                this.AttachedFiles.Visibility = Visibility.Visible;
            }
            
            var reactions = this._note.Reactions.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            var reactionEmojis = this._note.ReactionEmojis;
            if (reactions != null && reactionEmojis != null)
            {
                foreach (var reaction in reactions)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        var reactionName = reaction.Key;
                        var emojiUrl = "";
                        bool isRemote;
                        if (reactionName.StartsWith(':'))
                        {
                            reactionName = reactionName.Remove(reactionName.Length - 1, 1).Remove(0, 1);
                            emojiUrl = $"http{(Shared.MisharpApp.UseHttps ? "s" : "")}://{Shared.MisharpApp.Host}/emoji/{reactionName}.webp";
                            isRemote = !reactionName.EndsWith("@.");
                        }
                        else
                        {
                            var emojis = Twemoji.Parse(reactionName, Twemoji.ImageType.Png);
                            if (emojis.Count > 0) emojiUrl = emojis[0].Url;
                            isRemote = false;
                        }
                        if (emojiUrl != "")
                        {
                            var grid = new Grid()
                            {
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
                                Margin = new Thickness(0, 5, 0, 5),
                            };
                            grid.Children.Add(emoji);
                            Grid.SetColumn(emoji, 0);
                            var count = new TextBlock
                            {
                                Text = $":{reaction.Value}",
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(0, 0, 5, 0),
                            };
                            Grid.SetColumn(count, 1);
                            grid.Children.Add(count);
                            emoji.ImageLoadedEvent += (_) =>
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
                                grid.PointerEntered += Reaction_PointerEntered;
                                grid.PointerExited += Reaction_PointerExited;
                            }
                            if (reaction.Key == this._note.MyReaction)
                            {
                                grid.Background = new SolidColorBrush(new Color()
                                {
                                    A = 125,
                                    R = 240,
                                    G = 88,
                                    B = 124,
                                });
                                this._reacted = true;
                                ((FontIcon)this.ReactionButton.Child).Glyph = "\uE949";
                            }
                            
                            var tooltip = new ToolTip();
                            var tooltipGrid = new Grid();
                            tooltip.Opened += async (_, _) =>
                            {
                                var progressRing = new ProgressRing();
                                tooltipGrid.Children.Add(progressRing);
                                var reactions = (await Shared.MisharpApp.NotesApi.Reactions(this._note.Id, reaction.Key)).Result;
                                var tooltipEmoji = new Imager()
                                {
                                    Url = emojiUrl,
                                    Height = 35,
                                    VerticalAlignment = VerticalAlignment.Top,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Width = 75,
                                    Margin = new Thickness(0, 5, 0, 5),
                                };
                                var tooltipReactionName = new TextBlock()
                                {
                                    Text = reaction.Key,
                                    Width = 75,
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
                                                    Url = reactionItem.User.AvatarUrl.ToString(),
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
                                        },
                                        Width = 90,
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
                            
                            grid.Tag = reaction.Key;
                            this.Reactions.Children.Add(grid);
                        }
                    });
                }
            }
            
        }

        private void Reaction_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid grid)
            {
                grid.Background = new SolidColorBrush(new Color()
                {
                    A = 125,
                    R = 10,
                    G = 10,
                    B = 10,
                });
            }
        }

        private void Reaction_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Grid grid) return;
            grid.Background = new SolidColorBrush(new Color()
            {
                A = 125,
                R = 50,
                G = 50,
                B = 50,
            });
            if (grid.Tag != null && grid.Tag.ToString() == this._note.MyReaction)
            {
                grid.Background = new SolidColorBrush(new Color()
                {
                    A = 125,
                    R = 240,
                    G = 88,
                    B = 124,
                });
            }
        }

        private void ActionPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not StackPanel stackPanel) return;
            foreach (var item in stackPanel.Children)
            {
                FontIcon fontIcon = default;
                if (item is Border border)
                {
                    fontIcon = border.Child switch
                    {
                        FontIcon icon1 => icon1,
                        StackPanel { Children.Count: > 0 } panel => panel.Children[0] as FontIcon,
                        _ => default
                    };
                }
                if (fontIcon != null)
                    fontIcon.Foreground = new SolidColorBrush(new Color() { A = 255, R = 0xA0, G = 0xA0, B = 0xA0 });
            }
        }

        private void ActionPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not StackPanel stackPanel) return;
            foreach (var item in stackPanel.Children)
            {
                FontIcon fontIcon = default;
                if (item is Border border)
                {
                    fontIcon = border.Child switch
                    {
                        FontIcon icon1 => icon1,
                        StackPanel { Children.Count: > 0 } panel => panel.Children[0] as FontIcon,
                        _ => default
                    };
                }
                if (fontIcon != null)
                    fontIcon.Foreground = new SolidColorBrush(new Color() { A = 255, R = 0x40, G = 0x40, B = 0x40 });
            }
        }

        private async void ReactionButton_Tapped(object sender, TappedRoutedEventArgs _)
        {
            if (!this._reacted)
            {
                ReactionDeck.Instance.Reacted += ReactionDeck_Reacted;
                ReactionDeck.Instance.GetReactionDeckFlyout().Closing += ReactionDeck_Closing;
                ReactionDeck.Instance.CurrentNoteId = this._note.Id;
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
            else
            {
                foreach (var reaction in this.Reactions.Children)
                {
                    if (reaction is not Grid grid || grid.Tag.ToString() != this._note.MyReaction) continue;
                    this.Reactions.Children.Remove(grid);
                    break;
                }
                ((FontIcon)this.ReactionButton.Child).Glyph = "\uE710";
                this._reacted = false;
                await Shared.MisharpApp.NotesApi.ReactionsApi.Delete(this._note.Id);
            }
        }

        private async void ReactionDeck_Reacted(object sender, ReactionDeck.ReactedEventArgs e)
        {
            var contained = false;
            this._logger.LogInformation("{emoji} was reacted to {noteId}", e.Emoji, this._note.Id);
            await Shared.MisharpApp.NotesApi.ReactionsApi.Create(this._note.Id, $":{e.Emoji}:");
            foreach (var reaction in this.Reactions.Children)
            {
                if (reaction is not Grid grid) continue;
                contained = true;
                break;
            }
            if (!contained)
            {
                var grid = new Grid()
                {
                    Margin = new Thickness(5),
                };
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                var emoji = new Imager()
                {
                    Url = $"http{(Shared.MisharpApp.UseHttps ? "s" : "")}://{Shared.MisharpApp.Host}/emoji/{e.Emoji}.webp",
                    Height = 15,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    MaxWidth = 150,
                    Margin = new Thickness(0, 5, 0, 5),
                };
                grid.Children.Add(emoji);
                Grid.SetColumn(emoji, 0);
                var count = new TextBlock
                {
                    Text = $":1",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0),
                };
                Grid.SetColumn(count, 1);
                grid.Children.Add(count);
                emoji.ImageLoadedEvent += (_) =>
                {
                    emoji.Width = emoji.Aspect * emoji.Height + count.ActualWidth;
                };
                grid.CornerRadius = new CornerRadius(5);
                grid.Background = new SolidColorBrush(new Color()
                {
                    A = 125,
                    R = 50,
                    G = 50,
                    B = 50,
                });
                grid.PointerEntered += Reaction_PointerEntered;
                grid.PointerExited += Reaction_PointerExited;
                grid.Background = new SolidColorBrush(new Color()
                {
                    A = 125,
                    R = 240,
                    G = 88,
                    B = 124,
                });
                this._reacted = true;
                ((FontIcon)this.ReactionButton.Child).Glyph = "\uE949";
                var tooltip = new ToolTip();
                var tooltipGrid = new Grid();
                tooltip.Opened += async (_, _) =>
                {
                    var progressRing = new ProgressRing();
                    tooltipGrid.Children.Add(progressRing);
                    var reactions = (await Shared.MisharpApp.NotesApi.Reactions(this._note.Id, $":{e.Emoji}@.:")).Result;
                    var tooltipEmoji = new Imager()
                    {
                        Url = $"http{(Shared.MisharpApp.UseHttps ? "s" : "")}://{Shared.MisharpApp.Host}/emoji/{e.Emoji}.webp",
                        Height = 35,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        MaxWidth = 75,
                        Margin = new Thickness(0, 5, 0, 5),
                    };
                    var tooltipReactionName = new TextBlock()
                    {
                        Text = $":{e.Emoji}@.:",
                        MaxWidth = 75,
                        TextWrapping = TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };
                    var tooltipReactionInfo = new StackPanel()
                    {
                        Orientation = Orientation.Vertical,
                        Children =
                        {
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
                                        Url = reactionItem.User.AvatarUrl.ToString(),
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
                this.Reactions.Children.Add(grid);
            }
            ((FontIcon)this.ReactionButton.Child).Glyph = "\uE949";
            this._reacted = true;
        }

        private void ReactionDeck_Closing(object sender, FlyoutBaseClosingEventArgs e)
        {
            ReactionDeck.Instance.CurrentNoteId = null;
            ReactionDeck.Instance.Reacted -= ReactionDeck_Reacted;
            ReactionDeck.Instance.GetReactionDeckFlyout().Closing -= ReactionDeck_Closing;
        }

        private void RenoteButton_Tapped(object sender, TappedRoutedEventArgs _)
        {
            if (this._renoteFlyout == null)
            {
                var button = new Button()
                {
                    CornerRadius = new CornerRadius(3),
                    Content = new StackPanel()
                    {
                        Children =
                        {
                            new FontIcon() { FontSize = 12, Glyph = "\uE8EE", },
                            new TextBlock() { Text = "リノート", },
                        },
                        Orientation = Orientation.Horizontal,
                    },
                };
                button.Tapped += async (_, _) =>
                {
                    var note = (await Shared.MisharpApp.NotesApi.Create(renoteId: this._note.Id)).Result;
                    this._renoteFlyout?.Hide();
                };
                this._renoteFlyout = new Flyout()
                {
                    Content = new StackPanel()
                    {
                        Children =
                        {
                            button
                        },
                    },
                    Placement = FlyoutPlacementMode.Bottom,
                };
                FlyoutBase.SetAttachedFlyout(this.RenoteButton, this._renoteFlyout);
            }
            this._renoteFlyout.ShowAt((FrameworkElement)sender);
        }

        private void CwButton_Click(object sender, RoutedEventArgs e)
        {
            this.content.Visibility = this.content.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Icon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            App.MainWindow.SwitchPage(typeof(UserInfo), new Dictionary<string, string>
            {
                {
                    "UserId",this._note.User.Id
                },
            });
        }
    }
}
