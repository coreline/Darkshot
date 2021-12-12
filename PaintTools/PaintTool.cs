using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using Keyboard = System.Windows.Input.Keyboard;
using Key = System.Windows.Input.Key;

namespace Darkshot.PaintTools
{
    enum PaintToolType
    {
        Workarea,
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
        protected event KeyEventHandler KeyUp;
        protected event KeyPressEventHandler KeyPress;
        protected event MouseEventHandler MouseDown;
        protected event MouseEventHandler MouseMove;
        protected event MouseEventHandler MouseUp;

        public static PaintTool Create(PaintToolType func, Color color)
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
                    return new PaintToolMarker(color);
                default:
                    throw new NotImplementedException();
            }
        }

        public virtual Cursor GetDefaultCursor()
        {
            return Cursors.Default;
        }

        public virtual Rectangle GetBounds()
        {
            return NativeVirtualScreen.Bounds;
        }

        public void RaisePaint(Control sender, PaintEventArgs e)
        {
            if (Paint == null)
                return;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

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

        public void RaiseKeyUp(Control sender, KeyEventArgs e)
        {
            if (KeyUp == null)
                return;
            var bounds = GetBounds();
            KeyUp.Invoke(sender, e);
            Invalidate(sender, bounds);
        }

        public void RaiseKeyPress(Control sender, KeyPressEventArgs e)
        {
            if (KeyPress == null)
                return;
            var bounds = GetBounds();
            KeyPress.Invoke(sender, e);
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

        protected bool IsShiftDown()
        {
            return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        }

        protected bool IsCtrlDown()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        }
    }
}
