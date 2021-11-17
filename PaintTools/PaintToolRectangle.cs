using System;
using System.Drawing;
using System.Windows.Forms;

namespace Darkshot.PaintTools
{
    class PaintToolRectangle : IPaintTool
    {
        const int PEN_WIDTH = 3;
        Rectangle _rect;
        Point _point;
        Pen _pen;
        bool _drawing;

        public PaintToolRectangle(Color color)
        {
            _pen = new Pen(color, PEN_WIDTH);
            _rect = Rectangle.Empty;
            _drawing = false;
        }

        void IPaintTool.Paint(Graphics g)
        {
            g.DrawRectangle(_pen, _rect);
        }

        bool IPaintTool.ProcessMouseDown(MouseEventArgs e)
        {
            _drawing = true;
            _point = e.Location;
            _rect.Location = e.Location;
            return true;
        }

        bool IPaintTool.ProcessMouseMove(MouseEventArgs e)
        {
            if (!_drawing)
                return false;
            _rect.X = Math.Min(_point.X, e.Location.X);
            _rect.Y = Math.Min(_point.Y, e.Location.Y);
            _rect.Width = Math.Abs(_point.X - e.Location.X);
            _rect.Height = Math.Abs(_point.Y - e.Location.Y);
            return true;
        }

        bool IPaintTool.ProcessMouseUp(MouseEventArgs e)
        {
            _rect.X = Math.Min(_point.X, e.Location.X);
            _rect.Y = Math.Min(_point.Y, e.Location.Y);
            _rect.Width = Math.Abs(_point.X - e.Location.X);
            _rect.Height = Math.Abs(_point.Y - e.Location.Y);
            _drawing = false;
            return true;
        }

        Rectangle IPaintTool.GetBounds()
        {
            const int radius = 20;
            var rect = _rect;
            rect.X -= radius;
            rect.Y -= radius;
            rect.Width += 2 * radius;
            rect.Height += 2 * radius;
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
