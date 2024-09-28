using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Mislint.Components;

namespace Mislint.Pages
{
    public sealed partial class Timeline : Page
    {
        private int previousSelectedIndex = 0;
        private Components.Timeline timelineComponent;

        public Timeline()
        {
            this.InitializeComponent();
        }

        private void Timeline_Loaded(object sender, RoutedEventArgs e)
        {
            //var postform = new PostForm();
            var header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star),
            });
            header.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = GridLength.Auto,
            });
            var selectorBar = new SelectorBar();
            selectorBar.Items.Add(new SelectorBarItem
            {
                Text = "Home",
                IsSelected = true,
            });
            selectorBar.Items.Add(new SelectorBarItem
            {
                Text = "Local",
            });
            selectorBar.Items.Add(new SelectorBarItem
            {
                Text = "Hybrid",
            });
            selectorBar.Items.Add(new SelectorBarItem
            {
                Text = "Global",
            });
            var reloadButton = new Button
            {
                Content = new FontIcon()
                {
                    Glyph = "\uE72C",
                },
            };
            reloadButton.Click += (_, _) =>
            {
                if (this.Root.Children[^1] is Components.Timeline tl) tl.Reload();
            };
            Grid.SetColumn(selectorBar, 0);
            Grid.SetColumn(reloadButton, 1);
            header.Children.Add(selectorBar);
            header.Children.Add(reloadButton);
            this.timelineComponent = new Components.Timeline
            {
                TimelineType = Components.Timeline.TimelineTypeEnum.Home
            };
            //Grid.SetRow(postform, 0);
            Grid.SetRow(header, 1);
            Grid.SetRow(this.timelineComponent, 2);
            //this.root.Children.Add(postform);
            this.Root.Children.Add(header);
            this.Root.Children.Add(this.timelineComponent);
            this.timelineComponent.Loaded += (_, _) =>
            {
                selectorBar.SelectionChanged += this.SelectorBar_SelectionChanged;
            };
        }

        private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            var selectedItem = sender.SelectedItem;
            int currentSelectedIndex = sender.Items.IndexOf(selectedItem);
            this.timelineComponent.TimelineType = (Components.Timeline.TimelineTypeEnum)currentSelectedIndex;
            this.timelineComponent.Reload();
            var slideNavigationTransitionEffect = currentSelectedIndex - previousSelectedIndex > 0 ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft;
            previousSelectedIndex = currentSelectedIndex;
        }
    }
}
