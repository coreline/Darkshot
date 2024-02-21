using System;
using System.Drawing;
using System.Windows.Forms;

namespace Darkshot.PaintTools
{
    class PaintToolFilledRectangle : PaintTool
    {
        Rectangle _rect;
        Point _point;
        Brush _brush;
        bool _drawing;

        public PaintToolFilledRectangle(Color color)
        {
            Paint += (s, e) => { onPaint(e.Graphics); };
            MouseDown += (s, e) => { onMouseDown(e); };
            MouseUp += (s, e) => { onMouseUp(s as Control, e); };
            MouseMove += (s, e) => { onMouseMove(e); };

            _brush = new SolidBrush(color);
            _rect = Rectangle.Empty;
            _drawing = false;
        }

        void onPaint(Graphics g)
        {
            g.FillRectangle(_brush, _rect);
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
