using System;
using System.Drawing;
using System.Windows.Forms;

namespace Darkshot.PaintTools
{
    class PaintToolRectangle : PaintTool
    {
        const int PEN_WIDTH = 3;
        Rectangle _rect;
        Point _point;
        Pen _pen;
        bool _drawing;

        public PaintToolRectangle(Color color)
        {
            Paint += (s, e) => { onPaint(e.Graphics); };
            MouseDown += (s, e) => { onMouseDown(e); };
            MouseUp += (s, e) => { onMouseUp(e); };
            MouseMove += (s, e) => { onMouseMove(e); };

            _pen = new Pen(color, PEN_WIDTH);
            _rect = Rectangle.Empty;
            _drawing = false;
        }

        void onPaint(Graphics g)
        {
            g.DrawRectangle(_pen, _rect);
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

        void onMouseUp(MouseEventArgs e)
        {
            if (!_drawing)
                return;
            _rect.X = Math.Min(_point.X, e.Location.X);
            _rect.Y = Math.Min(_point.Y, e.Location.Y);
            _rect.Width = Math.Abs(_point.X - e.Location.X);
            _rect.Height = Math.Abs(_point.Y - e.Location.Y);
            _drawing = false;
            RaiseComplete();
        }

        public override Rectangle GetBounds()
        {
            const int radius = 20;
            var rect = _rect;
            rect.X -= radius;
            rect.Y -= radius;
            rect.Width += 2 * radius;
            rect.Height += 2 * radius;
            return rect;
        }
    }
}
