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
        public class ReactedEventArgs : EventArgs
        {
            public string NoteId { get; set; }
            public string Emoji { get; set; }
        }

        public event EventHandler<ReactedEventArgs> Reacted;
        public static ReactionDeck Instance { get; } = new ReactionDeck();
        public string CurrentNoteId { get; set; }
        public List<string> ReactionDeckEmojisList { get; private set; }
        private StackPanel _reactionDeckPanel;
        private readonly Dictionary<string, Expander> _categoriesExpander = new Dictionary<string, Expander>();
        private Flyout Flyout { get; set; }

        private StackPanel GetReactionDeckPanel()
        {
            if (this._reactionDeckPanel != null) return this._reactionDeckPanel;
            this._reactionDeckPanel = new StackPanel()
            {
                Width = 230,
            };

            Shared.Emojis.ForEach(emoji =>
            {
                if (!this._categoriesExpander.ContainsKey(emoji.Category ?? ""))
                {
                    this._categoriesExpander.Add(emoji.Category ?? "", new Expander()
                    {
                        Header = emoji.Category ?? "その他",
                        Content = new VariableSizedWrapGrid()
                        {
                            Orientation = Orientation.Horizontal,
                            Width = 230,
                            MaximumRowsOrColumns = 5,
                        },
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    });
                }
                var border = new Border()
                {
                    Child = new Imager()
                    {
                        Url = $"https://{Shared.MisharpApp.Host}/emoji/{emoji.Name}@..webp",
                        Height = 25,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        MaxWidth = 25,
                        Margin = new Thickness(5),
                    },
                };
                border.Tapped += (_, _) =>
                {
                    if (this.CurrentNoteId == null) return;
                    this.Reacted?.Invoke(this, new ReactedEventArgs { NoteId = this.CurrentNoteId, Emoji = emoji.Name });
                    this.Flyout.Hide();
                    this.CurrentNoteId = null;
                };
                (this._categoriesExpander[emoji.Category ?? ""].Content as VariableSizedWrapGrid)?.Children.Add(border);
            });
            foreach (var expander in this._categoriesExpander.Values)
            {
                this._reactionDeckPanel.Children.Add(expander);
            }
            return this._reactionDeckPanel;
        }

        public Flyout GetReactionDeckFlyout()
        {
            if (this.Flyout != null) return this.Flyout;
            var s = new Style
            {
                TargetType = typeof(FlyoutPresenter)
            };
            s.Setters.Add(new Setter(FrameworkElement.MaxHeightProperty, "200"));
            this.Flyout = new Flyout()
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
            return this.Flyout;
        }
    }
}
