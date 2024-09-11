using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static Mislint.Core.NativeMethods;

namespace Mislint.Core
{
    public static class WebP
    {
        public static bool AllocateFrames(ref AnimatedImage image, uint num_frames)
        {
            ulong rgba_size = (ulong)image.CanvasWidth * image.CanvasHeight * 4;
            ulong total_size = num_frames * rgba_size;
            ulong total_frames_size = num_frames * 16;
            if (total_size > int.MaxValue || total_frames_size > int.MaxValue)
            {
                return false;
            }
            image.NumFrames = num_frames;
            var decodedFrames = new DecodedFrame[total_frames_size];
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

        public static byte[] FromBGRABytes(byte[] bgra, int width)
        {
            var bitmap = new byte[bgra.Length];
            var height = bgra.Length / 4 / width;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * width * 4 + x * 4;
                    int rev = (height * width * 4) - (y * width * 4 + (width - x - 1) * 4) - 1;
                    bitmap[rev] = bgra[offset + 3];
                    bitmap[rev - 1] = bgra[offset + 2];
                    bitmap[rev - 2] = bgra[offset + 1];
                    bitmap[rev - 3] = bgra[offset];
                }
            }
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
            var bytes = new byte[bitmap.Length + bmpHeader.Length];
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
            Buffer.BlockCopy(bitmap, 0, bytes, bmpHeader.Length, bitmap.Length);
            return bytes;
        }

        public struct Image
        {
            public int width;
            public int height;
            public uint loop;
            public List<WebPFrame> frames;
        }

        public struct WebPFrame
        {
            public byte[] Data;
            public int Duration;
        }

        public static void WebPLoad(byte[] bytes, out Image? image)
        {
            image = null;
            var animatedImage = new AnimatedImage();
            var webPData = new WebPData();
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            webPData.bytes = handle.AddrOfPinnedObject();
            webPData.size = bytes.Length;
            var frame_index = 0;
            var prev_frame_timestamp = 0;
            var anim_info = new WebPAnimInfo();
            IntPtr dec = WebPAnimDecoderNewInternal(ref webPData, IntPtr.Zero, WEBP_DEMUX_ABI_VERSION);
            if (!WebPAnimDecoderGetInfo(dec, ref anim_info)) goto End;
            animatedImage.CanvasWidth = anim_info.canvas_width;
            animatedImage.CanvasHeight = anim_info.canvas_height;
            animatedImage.LoopCount = anim_info.loop_count;
            animatedImage.BgColor = anim_info.bgcolor;
            if (!AllocateFrames(ref animatedImage, anim_info.frame_count)) goto End;
            var frames = new List<WebPFrame>();
            int width = (int)animatedImage.CanvasWidth;
            int height = (int)animatedImage.CanvasHeight;
            while (WebPAnimDecoderHasMoreFrames(dec))
            {
                IntPtr frame_rgba = IntPtr.Zero;
                int timestamp = 0;
                if (!WebPAnimDecoderGetNext(dec, ref frame_rgba, ref timestamp))
                {
                    break;
                }
                int duration = timestamp - prev_frame_timestamp;
                CleanupTransparentPixels(frame_rgba, width, height);
                byte[] rgbaByte = new byte[width * 4 * height];
                Marshal.Copy(frame_rgba, rgbaByte, 0, width * 4 * height);
                var bitmap = new byte[animatedImage.CanvasWidth * animatedImage.CanvasHeight * 4];
                for (int y = 0; y < animatedImage.CanvasHeight; y++)
                {
                    for (int x = 0; x < animatedImage.CanvasWidth; x++)
                    {
                        int offset = y * width * 4 + x * 4;
                        bitmap[offset + 2] = rgbaByte[offset + 0];
                        bitmap[offset + 1] = rgbaByte[offset + 1];
                        bitmap[offset + 0] = rgbaByte[offset + 2];
                        bitmap[offset + 3] = rgbaByte[offset + 3];
                    }
                }
                frames.Add(new WebPFrame()
                {
                    Data = bitmap,
                    Duration = duration,
                });
                // File.WriteAllBytes($"{DateTime.Now.Second}{frame_index + 1}.bmp", FromBGRABytes(rgbaByte, (int)animatedImage.CanvasWidth));
                frame_index++;
                prev_frame_timestamp = timestamp;
            }
            image = new Image()
            {
                width = width,
                height = height,
                frames = frames,
                loop = animatedImage.LoopCount,
            };
        End:
            WebPAnimDecoderDelete(dec);
            return;
        }
    }

}
