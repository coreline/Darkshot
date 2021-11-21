using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Darkshot.PaintTools
{
    enum PaintToolType
    {
        None,
        Pen,
        Line,
        Arrow,
        Rectangle,
        FilledRectangle,
        Text,
        Marker
    }

    abstract class PaintTool
    {
        public event EventHandler Complete;
        protected event PaintEventHandler Paint;
        protected event KeyEventHandler KeyDown;
        protected event MouseEventHandler MouseDown;
        protected event MouseEventHandler MouseMove;
        protected event MouseEventHandler MouseUp;

        public static PaintTool Create(PaintToolType func, Color color, Color bg)
        {
            switch (func)
            {
                case PaintToolType.Pen:
                    return new PaintToolPen(color);
                case PaintToolType.Line:
                    return new PaintToolLine(color);
                case PaintToolType.Arrow:
                    return new PaintToolArrow(color);
                case PaintToolType.Rectangle:
                    return new PaintToolRectangle(color);
                case PaintToolType.FilledRectangle:
                    return new PaintToolFilledRectangle(color);
                case PaintToolType.Text:
                    return new PaintToolText(color);
                case PaintToolType.Marker:
                    return new PaintToolMarker(bg);
                default:
                    throw new NotImplementedException();
            }
        }

        public static Cursor GetDefaultCursor(PaintToolType func)
        {
            return func == PaintToolType.Text ? Cursors.IBeam : Cursors.Default;
        }

        public virtual Rectangle GetBounds()
        {
            return SystemInformation.VirtualScreen;
        }

        public void RaisePaint(Control sender, PaintEventArgs e)
        {
            if (Paint == null)
                return;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            //e.Graphics.TextContrast = 0;

            Paint.Invoke(sender, e);
        }

        public void RaiseKeyDown(Control sender, KeyEventArgs e)
        {
            if (KeyDown == null)
                return;
            var bounds = GetBounds();
            KeyDown.Invoke(sender, e);
            Invalidate(sender, bounds);
        }

        public void RaiseMouseDown(Control sender, MouseEventArgs e)
        {
            if (MouseDown == null)
                return;
            var bounds = GetBounds();
            MouseDown.Invoke(sender, e);
            Invalidate(sender, bounds);
        }

        public void RaiseMouseMove(Control sender, MouseEventArgs e)
        {
            if (MouseMove == null)
                return;
            var bounds = GetBounds();
            MouseMove.Invoke(sender, e);
            Invalidate(sender, bounds);
        }

        public void RaiseMouseUp(Control sender, MouseEventArgs e)
        {
            if (MouseUp == null)
                return;
            var bounds = GetBounds();
            MouseUp.Invoke(sender, e);
            Invalidate(sender, bounds);
        }

        public void RaiseComplete()
        {
            Complete?.Invoke(this, EventArgs.Empty);
        }

        protected void Invalidate(Control canvas, Rectangle roi)
        {
            var bounds = GetBounds();
            var x = Math.Min(bounds.X, roi.X);
            var y = Math.Min(bounds.Y, roi.Y);
            var w = Math.Max(bounds.X + bounds.Width, roi.X + roi.Width) - x;
            var h = Math.Max(bounds.Y + bounds.Height, roi.Y + roi.Height) - y;
            var rect = new Rectangle(x, y, w, h);
            canvas.Invalidate(rect);
        }
    }
}
