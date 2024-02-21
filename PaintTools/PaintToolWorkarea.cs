using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Keyboard = System.Windows.Input.Keyboard;
using Key = System.Windows.Input.Key;

namespace Darkshot.PaintTools
{
    class PaintToolWorkarea : PaintTool
    {
        enum RoiModeType
        {
            None,
            Corner,
            Horizontal,
            Vertical,
            Move
        }
        const int RESIZE_RECTANGLE_RADIUS = 3;
        Point _startPoint;
        Point[] _resizePoints;
        Rectangle[] _resizeRectangles;
        Cursor _cursor;

        public PaintToolWorkarea()
        {
            Paint += (s, e) => { onPaint(s as Control, e.Graphics); };
            MouseDown += (s, e) => { onMouseDown(s as Control, e); };
            MouseUp += (s, e) => { onMouseUp(s as Control, e); };
            MouseMove += (s, e) => { onMouseMove(s as Control, e); };
            KeyDown += (s, e) => { onKeyDown(s as Control, e); };
            KeyUp += (s, e) => { onKeyUp(s as Control, e); };

            this.IsEmpty = true;
            this.IsCreating = false;
            this._cursor = Cursors.Default;
            RoiMode = RoiModeType.None;
            _resizePoints = new Point[8];
            _resizeRectangles = new Rectangle[_resizePoints.Length];
        }
        public Rectangle Roi { get; private set; }
        RoiModeType RoiMode { get; set; }

        public bool IsEmpty
        {
            get; private set;
        }

        public bool IsCreating
        {
            get; private set;
        }

        public override Rectangle GetBounds()
        {
            const int radius = 100;
            var rect = Roi;
            rect.Inflate(radius, radius);
            return rect;
        }

        void onMouseDown(Control canvas, MouseEventArgs e)
        {
            IsCreating = true;
            if (IsEmpty)
            {
                IsEmpty = false;
                RoiMode = RoiModeType.Corner;
                RefreshRoi(e.Location);
            }
            else if (RoiMode != RoiModeType.None)
            {
                StartResizeRoi(e.Location);
                RefreshRoi(e.Location);
            }
            canvas.Cursor = _cursor;
        }

        void onMouseMove(Control canvas, MouseEventArgs e)
        {
            if (IsCreating)
                RefreshRoi(e.Location);
            else
                RefreshPossibleRoiMode(e.Location);
            canvas.Cursor = _cursor;
        }

        void onMouseUp(Control canvas, MouseEventArgs e)
        {
            if (!IsCreating)
                return;

            RefreshRoi(e.Location);
            IsCreating = false;
            RoiMode = RoiModeType.None;
            canvas.Cursor = _cursor;
            RaiseComplete(canvas);
        }

        void onKeyDown(Control canvas, KeyEventArgs e)
        {
            if (IsCreating)
                return;
            var left = Keyboard.IsKeyDown(Key.Left) ? 1 : 0;
            var right = Keyboard.IsKeyDown(Key.Right) ? 1 : 0;
            var up = Keyboard.IsKeyDown(Key.Up) ? 1 : 0;
            var down = Keyboard.IsKeyDown(Key.Down) ? 1 : 0;
            var scale = IsShiftDown() ? 1 : 0;
            var move = 1 - scale;
            var dx = right - left;
            var dy = down - up;
            if (dx == 0 && dy == 0)
                return;
            var x = Roi.X + move * dx;
            var y = Roi.Y + move * dy;
            var w = Roi.Width + scale * dx;
            var h = Roi.Height + scale * dy;
            Roi = new Rectangle(x, y, w, h);
            RefreshRoi();
        }

        void onKeyUp(Control canvas, KeyEventArgs e)
        {
        }

        void onPaint(Control canvas, Graphics g)
        {
            var roi = this.Roi;
            var size = canvas.Size;
            var darkRegions = new Rectangle[]
            {
                new Rectangle(0, 0, size.Width, roi.Y - 1),
                new Rectangle(0, roi.Y - 1, roi.X - 1, roi.Height + 1),
                new Rectangle(roi.X + roi.Width, roi.Y - 1, size.Width - roi.X + roi.Width, roi.Height + 1),
                new Rectangle(0, roi.Y + roi.Height, size.Width, size.Height - roi.Y - roi.Height)
            };
            g.SmoothingMode = SmoothingMode.Default;
            g.InterpolationMode = InterpolationMode.Default;
            g.CompositingQuality = CompositingQuality.Default;

            using (var brush = new SolidBrush(Color.FromArgb(127, Color.Black)))
                g.FillRectangles(brush, darkRegions);

            using (var pen = new Pen(Color.Gray))
            {
                pen.Width = 1;
                pen.DashCap = DashCap.Flat;
                pen.DashPattern = new float[] { 5, 2 };
                g.DrawRectangle(pen, Roi);
            }

            g.FillRectangles(Brushes.White, _resizeRectangles);
            g.DrawRectangles(Pens.Black, _resizeRectangles);

            if (IsCreating && (RoiMode == RoiModeType.Corner || RoiMode == RoiModeType.Horizontal || RoiMode == RoiModeType.Vertical) || IsShiftDown())
            {
                var bitmapRoi = roi;
                if (bitmapRoi.X < 0)
                {
                    bitmapRoi.Width -= Math.Abs(bitmapRoi.X);
                    bitmapRoi.X = 0;
                }
                if (bitmapRoi.Y < 0)
                {
                    bitmapRoi.Height -= Math.Abs(bitmapRoi.Y);
                    bitmapRoi.Y = 0;
                }
                if (bitmapRoi.Right > size.Width)
                    bitmapRoi.Width -= bitmapRoi.Right - size.Width;
                if (bitmapRoi.Bottom > size.Height)
                    bitmapRoi.Height -= bitmapRoi.Bottom - size.Height;
                bitmapRoi.Width = Math.Max(0, bitmapRoi.Width);
                bitmapRoi.Height = Math.Max(0, bitmapRoi.Height);

                var text = string.Format("[{0}×{1}]px", bitmapRoi.Width, bitmapRoi.Height);
                var font = new Font(FontFamily.GenericMonospace, 12, FontStyle.Regular, GraphicsUnit.Pixel);
                var fontFrameMargin = 4;
                var fontBoundsMargin = new Point(100, 20);
                var fontPadding = new Point(2, 1);
                var fontSize = g.MeasureString(text, font);
                var fontRoi = new Rectangle(bitmapRoi.X - 1,
                                            bitmapRoi.Y - (int)fontSize.Height - fontPadding.Y - fontFrameMargin - 1,
                                            (int)fontSize.Width + 2 * fontPadding.X + 1,
                                            (int)fontSize.Height + 2 * fontPadding.Y + 1);

                fontRoi.X = Math.Max(fontBoundsMargin.X, fontRoi.X);
                fontRoi.Y = Math.Max(fontBoundsMargin.Y, fontRoi.Y);
                if (fontRoi.Right + fontBoundsMargin.X > size.Width)
                    fontRoi.X = size.Width - fontBoundsMargin.X - fontRoi.Width;
                if (fontRoi.Bottom + fontBoundsMargin.Y > size.Height)
                    fontRoi.Y = size.Height - fontBoundsMargin.Y - fontRoi.Height;

                using (var brush = new SolidBrush(Color.FromArgb(255, Color.Black)))
                    g.FillPath(brush, RoundedRect(fontRoi, 3));
                g.DrawString(text, font, Brushes.White, fontRoi.X + fontPadding.X - 1, fontRoi.Y + fontPadding.Y - 1);
            }
        }

        void RefreshPossibleRoiMode(Point p)
        {
            if (IsPointInRectangle(_resizeRectangles[0], p) || IsPointInRectangle(_resizeRectangles[7], p))
            {
                RoiMode = RoiModeType.Corner;
                this._cursor = Cursors.SizeNWSE;
            }
            else if (IsPointInRectangle(_resizeRectangles[1], p) || IsPointInRectangle(_resizeRectangles[6], p))
            {
                RoiMode = RoiModeType.Vertical;
                this._cursor = Cursors.SizeNS;
            }
            else if (IsPointInRectangle(_resizeRectangles[2], p) || IsPointInRectangle(_resizeRectangles[5], p))
            {
                RoiMode = RoiModeType.Corner;
                this._cursor = Cursors.SizeNESW;
            }
            else if (IsPointInRectangle(_resizeRectangles[3], p) || IsPointInRectangle(_resizeRectangles[4], p))
            {
                RoiMode = RoiModeType.Horizontal;
                this._cursor = Cursors.SizeWE;
            }
            else if (IsPointInRectangle(Roi, p))
            {
                RoiMode = RoiModeType.Move;
                this._cursor = Cursors.SizeAll;
            }
            else
            {
                RoiMode = RoiModeType.None;
                this._cursor = Cursors.Default;
            }
        }

        void StartResizeRoi(Point p)
        {
            switch (RoiMode)
            {
                case RoiModeType.Corner:
                    if (IsPointInRectangle(_resizeRectangles[0], p))
                        _startPoint = _resizePoints[7];
                    else if (IsPointInRectangle(_resizeRectangles[2], p))
                        _startPoint = _resizePoints[5];
                    else if (IsPointInRectangle(_resizeRectangles[5], p))
                        _startPoint = _resizePoints[2];
                    else if (IsPointInRectangle(_resizeRectangles[7], p))
                        _startPoint = _resizePoints[0];
                    break;
                case RoiModeType.Horizontal:
                    if (IsPointInRectangle(_resizeRectangles[3], p))
                        _startPoint = _resizePoints[4];
                    else if (IsPointInRectangle(_resizeRectangles[4], p))
                        _startPoint = _resizePoints[3];
                    break;
                case RoiModeType.Vertical:
                    if (IsPointInRectangle(_resizeRectangles[1], p))
                        _startPoint = _resizePoints[6];
                    else if (IsPointInRectangle(_resizeRectangles[6], p))
                        _startPoint = _resizePoints[1];
                    break;
                case RoiModeType.Move:
                    _startPoint.X = p.X - Roi.X;
                    _startPoint.Y = p.Y - Roi.Y;
                    break;
            }
            RefreshRoi(p);
        }

        void RefreshRoi(Point pos)
        {
            if (_startPoint.IsEmpty)
                _startPoint = pos;

            var roi = Roi;
            switch (RoiMode)
            {
                case RoiModeType.Corner:
                    roi.X = Math.Min(_startPoint.X, pos.X);
                    roi.Y = Math.Min(_startPoint.Y, pos.Y);
                    roi.Width = Math.Abs(_startPoint.X - pos.X);
                    roi.Height = Math.Abs(_startPoint.Y - pos.Y);
                    break;

                case RoiModeType.Horizontal:
                    roi.X = Math.Min(_startPoint.X, pos.X);
                    roi.Width = Math.Abs(_startPoint.X - pos.X);
                    break;

                case RoiModeType.Vertical:
                    roi.Y = Math.Min(_startPoint.Y, pos.Y);
                    roi.Height = Math.Abs(_startPoint.Y - pos.Y);
                    break;

                case RoiModeType.Move:
                    roi.X = pos.X - _startPoint.X;
                    roi.Y = pos.Y - _startPoint.Y;
                    break;

                default:
                    return;
            }
            Roi = roi;
            RefreshRoi();
        }

        void RefreshRoi()
        {
            _resizePoints[0] = new Point(Roi.X, Roi.Y);
            _resizePoints[1] = new Point(Roi.X + Roi.Width / 2, Roi.Y);
            _resizePoints[2] = new Point(Roi.X + Roi.Width, Roi.Y);
            _resizePoints[3] = new Point(Roi.X, Roi.Y + Roi.Height / 2);
            _resizePoints[4] = new Point(Roi.X + Roi.Width, Roi.Y + Roi.Height / 2);
            _resizePoints[5] = new Point(Roi.X, Roi.Y + Roi.Height);
            _resizePoints[6] = new Point(Roi.X + Roi.Width / 2, Roi.Y + Roi.Height);
            _resizePoints[7] = new Point(Roi.X + Roi.Width, Roi.Y + Roi.Height);

            for (var i = 0; i < _resizePoints.Length; i++)
            {
                _resizeRectangles[i].X = _resizePoints[i].X - RESIZE_RECTANGLE_RADIUS;
                _resizeRectangles[i].Y = _resizePoints[i].Y - RESIZE_RECTANGLE_RADIUS;
                _resizeRectangles[i].Width = 2 * RESIZE_RECTANGLE_RADIUS + 1;
                _resizeRectangles[i].Height = 2 * RESIZE_RECTANGLE_RADIUS + 1;
            }
        }

        bool IsPointInRectangle(Rectangle rect, Point point)
        {
            return point.X >= rect.X && point.X <= rect.X + rect.Width
                && point.Y >= rect.Y && point.Y <= rect.Y + rect.Height;
        }

        GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var diameter = radius * 2;
            var size = new Size(diameter, diameter);
            var arc = new Rectangle(bounds.Location, size);
            var path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}
