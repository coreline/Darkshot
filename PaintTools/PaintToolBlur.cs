using Darkshot.Gdi32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Darkshot.PaintTools
{
    class PaintToolBlur : PaintTool
    {
        const int RADIUS = 16;
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
                using (var blurred = Blur(bitmap, RADIUS))
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

        Bitmap Blur(Bitmap bitmap, float radius)
        {
            if (bitmap.Width < 3 || bitmap.Height < 3 || radius < 0.25f)
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

            var l = w * h;
            var m = new float[l];
            var q = (float)((radius <= 2.5)
                             ? (3.97156 - 4.14554 * Math.Sqrt(1.0 - 0.26891 * Math.Sqrt(radius)))
                             : (-0.9633 + 0.98711 * Math.Sqrt(radius)));
            var b0 = (float)(1.57825 + 2.44413 * q + 1.4281 * q * q + 0.422205 * q * q * q);
            var b1 = (float)(2.44413 * q + 2.85619 * q * q + 1.26661 * q * q * q);
            var b2 = (float)(-1.4281 * q * q - 1.26661 * q * q * q);
            var b3 = (float)(0.422205 * q * q * q);
            var b4 = (float)(1.0 - ((b1 + b2 + b3) / b0));
            var b5 = (float)(b4 + (b1 + b2 + b3) / b0);
            var b6 = (float)((b1 + b2 + b3) / b0);
            var b7 = (float)(b2 + b3);


            for (var c = 0; c < 3; c++)
            {
                for (var i = 0; i < l; i++)
                    m[i] = 1f * bytes[i * ps + c];

                for (int y = 0; y < l; y += w)
                {
                    m[y] = m[y] * b5;
                    m[y + 1] = b4 * m[y + 1] + m[y] * b6;
                    m[y + 2] = b4 * m[y + 2] + (b1 * m[y + 1] + b7 * m[y]) / b0;

                    for (int x = 3; x < w; x++)
                        m[y + x] = b4 * m[y + x] + (b1 * m[y + x - 1] + b2 * m[y + x - 2] + b3 * m[y + x - 3]) / b0;

                    m[y + w - 1] = m[y + w - 1] * b5;
                    m[y + w - 2] = b4 * m[y + w - 2] + m[y + w - 1] * b6;
                    m[y + w - 3] = b4 * m[y + w - 3] + (b1 * m[y + w - 2] + b7 * m[y + w - 1]) / b0;

                    for (int x = w - 4; x >= 0; x--)
                        m[y + x] = b4 * m[y + x] + (b1 * m[y + x + 1] + b2 * m[y + x + 2] + b3 * m[y + x + 3]) / b0;
                }

                for (int x = 0; x < w; x++)
                {
                    m[x] = m[x] * b5;
                    m[x + w] = b4 * m[x + w] + m[x] * b6;
                    m[x + 2 * w] = b4 * m[x + 2 * w] + (b1 * m[x + w] + b7 * m[x]) / b0;

                    for (int y = 3 * w + x; y < l; y += w)
                        m[y] = b4 * m[y] + (b1 * m[y - w] + b2 * m[y - 2 * w] + b3 * m[y - 3 * w]) / b0;

                    m[(h - 1) * w + x] = m[(h - 1) * w + x] * b5;
                    m[(h - 2) * w + x] = b4 * m[(h - 2) * w + x] + m[(h - 1) * w + x] * b6;
                    m[(h - 3) * w + x] = b4 * m[(h - 3) * w + x] + (b1 * m[(h - 2) * w + x] + b7 * m[(h - 1) * w + x]) / b0;

                    for (int y = (h - 4) * w + x; y >= 0; y -= w)
                        m[y] = b4 * m[y] + (b1 * m[y + w] + b2 * m[y + 2 * w] + b3 * m[y + 3 * w]) / b0;
                }

                for (var i = 0; i < l; i++)
                {
                    var val = m[i];
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
