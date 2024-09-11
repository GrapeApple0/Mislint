using System.Runtime.InteropServices;

namespace libwebp_wrapper
{
    public partial class NativeMethods
    {
        public const int WEBP_DEMUX_ABI_VERSION = 0x0107;
        public const int WEBP_DECODER_ABI_VERSION = 0x0209;

        [StructLayout(LayoutKind.Sequential)]
        public struct DecodedFrame
        {
            public IntPtr rgba;         // Decoded and reconstructed full frame.
            public int duration;          // Frame duration in milliseconds.
            public int is_key_frame;      // True if this frame is a key-frame.
        }

        public enum AnimatedFileFormat
        {
            ANIM_GIF,
            ANIM_WEBP
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

        [LibraryImport(@"lib-cpp.dll", StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr CWebPAnimDecoderNewInternal(ref WebPData webp_data, IntPtr dec_options, int abi_version);

        [LibraryImport(@"lib-cpp.dll", StringMarshalling = StringMarshalling.Utf8)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool CWebPAnimDecoderGetInfo(IntPtr dec, ref WebPAnimInfo info);

        [LibraryImport(@"lib-cpp.dll", StringMarshalling = StringMarshalling.Utf8)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool CWebPAnimDecoderHasMoreFrames(IntPtr dec);

        [LibraryImport(@"lib-cpp.dll", StringMarshalling = StringMarshalling.Utf8)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool CWebPAnimDecoderGetNext(IntPtr dec, ref IntPtr buf_ptr, ref int timestamp_ptr);
    }

    public static class WebP
    {
        public static bool AllocateFrames(ref NativeMethods.AnimatedImage image, uint num_frames)
        {
            ulong rgba_size = (ulong)image.CanvasWidth * image.CanvasHeight * 4;
            ulong total_size = num_frames * rgba_size;
            ulong total_frames_size = num_frames * 16;
            if (total_size > int.MaxValue || total_frames_size > int.MaxValue)
            {
                return false;
            }
            image.NumFrames = num_frames;
            var decodedFrames = new NativeMethods.DecodedFrame[total_frames_size];
            var mem = Marshal.AllocHGlobal((int)total_size);
            for (uint i = 0; i < num_frames; i++)
            {
                decodedFrames[i].rgba = mem/* + (IntPtr)(i * rgba_size)*/;
                decodedFrames[i].duration = 0;
                decodedFrames[i].is_key_frame = 0;
            }
            image.Frames = decodedFrames;
            image.RawMem = mem;
            return true;
        }

        public static void CleanupTransparentPixels(IntPtr rgba, uint width, uint height)
        {
            uint rgba_end = (uint)rgba + width * height;
            while ((uint)rgba < rgba_end)
            {
                char alpha = (char)(((uint)rgba >> 24) & 0xff);
                if (alpha != 0)
                {
                    rgba = IntPtr.Zero;
                }
                rgba += 1;
            }
        }

        public static byte[] FromBGRABytes(byte[] bgra, int width)
        {
            var bmpHeader = new byte[] // All values are little-endian
            {
                0x42, 0x4D,             // Signature 'BM'
                0x00, 0x00, 0x00, 0x00, // Size
                0x00, 0x00,             // Unused
                0x00, 0x00,             // Unused
                0x8a, 0x00, 0x00, 0x00, // Offset to image data

                0x7c, 0x00, 0x00, 0x00, // DIB header size (124 bytes)
                0x00, 0x00, 0x00, 0x00, // Width
                0x00, 0x00, 0x00, 0x00, // Height
                0x01, 0x00,             // Planes (1)
                0x20, 0x00,             // Bits per pixel (32)
                0x03, 0x00, 0x00, 0x00, // Format (bitfield = use bitfields | no compression)
                0x00, 0x00, 0x00, 0x00, // Image raw size (32 bytes)
                0x13, 0x0B, 0x00, 0x00, // Horizontal print resolution (2835 = 72dpi * 39.3701)
                0x13, 0x0B, 0x00, 0x00, // Vertical print resolution (2835 = 72dpi * 39.3701)
                0x00, 0x00, 0x00, 0x00, // Colors in palette (none)
                0x00, 0x00, 0x00, 0x00, // Important colors (0 = all)
                0xFF, 0x00, 0x00, 0x00, // R bitmask (00FF0000)
                0x00, 0xFF, 0x00, 0x00, // G bitmask (0000FF00)
                0x00, 0x00, 0xFF, 0x00, // B bitmask (000000FF)
                0x00, 0x00, 0x00, 0xFF, // A bitmask (FF000000)
                0x42, 0x47, 0x52, 0x73, // sRGB color space
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Unused R, G, B entries for color space
                0x00, 0x00, 0x00, 0x00, // Unused Gamma X entry for color space
                0x00, 0x00, 0x00, 0x00, // Unused Gamma Y entry for color space
                0x00, 0x00, 0x00, 0x00, // Unused Gamma Z entry for color space

                0x00, 0x00, 0x00, 0x00, // Unknown
                0x00, 0x00, 0x00, 0x00, // Unknown
                0x00, 0x00, 0x00, 0x00, // Unknown
                0x00, 0x00, 0x00, 0x00, // Unknown
            };
            int height = (bgra.Length / 4) / width;
            var bytes = new byte[bgra.Length + bmpHeader.Length];
            //write header
            Buffer.BlockCopy(bmpHeader, 0, bytes, 0, bmpHeader.Length);
            //write filesize to offset 2
            Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, bytes, 2, 4);
            //write width to offset 18
            Buffer.BlockCopy(BitConverter.GetBytes(width), 0, bytes, 18, 4);
            //write height to offset 22
            Buffer.BlockCopy(BitConverter.GetBytes(height), 0, bytes, 22, 4);
            //write image size to offset 34
            Buffer.BlockCopy(BitConverter.GetBytes(width * height * 4), 0, bytes, 34, 4);
            //finally, write the image data
            Buffer.BlockCopy(bgra, 0, bytes, bmpHeader.Length, bgra.Length);
            return bytes;
        }

        public struct BitmapFrame
        {
            public byte[] Data;
            public int Duration;
        }

        public static List<BitmapFrame>? WebPLoad(byte[] bytes)
        {
            var animatedImage = new NativeMethods.AnimatedImage();
            var webPData = new NativeMethods.WebPData();
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            webPData.bytes = handle.AddrOfPinnedObject();
            webPData.size = bytes.Length;
            var frame_index = 0;
            var prev_frame_timestamp = 0;
            var anim_info = new NativeMethods.WebPAnimInfo();
            IntPtr dec = NativeMethods.CWebPAnimDecoderNewInternal(ref webPData, IntPtr.Zero, NativeMethods.WEBP_DEMUX_ABI_VERSION);
            if (!NativeMethods.CWebPAnimDecoderGetInfo(dec, ref anim_info))
            {
                return null;
            }
            animatedImage.CanvasWidth = anim_info.canvas_width;
            animatedImage.CanvasHeight = anim_info.canvas_height;
            animatedImage.LoopCount = anim_info.loop_count;
            animatedImage.BgColor = anim_info.bgcolor;
            if (!AllocateFrames(ref animatedImage, anim_info.frame_count)) return null;
            var frames = new List<BitmapFrame>();
            while (NativeMethods.CWebPAnimDecoderHasMoreFrames(dec))
            {
                IntPtr frame_rgba = IntPtr.Zero;
                int timestamp = 0;
                if (!NativeMethods.CWebPAnimDecoderGetNext(dec, ref frame_rgba, ref timestamp))
                {
                    break;
                }
                int duration = timestamp - prev_frame_timestamp;
                CleanupTransparentPixels(frame_rgba, animatedImage.CanvasWidth, animatedImage.CanvasHeight);
                byte[] rgbaByte = new byte[(int)(animatedImage.CanvasWidth * 4 * animatedImage.CanvasHeight)];
                Marshal.Copy(frame_rgba, rgbaByte, 0, (int)(animatedImage.CanvasWidth * 4 * animatedImage.CanvasHeight));
                var bitmap = new byte[animatedImage.CanvasWidth * animatedImage.CanvasHeight * 4];
                for (int y = 0; y < animatedImage.CanvasHeight; y++)
                {
                    for (int x = 0; x < animatedImage.CanvasWidth; x++)
                    {
                        int offset = y * (int)animatedImage.CanvasWidth * 4 + x * 4;
                        var r = rgbaByte[offset];
                        var g = rgbaByte[offset + 1];
                        var b = rgbaByte[offset + 2];
                        var a = rgbaByte[offset + 3];
                        bitmap[offset] = b;
                        bitmap[offset + 1] = g;
                        bitmap[offset + 2] = r;
                        bitmap[offset + 3] = a;
                    }
                }
                frames.Add(new BitmapFrame()
                {
                    Data = bitmap,
                    Duration = duration,
                });
                File.WriteAllBytes($"frame{frame_index}.bmp", FromBGRABytes(bitmap, (int)animatedImage.CanvasWidth));
                frame_index++;
                prev_frame_timestamp = timestamp;
            }
            return frames;
        }
    }
}
