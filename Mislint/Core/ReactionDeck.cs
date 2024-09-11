using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Mislint.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mislint.Core
{
    public class ReactionDeck
    {
        public class ReactedEventArgs : EventArgs {
            public string NoteId { get; set; }
            public string Emoji { get; set; }
        }

        public event EventHandler<ReactedEventArgs> Reacted;
        public static ReactionDeck Instance { get; } = new ReactionDeck();
        public string CurrentNoteId { get; set; }
        public List<string> ReactionDeckEmojisList { get; private set; }
        private StackPanel ReactionDeckPanel;
        private Dictionary<string, Expander> CategoriesExpander = new Dictionary<string, Expander>();
        private Flyout flyout { get; set; }
        //public async Task<List<string>> GetPinnedEmojis()
        //{
        //    this.ReactionDeckEmojisList ??= JsonSerializer.Deserialize<List<string>>((await Shared.MisharpApp.IApi.RegistryApi.GetDetail("pinnedEmojis", ["client", "base"])).Result.Value);
        //    return this.ReactionDeckEmojisList;
        //}

        public StackPanel GetReactionDeckPanel()
        {
            if (this.ReactionDeckPanel == null)
            {
                this.ReactionDeckPanel = new StackPanel() { 
                    Width = 230,
                };

                Shared.Emojis.ForEach((emoji) =>
                {
                    if (!this.CategoriesExpander.ContainsKey(emoji.Category ?? ""))
                    {
                        this.CategoriesExpander.Add(emoji.Category ?? "", new Expander()
                        {
                            Header = emoji.Category ?? "その他",
                            Content = new VariableSizedWrapGrid()
                            {
                                Orientation = Orientation.Horizontal,
                                Width = 230,
                                MaximumRowsOrColumns = 6,
                            },
                        });
                    }
                    var border = new Border()
                    {
                        Child = new Imager()
                        {
                            Url = $"http://{Shared.MisharpApp.Host}/emoji/{emoji.Name}@..webp",
                            Height = 25,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            MaxWidth = 25,
                            Margin = new Thickness(5),
                        },
                    };
                    border.Tapped += async (_, _) =>
                    {
                        if (this.CurrentNoteId != null)
                        {
                            //Debug.WriteLine($"Reacted to {this.CurrentNoteId} with {emoji.Name}");
                            this.Reacted?.Invoke(this, new ReactedEventArgs { NoteId = this.CurrentNoteId, Emoji = emoji.Name });
                            this.flyout.Hide();
                            this.CurrentNoteId = null;
                        }
                    };
                    (this.CategoriesExpander[emoji.Category ?? ""].Content as VariableSizedWrapGrid).Children.Add(border);
                });
                foreach (var expander in this.CategoriesExpander.Values)
                {
                    this.ReactionDeckPanel.Children.Add(expander);
                }
            }
            return this.ReactionDeckPanel;
        }

        public Flyout GetReactionDeckFlyout()
        {
            if (this.flyout == null)
            {
                Style s = new Style { 
                    TargetType = typeof(FlyoutPresenter)
                };
                s.Setters.Add(new Setter(FrameworkElement.MaxHeightProperty, "200"));
                this.flyout = new Flyout()
                {
                    Content = new StackPanel()
                    {
                        Children =
                        {
                            GetReactionDeckPanel()
                        },
                    },
                    Placement = FlyoutPlacementMode.Bottom,
                    FlyoutPresenterStyle = s,
                };
            }
            return this.flyout;
        }
    }
}
