using Darkshot.Gdi32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Darkshot.PaintTools
{
    class PaintToolBlur : PaintTool
    {
        const int RADIUS = 20;
        Rectangle _rect;
        Point _point;
        bool _drawing;

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
                using (var blurred = bitmap.Gaussian(RADIUS))
                {
                    using (var croppedBlurred = blurred.Clone(rectIn, bitmap.PixelFormat))
                        g.DrawImage(croppedBlurred, _rect);
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
    }
}
