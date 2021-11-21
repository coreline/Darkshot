using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

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
            const int radius = 40;
            var rect = Roi;
            rect.X -= radius;
            rect.Y -= radius;
            rect.Width += 2 * radius;
            rect.Height += 2 * radius;
            return rect;
        }

        void onMouseDown(Control canvas, MouseEventArgs e)
        {
            if (IsEmpty)
            {
                IsEmpty = false;
                IsCreating = true;
                RoiMode = RoiModeType.Corner;
                RefreshRoi(e.Location);
            }
            else if (RoiMode != RoiModeType.None)
            {
                IsCreating = true;
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
            RaiseComplete();
        }

        void onPaint(Control canvas, Graphics g)
        {
            var roi = this.Roi;
            var size = canvas.Size;
            var darkRegions = new Rectangle[]
            {
                new Rectangle(0, 0, size.Width, roi.Y),
                new Rectangle(0, roi.Y, roi.X, roi.Height),
                new Rectangle(roi.X + roi.Width, roi.Y, size.Width - roi.X + roi.Width, roi.Height),
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
                this._cursor = IsEmpty ? Cursors.Cross : Cursors.Default;
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
    }
}
