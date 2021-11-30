using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Keyboard = System.Windows.Input.Keyboard;
using Key = System.Windows.Input.Key;

namespace Darkshot.PaintTools
{
    class PaintToolArrow : PaintTool
    {
        const float ARROW_OFFSET = 5;
        const float PEN_WIDTH = 3;
        Point _pointStart;
        Point _pointEnd;
        Pen _pen;
        bool _drawing;

        public PaintToolArrow(Color color)
        {
            Paint += (s, e) => { onPaint(e.Graphics); };
            MouseDown += (s, e) => { onMouseDown(e); };
            MouseUp += (s, e) => { onMouseUp(e); };
            MouseMove += (s, e) => { onMouseMove(e); };

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

        void onMouseUp(MouseEventArgs e)
        {
            if (!_drawing)
                return;
            _drawing = false;
            setEndPoint(e.Location);
            RaiseComplete();
        }

        void setEndPoint(Point point)
        {
            _pointEnd = point;

            if (!Keyboard.IsKeyDown(Key.LeftShift))
                return;

            var dx = Math.Abs(_pointEnd.X - _pointStart.X);
            var dy = Math.Abs(_pointEnd.Y - _pointStart.Y);
            var posX = _pointStart.X + Math.Sign(_pointEnd.X - _pointStart.X) * Math.Max(dx, dy);
            var posY = _pointStart.Y + Math.Sign(_pointEnd.Y - _pointStart.Y) * Math.Max(dx, dy);
            var lenZ = (int)Math.Sqrt(Math.Pow(posX - _pointStart.X, 2) + Math.Pow(posY - _pointStart.Y, 2));
            var dz = (int)Math.Abs(lenZ - Math.Sqrt(dx * dx + dy * dy));
            var min = Math.Min(Math.Min(dx, dy), dz);
            if (min == dx)
                _pointEnd.X = _pointStart.X;
            else if (min == dy)
                _pointEnd.Y = _pointStart.Y;
            else
                _pointEnd = new Point(posX, posY);
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
    }
}
