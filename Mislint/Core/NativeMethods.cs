using System;
using System.Runtime.InteropServices;

namespace Mislint.Core
{
    public abstract partial class NativeMethods
    {
        public const int WebpDemuxAbiVersion = 0x0107;
        public const int WebpDecoderAbiVersion = 0x0209;

        [StructLayout(LayoutKind.Sequential)]
        public struct DecodedFrame
        {
            public IntPtr rgba;         // Decoded and reconstructed full frame.
            public int duration;          // Frame duration in milliseconds.
            public int is_key_frame;      // True if this frame is a key-frame.
        }

        public enum AnimatedFileFormat
        {
            AnimGif,
            AnimWebp
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AnimatedImage
        {
            public AnimatedFileFormat Format { get; set; }
            public uint CanvasWidth { get; set; }
            public uint CanvasHeight { get; set; }
            public uint BgColor { get; set; }
            public uint LoopCount { get; set; }
            public DecodedFrame[] Frames { get; set; }
            public uint NumFrames { get; set; }
            public IntPtr RawMem { get; set; }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WebPData
        {
            public IntPtr bytes;
            public int size;
        };

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct WebPAnimInfo
        {
            public uint canvas_width;
            public uint canvas_height;
            public uint loop_count;
            public uint bgcolor;
            public uint frame_count;
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U4)]
            public fixed uint pad[4];   // padding for later use
        };

        [LibraryImport(@"cpp-lib.dll", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "CallWebPAnimDecoderNewInternal")]
        public static partial IntPtr WebPAnimDecoderNewInternal(ref WebPData webpData, IntPtr decOptions, int abiVersion);

        [LibraryImport(@"cpp-lib.dll", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "CallWebPAnimDecoderGetInfo")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool WebPAnimDecoderGetInfo(IntPtr dec, ref WebPAnimInfo info);

        [LibraryImport(@"cpp-lib.dll", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "CallWebPAnimDecoderHasMoreFrames")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool WebPAnimDecoderHasMoreFrames(IntPtr dec);

        [LibraryImport(@"cpp-lib.dll", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "CallWebPAnimDecoderGetNext")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool WebPAnimDecoderGetNext(IntPtr dec, ref IntPtr bufPtr, ref int timestampPtr);

        [LibraryImport(@"cpp-lib.dll", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "CallWebPAnimDecoderDelete")]
        public static partial void WebPAnimDecoderDelete(IntPtr dec);

        [LibraryImport(@"cpp-lib.dll", StringMarshalling = StringMarshalling.Utf8)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool CleanupTransparentPixels(IntPtr rgba, int width, int height);

        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [Flags]
        public enum WindowLongIndexFlags : int
        {
            GWL_WNDPROC = -4,
        }

        public enum WindowMessage : int
        {
            WM_GETMINMAXINFO = 0x0024,
        }

        public delegate IntPtr WinProc(IntPtr hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll")]
        public static extern int GetDpiForWindow(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, WindowLongIndexFlags nIndex, WinProc newProc);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, WindowLongIndexFlags nIndex, WinProc newProc);

        [DllImport("user32.dll")]
        public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam);
        
        public static IntPtr SetWindowLongPtr(IntPtr hWnd, WindowLongIndexFlags nIndex, WinProc newProc)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, newProc);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, newProc));
        }
    }
}