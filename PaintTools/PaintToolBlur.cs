using Darkshot.Gdi32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Darkshot.PaintTools
{
    class PaintToolBlur : PaintTool
    {
        const int RADIUS = 20;
        Rectangle _rect;
        Point _point;
        bool _drawing;
        Bitmap _bitmap;

        public PaintToolBlur()
        {
            Paint += (s, e) => { onPaint(e.Graphics); };
            MouseDown += (s, e) => { onMouseDown(e); };
            MouseUp += (s, e) => { onMouseUp(s as Control, e); };
            MouseMove += (s, e) => { onMouseMove(e); };

            _rect = Rectangle.Empty;
            _drawing = false;
        }

        void onPaint(Graphics g)
        {
            if (_bitmap != null)
            {
                g.DrawImageUnscaled(_bitmap, _rect.Location);
                return;
            }

            var rectOut = new Rectangle(_rect.X - RADIUS, _rect.Y - RADIUS, _rect.Width + 2 * RADIUS, _rect.Height + 2 * RADIUS);
            var rectIn = new Rectangle(RADIUS, RADIUS, _rect.Width, _rect.Height);
            if (rectOut.X < 0)
            {
                rectIn.X += rectOut.X;
                rectOut.Width += rectOut.X;
                rectOut.X = 0;
            }
            if (rectOut.Y < 0)
            {
                rectIn.Y += rectOut.Y;
                rectOut.Height += rectOut.Y;
                rectOut.Y = 0;
            }
            if (rectOut.X + rectOut.Width > NativeVirtualScreen.Bounds.Width)
                rectOut.Width = NativeVirtualScreen.Bounds.Width - rectOut.X;
            if (rectOut.Y + rectOut.Height > NativeVirtualScreen.Bounds.Height)
                rectOut.Height = NativeVirtualScreen.Bounds.Height - rectOut.Y;

            if (Math.Min(_rect.Width, _rect.Height) < 3)
                return;

            using (var bitmap = g.ToBitmap(rectOut))
            {
                using (var blurred = Gaussian(bitmap, RADIUS))
                {
                    using (var croppedBlurred = blurred.Clone(rectIn, bitmap.PixelFormat))
                    {
                        if (!_drawing)
                            _bitmap = croppedBlurred.Clone() as Bitmap;
                        g.DrawImageUnscaled(croppedBlurred, _rect.Location);
                    }
                }
            }
        }

        void onMouseDown(MouseEventArgs e)
        {
            _drawing = true;
            _point = e.Location;
            _rect.Location = e.Location;
        }

        void onMouseMove(MouseEventArgs e)
        {
            if (!_drawing)
                return;
            _rect.X = Math.Min(_point.X, e.Location.X);
            _rect.Y = Math.Min(_point.Y, e.Location.Y);
            _rect.Width = Math.Abs(_point.X - e.Location.X);
            _rect.Height = Math.Abs(_point.Y - e.Location.Y);
        }

        void onMouseUp(Control canvas, MouseEventArgs e)
        {
            if (!_drawing)
                return;
            _rect.X = Math.Min(_point.X, e.Location.X);
            _rect.Y = Math.Min(_point.Y, e.Location.Y);
            _rect.Width = Math.Abs(_point.X - e.Location.X);
            _rect.Height = Math.Abs(_point.Y - e.Location.Y);
            _drawing = false;
            RaiseComplete(canvas);
        }

        public override Rectangle GetBounds()
        {
            int radius = 10 * RADIUS;
            var rect = _rect;
            rect.X -= radius;
            rect.Y -= radius;
            rect.Width += 2 * radius;
            rect.Height += 2 * radius;
            return rect;
        }

        Bitmap Gaussian(Bitmap bitmap, float sigma)
        {
            if (bitmap.Width < 3 || bitmap.Height < 3 || sigma < 0.25f)
                return bitmap;

            int w = bitmap.Width;
            int h = bitmap.Height;
            var rect = new Rectangle(Point.Empty, bitmap.Size);
            var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var stride = bitmapData.Stride;
            var ps = stride / w;

            var bytes = new byte[stride * h];
            Marshal.Copy(bitmapData.Scan0, bytes, 0, bytes.Length);
            bitmap.UnlockBits(bitmapData);

            var m = new float[w * h];
            var r = new float[w * h];
            var q = (float)((sigma <= 2.5)
                             ? (3.97156 - 4.14554 * Math.Sqrt(1.0 - 0.26891 * Math.Sqrt(sigma)))
                             : (-0.9633 + 0.98711 * Math.Sqrt(sigma)));
            var b0 = (float)(1.57825 + 2.44413 * q + 1.4281 * q * q + 0.422205 * q * q * q);
            var b1 = (float)(2.44413 * q + 2.85619 * q * q + 1.26661 * q * q * q);
            var b2 = (float)(-1.4281 * q * q - 1.26661 * q * q * q);
            var b3 = (float)(0.422205 * q * q * q);
            var k1 = (float)(1.0 - ((b1 + b2 + b3) / b0));
            var k2 = (float)(k1 + (b1 + b2 + b3) / b0);
            var k3 = (float)((b1 + b2 + b3) / b0);

            for (var c = 0; c < 3; c++)
            {
                for (var i = 0; i < m.Length; i++)
                {
                    var y = i / w;
                    var x = i % w;
                    m[x * h + y] = 1f * bytes[i * ps + c];
                }

                for (int y = 0; y < h; y++)
                {
                    r[y] = m[y] * k2;
                    r[h + y] = k1 * m[h + y] + r[y] * k3;
                    r[2 * h + y] = k1 * m[2 * h + y] + (b1 * r[h + y] + (b2 + b3) * r[y]) / b0;

                    for (int x = 3; x < w; x++)
                        r[x * h + y] = k1 * m[x * h + y] + (b1 * r[(x - 1) * h + y] + b2 * r[(x - 2) * h + y] + b3 * r[(x - 3) * h + y]) / b0;

                    r[(w - 1) * h + y] = r[(w - 1) * h + y] * k2;
                    r[(w - 2) * h + y] = k1 * r[(w - 2) * h + y] + r[(w - 1) * h + y] * k3;
                    r[(w - 3) * h + y] = k1 * r[(w - 3) * h + y] + (b1 * r[(w - 2) * h + y] + (b2 + b3) * r[(w - 1) * h + y]) / b0;

                    for (int x = w - 4; x >= 0; x--)
                        r[x * h + y] = k1 * r[x * h + y] + (b1 * r[(x + 1) * h + y] + b2 * r[(x + 2) * h + y] + b3 * r[(x + 3) * h + y]) / b0;
                }

                for (int x = 0; x < w; x++)
                {
                    int row = x * h;
                    r[row] = r[row] * k2;
                    r[row + 1] = k1 * r[row + 1] + r[row] * k3;
                    r[row + 2] = k1 * r[row + 2] + (b1 * r[row + 1] + (b2 + b3) * r[row]) / b0;

                    for (int y = 3; y < h; y++)
                        r[row + y] = k1 * r[row + y] + (b1 * r[row + y - 1] + b2 * r[row + y - 2] + b3 * r[row + y - 3]) / b0;

                    r[row + h - 1] = r[row + h - 1] * k2;
                    r[row + h - 2] = k1 * r[row + h - 2] + r[row + h - 1] * k3;
                    r[row + h - 3] = k1 * r[row + h - 3] + (b1 * r[row + h - 2] + (b2 + b3) * r[row + h - 1]) / b0;

                    for (int y = h - 4; y >= 0; y--)
                        r[row + y] = k1 * r[row + y] + (b1 * r[row + y + 1] + b2 * r[row + y + 2] + b3 * r[row + y + 3]) / b0;
                }

                for (var i = 0; i < r.Length; i++)
                {
                    var y = i / w;
                    var x = i % w;
                    var val = r[x * h + y];
                    bytes[i * ps + c] = (byte)((val < 255) ? (val > 0 ? val : 0) : 255);
                }
            }

            var result = new Bitmap(w, h, bitmap.PixelFormat);
            bitmapData = result.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            Marshal.Copy(bytes, 0, bitmapData.Scan0, bytes.Length);
            result.UnlockBits(bitmapData);

            return result;
        }
    }
}
