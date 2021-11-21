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
        Pen _pen;
        bool _drawing;

        public PaintToolLine(Color color)
        {
            Paint += (s, e) => { onPaint(e.Graphics); };
            MouseDown += (s, e) => { onMouseDown(e); };
            MouseUp += (s, e) => { onMouseUp(e); };
            MouseMove += (s, e) => { onMouseMove(e); };

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

        void  onMouseMove(MouseEventArgs e)
        {
            if (!_drawing)
                return;
            _pointEnd = e.Location;
        }

        void onMouseUp(MouseEventArgs e)
        {
            if (!_drawing)
                return;
            _pointEnd = e.Location;
            _drawing = false;
            RaiseComplete();
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
