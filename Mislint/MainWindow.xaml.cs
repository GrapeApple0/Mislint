using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Mislint.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using WinRT.Interop;
using WindowActivatedEventArgs = Microsoft.UI.Xaml.WindowActivatedEventArgs;

namespace Mislint
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class MainWindow : Window
    {
        private readonly AppWindow _appWindow;
        private NativeMethods.WinProc _newWndProc;
        private IntPtr _oldWndProc = IntPtr.Zero;
        private readonly IntPtr _hWnd;
        public static int MinWindowWidth { get; set; } = 360;
        public static int MinWindowHeight { get; set; } = 480;
        public static int MaxWindowWidth { get; set; } = 0;
        public static int MaxWindowHeight { get; set; } = 0;

        private void RegisterWindowMinMax()
        {
            this._newWndProc = this.WndProc;
            this._oldWndProc = NativeMethods.SetWindowLongPtr(this._hWnd, NativeMethods.WindowLongIndexFlags.GWL_WNDPROC, _newWndProc);
        }

        private IntPtr WndProc(IntPtr hWnd, NativeMethods.WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == NativeMethods.WindowMessage.WM_GETMINMAXINFO)
            {
                var dpi = NativeMethods.GetDpiForWindow(hWnd);
                var scalingFactor = (float)dpi / 96;

                var minMaxInfo = Marshal.PtrToStructure<NativeMethods.MINMAXINFO>(lParam);
                if (0 < MinWindowWidth)
                    minMaxInfo.ptMinTrackSize.x = (int)(MinWindowWidth * scalingFactor);
                if (0 < MinWindowHeight)
                    minMaxInfo.ptMinTrackSize.y = (int)(MinWindowHeight * scalingFactor);
                if (0 < MaxWindowWidth)
                    minMaxInfo.ptMaxTrackSize.x = (int)(MaxWindowWidth * scalingFactor);
                if (0 < MaxWindowHeight)
                    minMaxInfo.ptMaxTrackSize.y = (int)(MaxWindowHeight * scalingFactor);
                Marshal.StructureToPtr(minMaxInfo, lParam, true);
            }
            return NativeMethods.CallWindowProc(this._oldWndProc, hWnd, msg, wParam, lParam);
        }

        private AppWindow GetAppWindowForCurrentWindow()
        {
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(this._hWnd);
            return AppWindow.GetFromWindowId(myWndId);
        }

        private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null, TypedEventHandler<KeyboardAccelerator, KeyboardAcceleratorInvokedEventArgs>? eventHandler = null)
        {
            var keyboardAccelerator = new KeyboardAccelerator { Key = key };
            if (modifiers.HasValue)
            {
                keyboardAccelerator.Modifiers = modifiers.Value;
            }
            if (eventHandler != null)
                keyboardAccelerator.Invoked += eventHandler;
            return keyboardAccelerator;
        }

        private readonly KeyboardAccelerator _altLeftKeyboardAccelerator;
        private readonly KeyboardAccelerator _altRightKeyboardAccelerator;

        public MainWindow()
        {
            this.InitializeComponent();
            this._hWnd = WindowNative.GetWindowHandle(this);
            _appWindow = GetAppWindowForCurrentWindow();
            _appWindow.Closing += OnClosing;
            RegisterWindowMinMax();
            this.Content.PointerPressed += (sender, e) =>
            {
                if (e.GetCurrentPoint((UIElement)sender).Properties.IsXButton1Pressed)
                {
                    e.Handled = !TryGoBack();
                }
                else if (e.GetCurrentPoint((UIElement)sender).Properties.IsXButton2Pressed)
                {
                    e.Handled = !TryGoForward();
                }
            };
            this.Content.KeyDown += (sender, e) =>
            {
                if (e.Key.HasFlag(VirtualKey.Control))
                {
                    Debug.WriteLine("ctrl+left down");
                }
            };
            //this._altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Control,
            //    (sender, args) =>
            //    {
            //        args.Handled = this.TryGoBack();
            //    });
            //this._altRightKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Right, VirtualKeyModifiers.Control,
            //    (sender, args) =>
            //    {
            //        args.Handled = this.TryGoForward();
            //    });
            //this.Content.KeyboardAccelerators.Add(this._altLeftKeyboardAccelerator);
            //this.Content.KeyboardAccelerators.Add(this._altRightKeyboardAccelerator);
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (Settings.Instance.Setting.Host != null && Settings.Instance.Setting.Token != null)
            {
                this.ContentFrame.Navigate(typeof(Pages.Timeline));
            }
            else
            {
                this.ContentFrame.Navigate(typeof(Pages.Settings));
            }
            this.Activated -= Window_Activated;
        }

        private void OnClosing(object sender, AppWindowClosingEventArgs e)
        {
            if (!GlobalLock.Instance.Lock) return;
            e.Cancel = true;
            GlobalLock.Instance.Unlocked += (_, _) =>
            {
                this.Close();
            };
        }

        public void ShowPopup(Image child)
        {
            this.OverlayFlyout.Content = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Children =
                {
                    child
                },
                MaxWidth = this.Root.ActualWidth - 25,
                MaxHeight = this.Root.ActualHeight / 1.5,
            };
            this.OverlayFlyout.ShowAt(this.Content, new FlyoutShowOptions
            {
                Position = new Point(this.Content.ActualSize.X / 1.5, this.ContentFrame.ActualHeight),
            });
        }

        public async void ShowDialog(string text)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = text,
                CloseButtonText = "Ok",
                XamlRoot = this.Content.XamlRoot
            };
            var result = (await dialog.ShowAsync());
        }

        public void SwitchPage(Type sourcePageType, Dictionary<string, string> parameter)
        {
            this.ContentFrame.Navigate(sourcePageType, parameter);
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            FrameNavigationOptions navOptions = new FrameNavigationOptions
            {
                TransitionInfoOverride = args.RecommendedNavigationTransitionInfo
            };
            if (sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
            {
                navOptions.IsNavigationStackEnabled = false;
            }
            if (args.SelectedItem is not NavigationViewItem viewItem) return;
            var page = viewItem.Tag.ToString();

            switch (page)
            {
                case "Timeline":
                    this.ContentFrame.Navigate(typeof(Pages.Timeline));
                    break;
                case "Notification":
                    throw new Exception("test");
                case "Profile":
                    this.ContentFrame.Navigate(typeof(Pages.UserInfo), new Dictionary<string, string>
                    {
                        {
                            "UserId", Shared.I.Id
                        },
                    });
                    break;
                case "Post":
                    this.ContentFrame.Navigate(typeof(Pages.PostForm));
                    break;
                case "Settings":
                    this.ContentFrame.Navigate(typeof(Pages.Settings));
                    break;
            }
        }

        private bool TryGoBack()
        {
            if (!this.ContentFrame.CanGoBack) return false;
            this.ContentFrame.GoBack();
            return true;
        }

        private bool TryGoForward()
        {
            if (!this.ContentFrame.CanGoForward) return false;
            this.ContentFrame.GoForward();
            return true;
        }

        private void NavigationView_OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            TryGoBack();
        }
    }
}
