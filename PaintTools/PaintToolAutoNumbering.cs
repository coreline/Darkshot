using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using Media = System.Windows.Media;

namespace Darkshot.PaintTools
{
    class PaintToolAutoNumbering : PaintTool
    {
        static Dictionary<PaintToolAutoNumbering, int> _counter;
        static int _count;
        const int PEN_WIDTH = 2;
        const int RADIUS = 17;
        Size _size;
        Point _point;
        Rectangle _rect;
        Pen _pen;
        SolidBrush _brush;
        Font _font;
        bool _drawing;

        public PaintToolAutoNumbering(Color color)
        {
            Paint += (s, e) => { onPaint(e.Graphics); };
            MouseDown += (s, e) => { onMouseDown(e); };
            MouseUp += (s, e) => { onMouseUp(s as Control, e); };
            MouseMove += (s, e) => { onMouseMove(e); };
            Complete += (s, e) => { onComplete(); };

            _brush = new SolidBrush(color);
            _pen = new Pen(color.GetBrightness() > 0.5 ? Color.Black : Color.White, PEN_WIDTH);
            _font = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold);
            _size = new Size(2 * RADIUS, 2 * RADIUS);
            _drawing = false;
            _counter = _counter ?? new Dictionary<PaintToolAutoNumbering, int>();
        }

        public static void Reset()
        {
            _counter?.Clear();
            _count = 0;
        }

        void onPaint(Graphics g)
        {
            g.FillEllipse(_brush, _rect);
            g.DrawEllipse(_pen, _rect);

            if (!_counter.ContainsKey(this))
                _counter.Add(this, _counter.Count + 1);
            _count = _counter[this];
            var text = _count.ToString();
            var size = g.MeasureString(text, _font);
            var point = new Point(_point.X - (int)Math.Floor(size.Width / 2) + 1, (int)Math.Floor(_point.Y - size.Height / 2));

            TextFormatFlags flags = TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
            TextRenderer.DrawText(g, text, _font, point, _pen.Color, flags);
        }

        void onMouseDown(MouseEventArgs e)
        {
            _drawing = true;
            setPoint(e.Location);
        }

        void onMouseMove(MouseEventArgs e)
        {
            if (!_drawing)
                return;
            setPoint(e.Location);
        }

        void onMouseUp(Control canvas, MouseEventArgs e)
        {
            if (!_drawing)
                return;
            _drawing = false;
            setPoint(e.Location);
            RaiseComplete(canvas);
        }

        private void onComplete()
        {
            _count = _counter.Values.Max();
            _counter.Clear();
        }

        void setPoint(Point p)
        {
            _point = p;
            _rect = new Rectangle(new Point(p.X - RADIUS, p.Y - RADIUS), _size);
        }


        public override Rectangle GetBounds()
        {
            const int radius = 3 * RADIUS;
            return new Rectangle(_point.X - radius, _point.Y - radius, 2 * radius, 2 * radius);
        }

        public override Cursor GetDefaultCursor()
        {
            using (var bitmap = new Bitmap(RADIUS * 2 + PEN_WIDTH, RADIUS * 2 + PEN_WIDTH, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bitmap))
            {
                var rect = new RectangleF(1, 1, bitmap.Width - 2, bitmap.Height - 2);
                var brush = new Pen(Color.FromArgb(127, _brush.Color)).Brush;
                var pen = new Pen(Color.FromArgb(127, _pen.Color), PEN_WIDTH);
                g.FillEllipse(brush, rect);
                g.DrawEllipse(pen, rect);


                var text = (_count + 1).ToString();
                var size = g.MeasureString(text, _font);
                var point = new Point(RADIUS - (int)Math.Floor(size.Width / 2) + 2, (int)Math.Floor(RADIUS - size.Height / 2) + 1);

                TextFormatFlags flags = TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
                TextRenderer.DrawText(g, text, _font, point, pen.Color, flags);

                return new Cursor(bitmap.GetHicon());
            }
        }
    }
}
