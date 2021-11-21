using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Darkshot.Gdi32
{
    internal static class Gdi32
    {
        [DllImportAttribute("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest,
                                        int nWidth, int nHeight, IntPtr hdcSrc,
                                        int nXSrc, int nYSrc, uint dwRop);
        public static Bitmap CaptureControl(this Control control, Rectangle roi)
        {
            const uint SRCCOPY = 0xCC0020;

            Bitmap bitmap;
            using (Graphics gScr = control.CreateGraphics())
            {
                bitmap = new Bitmap(roi.Width, roi.Height, gScr);
                using (Graphics gDest = Graphics.FromImage(bitmap))
                {
                    IntPtr dc1Src = gScr.GetHdc();
                    IntPtr dcDest = gDest.GetHdc();
                    BitBlt(dcDest, 0, 0, roi.Width, roi.Height, dc1Src, roi.X, roi.Y, SRCCOPY);
                    gScr.ReleaseHdc(dc1Src);
                    gDest.ReleaseHdc(dcDest);
                }
            }

            return bitmap;
        }
    }
}
