using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Darkshot.PaintTools
{
    class PaintToolPen : IPaintTool
    {
        const int PEN_WIDTH = 3;
        List<Point> _points;
        Pen _pen;
        bool _drawing;

        public PaintToolPen(Color color)
        {
            _pen = new Pen(color, PEN_WIDTH);
            _pen.StartCap = LineCap.Round;
            _pen.EndCap = LineCap.Round;
            _pen.LineJoin = LineJoin.Round;
            _points = new List<Point>();
            _drawing = false;
        }

        void IPaintTool.Paint(Graphics g)
        {
            if (_points.Count < 1)
                return;

            var points = _points.Count > 1
                ? _points.ToArray()
                : new Point[] { _points[0], _points[0] };
            g.DrawLines(_pen, points);
        }

        bool IPaintTool.ProcessMouseDown(MouseEventArgs e)
        {
            _drawing = true;
            _points.Add(e.Location);
            return true;
        }

        bool IPaintTool.ProcessMouseMove(MouseEventArgs e)
        {
            if (!_drawing)
                return false;
            _points.Add(e.Location);
            return true;
        }

        bool IPaintTool.ProcessMouseUp(MouseEventArgs e)
        {
            _drawing = false;
            return true;
        }

        Rectangle IPaintTool.GetBounds()
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
