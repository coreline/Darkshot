using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Darkshot.PaintTools
{
    class PaintToolArrow : PaintTool
    {
        const float ARROW_OFFSET = 5;
        const float PEN_WIDTH = 3;
        Point _pointStart;
        Point _pointEnd;
        Point _pointLast;
        Pen _pen;
        bool _drawing;

        public PaintToolArrow(Color color)
        {
            Paint += (s, e) => { onPaint(e.Graphics); };
            MouseDown += (s, e) => { onMouseDown(e); };
            MouseUp += (s, e) => { onMouseUp(s as Control, e); };
            MouseMove += (s, e) => { onMouseMove(e); };
            KeyDown += (s, e) => setLastPoint();
            KeyUp += (s, e) => setLastPoint();

            _pointStart = Point.Empty;
            _pointEnd = Point.Empty;
            _drawing = false;

            var w = 3F;
            var h = 3F;
            var cap = new GraphicsPath();
            cap.AddPolygon(new PointF[] { new PointF(0, ARROW_OFFSET), new PointF(-w, -h), new PointF(w, -h) });
            _pen = new Pen(color, PEN_WIDTH);
            _pen.CustomEndCap = new CustomLineCap(cap, null, LineCap.RoundAnchor);
        }

        public override Rectangle GetBounds()
        {
            const int radius = 40;
            var rect = new Rectangle();
            rect.X = Math.Min(_pointStart.X, _pointEnd.X) - radius;
            rect.Y = Math.Min(_pointStart.Y, _pointEnd.Y) - radius;
            rect.Width = Math.Abs(_pointStart.X - _pointEnd.X) + 2 * radius;
            rect.Height = Math.Abs(_pointStart.Y - _pointEnd.Y) + 2 * radius;
            return rect;
        }

        void onPaint(Graphics g)
        {
            var dx = _pointEnd.X - _pointStart.X;
            var dy = _pointEnd.Y - _pointStart.Y;
            var length = Math.Sqrt(dx * dx + dy * dy);
            var offset = ARROW_OFFSET * PEN_WIDTH;
            if (length < offset)
                return;
            var cos = 1d * dx / length;
            var sin = 1d * dy / length;
            var x = _pointStart.X + (length - offset) * cos;
            var y = _pointStart.Y + (length - offset) * sin;
            var point = new Point((int)x, (int)y);
            g.DrawLine(_pen, _pointStart, point);
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

        void onMouseUp(Control canvas, MouseEventArgs e)
        {
            if (!_drawing)
                return;
            _drawing = false;
            setEndPoint(e.Location);
            RaiseComplete(canvas);
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
