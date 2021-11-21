using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Darkshot.PaintTools
{
    class PaintToolPen : PaintTool
    {
        const int PEN_WIDTH = 3;
        List<Point> _points;
        Pen _pen;
        bool _drawing;

        public PaintToolPen(Color color)
        {
            Paint += (s, e) => { onPaint(e.Graphics); };
            MouseDown += (s, e) => { onMouseDown(e); };
            MouseUp += (s, e) => { onMouseUp(e); };
            MouseMove += (s, e) => { onMouseMove(e); };

            _pen = new Pen(color, PEN_WIDTH);
            _pen.StartCap = LineCap.Round;
            _pen.EndCap = LineCap.Round;
            _pen.LineJoin = LineJoin.Round;
            _points = new List<Point>();
            _drawing = false;
        }

        void onPaint(Graphics g)
        {
            if (_points.Count < 1)
                return;

            var points = _points.Count > 1
                ? _points.ToArray()
                : new Point[] { _points[0], _points[0] };
            g.DrawLines(_pen, points);
        }

        void onMouseDown(MouseEventArgs e)
        {
            _drawing = true;
            _points.Add(e.Location);
        }

        void onMouseMove(MouseEventArgs e)
        {
            if (!_drawing)
                return;
            _points.Add(e.Location);
        }

        void onMouseUp(MouseEventArgs e)
        {
            if (!_drawing)
                return;
            _drawing = false;
            RaiseComplete();
        }

        public override Rectangle GetBounds()
        {
            const int radius = 20;
            if (_points.Count == 0)
                return Rectangle.Empty;

            var rect = new Rectangle(_points[0], Size.Empty);
            foreach (var p in _points)
            {
                rect.X = Math.Min(rect.X, p.X);
                rect.Y = Math.Min(rect.Y, p.Y);
                rect.Width = Math.Abs(rect.X - p.X);
                rect.Height = Math.Abs(rect.Y - p.Y);
            }
            rect.X -= radius;
            rect.Y -= radius;
            rect.Width += 2 * radius;
            rect.Height += 2 * radius;
            return rect;
        }
    }
}
