using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using static Darkshot.Gdi32.Gdi32;

namespace Darkshot.Gdi32
{
    internal static class Gdi32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public ushort bmPlanes;
            public ushort bmBitsPixel;
            public IntPtr bmBits;
        }
        public struct SIZE
        {
            public int cx;
            public int cy;
        }
        public const int SRCCOPY = 0xCC0020;
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;


        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern IntPtr DeleteDC(IntPtr hDc);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        public static extern IntPtr DeleteObject(IntPtr hDc);

        [DllImport("gdi32.dll", EntryPoint = "BitBlt")]
        public static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, int RasterOp);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", EntryPoint = "GetDC")]
        public static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        public static extern int GetSystemMetrics(int abc);

        [DllImport("user32.dll", EntryPoint = "GetWindowDC")]
        public static extern IntPtr GetWindowDC(Int32 ptr);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("gdi32", CharSet = CharSet.Auto, EntryPoint = "GetObject")]
        public static extern int GetObjectBitmap(IntPtr hObject, int nCount, ref BITMAP lpObject);

        public static bool BitBlt(Graphics dest, int xDest, int yDest, int wDest, int hDest, Graphics src, int xSrc, int ySrc, CopyPixelOperation operation)
        {
            IntPtr srcDc = src.GetHdc();
            IntPtr dstDc = dest.GetHdc();
            var result = BitBlt(dstDc, xDest, yDest, wDest, hDest, srcDc, xSrc, ySrc, (int)operation);
            src.ReleaseHdc(srcDc);
            dest.ReleaseHdc(dstDc);
            return result;
        }
        public static bool BitBlt(Graphics dest, Point pDest, Size size, Graphics src, Point pSrc, CopyPixelOperation operation)
        {
            return BitBlt(dest, pDest.X, pDest.Y, size.Width, size.Height, src, pSrc.X, pSrc.Y, operation);
        }

        public static Bitmap CaptureControl(this Control control, Rectangle roi)
        {

            Bitmap bitmap;
            using (Graphics gScr = control.CreateGraphics())
            {
                bitmap = new Bitmap(Math.Abs(roi.Width), Math.Abs(roi.Height), gScr);
                using (Graphics gDest = Graphics.FromImage(bitmap))
                {
                    if (roi.Width < 0 || roi.Height < 0)
                    {
                        gDest.Clear(Color.Black);
                    }
                    else
                    {
                        IntPtr dc1Src = gScr.GetHdc();
                        IntPtr dcDest = gDest.GetHdc();
                        BitBlt(dcDest, 0, 0, roi.Width, roi.Height, dc1Src, roi.X, roi.Y, SRCCOPY);
                        gScr.ReleaseHdc(dc1Src);
                        gDest.ReleaseHdc(dcDest);
                    }
                }
            }

            return bitmap;
        }
        public static Bitmap GetDesktopImage()
        {
            IntPtr m_HBitmap;
            SIZE size;

            IntPtr hDC = GetDC(GetDesktopWindow());
            IntPtr hMemDC = CreateCompatibleDC(hDC);

            size.cx = GetSystemMetrics(SM_CXSCREEN);
            size.cy = GetSystemMetrics(SM_CYSCREEN);

            m_HBitmap = CreateCompatibleBitmap(hDC, size.cx, size.cy);

            if (m_HBitmap != IntPtr.Zero)
            {
                IntPtr hOld = (IntPtr)SelectObject(hMemDC, m_HBitmap);
                BitBlt(hMemDC, 0, 0, size.cx, size.cy, hDC, 0, 0, SRCCOPY);
                SelectObject(hMemDC, hOld);
                DeleteDC(hMemDC);
                ReleaseDC(GetDesktopWindow(), hDC);
                return Image.FromHbitmap(m_HBitmap);
            }
            return null;
        }

        public static void PasteBitmap(this Graphics graphic, Bitmap bitmap, Point dest, CopyPixelOperation operation)
        {
            IntPtr targetDC = graphic.GetHdc();
            IntPtr sourceDC = CreateCompatibleDC(targetDC);
            IntPtr sourceBitmap = bitmap.GetHbitmap();
            IntPtr originalBitmap = SelectObject(sourceDC, sourceBitmap);
            BitBlt(targetDC, dest.X, dest.Y, bitmap.Width, bitmap.Height, sourceDC, 0, 0, (int)operation);
            SelectObject(sourceDC, originalBitmap);
            DeleteObject(sourceBitmap);
            DeleteDC(sourceDC);
            graphic.ReleaseHdc(targetDC);
        }

        public static Bitmap ToBitmap(this Graphics source, Rectangle roi)
        {
            var bitmap = new Bitmap(roi.Width, roi.Height);
            using (var target = Graphics.FromImage(bitmap))
                BitBlt(target, Point.Empty, bitmap.Size, source, roi.Location, CopyPixelOperation.SourceCopy);
            return bitmap;
        }
    }
}
