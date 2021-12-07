using System;
using System.Drawing;
using System.Windows.Forms;

namespace Darkshot.PaintTools
{
    class PaintToolLine : PaintTool
    {
        const int PEN_WIDTH = 3;
        Point _pointStart;
        Point _pointEnd;
        Point _pointLast;
        Pen _pen;
        bool _drawing;

        public PaintToolLine(Color color)
        {
            Paint += (s, e) => { onPaint(e.Graphics); };
            MouseDown += (s, e) => { onMouseDown(e); };
            MouseUp += (s, e) => { onMouseUp(e); };
            MouseMove += (s, e) => { onMouseMove(e); };
            KeyDown += (s, e) => setLastPoint();
            KeyUp += (s, e) => setLastPoint();

            _pen = new Pen(color, PEN_WIDTH);
            _pointStart = Point.Empty;
            _pointEnd = Point.Empty;
            _drawing = false;
        }

        public override Rectangle GetBounds()
        {
            const int radius = 20;
            var rect = new Rectangle();
            rect.X = Math.Min(_pointStart.X, _pointEnd.X) - radius;
            rect.Y = Math.Min(_pointStart.Y, _pointEnd.Y) - radius;
            rect.Width = Math.Abs(_pointStart.X - _pointEnd.X) + 2 * radius;
            rect.Height = Math.Abs(_pointStart.Y - _pointEnd.Y) + 2 * radius;
            return rect;
        }

        void onPaint(Graphics g)
        {
            if (_pointStart == Point.Empty && _pointEnd == Point.Empty)
                return;
            g.DrawLine(_pen, _pointStart, _pointEnd);
        }

        void onMouseDown(MouseEventArgs e)
        {
            _drawing = true;
            _pointStart = e.Location;
            _pointEnd = e.Location;
        }

        void onMouseMove(MouseEventArgs e)
        {
            if (!_drawing)
                return;
            setEndPoint(e.Location);
        }

        void onMouseUp(MouseEventArgs e)
        {
            if (!_drawing)
                return;
            _drawing = false;
            setEndPoint(e.Location);
            RaiseComplete();
        }

        void setLastPoint()
        {
            setEndPoint(_pointLast);
        }

        void setEndPoint(Point point)
        {
            _pointLast = point;
            _pointEnd = IsShiftDown() ? getStraightLine(_pointStart, point) : point;
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
