using System;
using System.Drawing;
using System.Windows.Forms;
using Keyboard = System.Windows.Input.Keyboard;
using Key = System.Windows.Input.Key;

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
            const int radius = 20;
            var rect = new Rectangle();
            rect.X = Math.Min(_pointStart.X, _pointEnd.X) - radius;
            rect.Y = Math.Min(_pointStart.Y, _pointEnd.Y) - radius;
            rect.Width = Math.Abs(_pointStart.X - _pointEnd.X) + 2 * radius;
            rect.Height = Math.Abs(_pointStart.Y - _pointEnd.Y) + 2 * radius;
            return rect;
        }
    }
}
