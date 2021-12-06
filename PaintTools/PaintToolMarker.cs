using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;
using System.Drawing.Imaging;

namespace Darkshot.PaintTools
{
    class PaintToolMarker : PaintTool
    {
        const float PEN_WIDTH = 24;
        Cursor _cursor;
        Point _pointLast;
        List<Point> _points;
        Pen _pen;
        bool _drawing;

        public PaintToolMarker(Color color)
        {
            Paint += (s, e) => { onPaint(e.Graphics); };
            MouseDown += (s, e) => { onMouseDown(s as Control, e); };
            MouseUp += (s, e) => { onMouseUp(s as Control, e); };
            MouseMove += (s, e) => { onMouseMove(s as Control, e); };
            KeyDown += (s, e) => { refreshPoints(); };
            KeyUp += (s, e) => { refreshPoints(); };

            _pen = new Pen(Color.FromArgb(127, color), PEN_WIDTH);
            _pen.StartCap = LineCap.Round;
            _pen.EndCap = LineCap.Round;
            _pen.LineJoin = LineJoin.Round;
            _points = new List<Point>();
            _drawing = false;

            using (var bitmap = new Bitmap((int)PEN_WIDTH + 2, (int)PEN_WIDTH + 2, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bitmap))
            {
                var rect = new RectangleF(1, 1, bitmap.Width - 2, bitmap.Height - 2);
                g.FillEllipse(_pen.Brush, rect);
                g.FillEllipse(_pen.Brush, rect);
                using (var pen = new Pen(color, 1))
                    g.DrawEllipse(pen, rect);
                _cursor = new Cursor(bitmap.GetHicon());
            }
        }

        public override Rectangle GetBounds()
        {
            if (_points.Count == 0)
                return SystemInformation.VirtualScreen;

            var minX = _points.Min(p => p.X);
            var minY = _points.Min(p => p.Y);
            var maxX = _points.Max(p => p.X);
            var maxY = _points.Max(p => p.Y);

            var rect = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
            rect.Inflate((int)PEN_WIDTH, (int)PEN_WIDTH);
            return rect;
        }

        public override Cursor GetDefaultCursor()
        {
            return _cursor;
        }

        void onPaint(Graphics g)
        {
            if (_points.Count < 1)
                return;

            if (_points.Count == 1)
            {
                var w = PEN_WIDTH;
                var r = w / 2;
                var x = _points[0].X - r;
                var y = _points[0].Y - r;
                var rect = new RectangleF(x, y, w, w);
                g.FillEllipse(_pen.Brush, rect);
            }
            else
            {
                g.DrawLines(_pen, _points.ToArray());
            }
        }

        void onMouseDown(Control canvas, MouseEventArgs e)
        {
            _drawing = true;
            _points.Add(e.Location);
            refreshPoints();
        }

        void onMouseMove(Control canvas, MouseEventArgs e)
        {
            if (!_drawing)
                return;
            _pointLast = e.Location;
            refreshPoints();
        }

        void onMouseUp(Control canvas, MouseEventArgs e)
        {
            if (!_drawing)
                return;
            _drawing = false;
            RaiseComplete();
        }

        void refreshPoints()
        {
            if (_pointLast != Point.Empty && _pointLast != _points.Last())
                _points.Add(_pointLast);
            if (_points.Count < 2)
                return;
            if (!IsCtrlDown() && !IsShiftDown())
                return;

            var first = _points[0];
            var last = _pointLast;
            last = IsShiftDown() ? getStraightLine(first, last) : last;
            _points.Clear();
            _points.Add(first);
            _points.Add(last);
        }

        Point getStraightLine(Point start, Point end)
        {
            var w = Math.Abs(end.X - start.X);
            var h = Math.Abs(end.Y - start.Y);
            var x = start.X + Math.Sign(end.X - start.X) * Math.Max(w, h);
            var y = start.Y + Math.Sign(end.Y - start.Y) * Math.Max(w, h);
            var t = (int)Math.Sqrt(Math.Pow(x - start.X, 2) + Math.Pow(y - start.Y, 2));
            var z = (int)Math.Abs(t - Math.Sqrt(w * w + h * h));
            var min = Math.Min(Math.Min(w, h), z);

            if (min == w)
                return new Point(start.X, end.Y);
            if (min == h)
                return new Point(end.X, start.Y);
            return new Point(x, y);
        }
    }
}
