using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Mislint.Components;
using Mislint.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using WinRT.Interop;

namespace Mislint
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class MainWindow : Window
    {
        private AppWindow _appWindow;
        private static NativeMethods.WinProc newWndProc = null;
        private static IntPtr oldWndProc = IntPtr.Zero;

        public static int MinWindowWidth { get; set; } = 360;
        public static int MinWindowHeight { get; set; } = 480;
        //public static int MaxWindowWidth { get; set; } = 1800;
        //public static int MaxWindowHeight { get; set; } = 1600;

        private static void RegisterWindowMinMax(Window window)
        {
            var hwnd = GetWindowHandleForCurrentWindow(window);

            newWndProc = new NativeMethods.WinProc(WndProc);
            oldWndProc = NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.WindowLongIndexFlags.GWL_WNDPROC, newWndProc);
        }

        private static IntPtr GetWindowHandleForCurrentWindow(object target) => WindowNative.GetWindowHandle(target);

        private static IntPtr WndProc(IntPtr hWnd, NativeMethods.WindowMessage Msg, IntPtr wParam, IntPtr lParam)
        {
            switch (Msg)
            {
                case NativeMethods.WindowMessage.WM_GETMINMAXINFO:
                    var dpi = NativeMethods.GetDpiForWindow(hWnd);
                    var scalingFactor = (float)dpi / 96;

                    var minMaxInfo = Marshal.PtrToStructure<NativeMethods.MINMAXINFO>(lParam);
                    minMaxInfo.ptMinTrackSize.x = (int)(MinWindowWidth * scalingFactor);
                    minMaxInfo.ptMinTrackSize.y = (int)(MinWindowHeight * scalingFactor);
                    //minMaxInfo.ptMaxTrackSize.x = (int)(MaxWindowWidth * scalingFactor);
                    //minMaxInfo.ptMaxTrackSize.y = (int)(MaxWindowHeight * scalingFactor);
                    Marshal.StructureToPtr(minMaxInfo, lParam, true);
                    break;

            }
            return NativeMethods.CallWindowProc(oldWndProc, hWnd, Msg, wParam, lParam);
        }

        public MainWindow()
        {
            this.InitializeComponent();
            _appWindow = GetAppWindowForCurrentWindow();
            _appWindow.Closing += OnClosing;
            RegisterWindowMinMax(this);
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs e)
        {
            this.contentFrame.Navigate(typeof(Pages.Timeline));
            this.Activated -= Window_Activated;
        }

        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(myWndId);
        }

        private void OnClosing(object sender, AppWindowClosingEventArgs e)
        {
            if (GlobalLock.Instance.Lock)
            {
                e.Cancel = true;
                GlobalLock.Instance.Unlocked += (_, _) =>
                {
                    this.Close();
                };
            }
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
            if (args.SelectedItem is NavigationViewItem viewItem)
            {
                if (viewItem.Tag.ToString() == "Timeline")
                {
                    this.contentFrame.Navigate(typeof(Pages.Timeline));
                }
                else if (viewItem.Tag.ToString() == "Profile")
                {
                    this.contentFrame.Navigate(typeof(Pages.UserInfo), new Dictionary<string, string>()
                    {
                        {
                            "UserId", Shared.I.Id
                        },
                    });
                }
            }
        }
    }
}
