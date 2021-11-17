using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Darkshot.PaintTools
{
    class PaintToolArrow : IPaintTool
    {
        const int PEN_WIDTH = 3;
        Point _pointStart;
        Point _pointEnd;
        Pen _pen;
        bool _drawing;

        public PaintToolArrow(Color color)
        {
            var w = 3F;
            var h = w * 3F;
            var cap = new GraphicsPath();
            cap.AddPolygon(new PointF[] { new PointF(0, 0.5F), new PointF(-w, -h), new PointF(w, -h) });
            _pen = new Pen(color, PEN_WIDTH);
            _pen.CustomEndCap = new CustomLineCap(cap, null, LineCap.RoundAnchor);
            _pointStart = Point.Empty;
            _pointEnd = Point.Empty;
            _drawing = false;
        }

        void IPaintTool.Paint(Graphics g)
        {
            if (_pointStart == Point.Empty && _pointEnd == Point.Empty)
                return;
            g.DrawLine(_pen, _pointStart, _pointEnd);
        }

        bool IPaintTool.ProcessMouseDown(MouseEventArgs e)
        {
            _drawing = true;
            _pointStart = e.Location;
            _pointEnd = e.Location;
            return true;
        }

        bool IPaintTool.ProcessMouseMove(MouseEventArgs e)
        {
            if (!_drawing)
                return false;
            _pointEnd = e.Location;
            return true;
        }

        bool IPaintTool.ProcessMouseUp(MouseEventArgs e)
        {
            _pointEnd = e.Location;
            _drawing = false;
            return true;
        }

        Rectangle IPaintTool.GetBounds()
        {
            const int radius = 40;
            var rect = new Rectangle();
            rect.X = Math.Min(_pointStart.X, _pointEnd.X) - radius;
            rect.Y = Math.Min(_pointStart.Y, _pointEnd.Y) - radius;
            rect.Width = Math.Abs(_pointStart.X - _pointEnd.X) + 2 * radius;
            rect.Height = Math.Abs(_pointStart.Y - _pointEnd.Y) + 2 * radius;
            return rect;
        }

        Cursor IPaintTool.GetCursor()
        {
            return Cursors.Default;
        }

        bool IPaintTool.ProcessKeyDown(KeyEventArgs e)
        {
            return false;
        }
    }
}
