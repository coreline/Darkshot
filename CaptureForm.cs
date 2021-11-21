using Darkshot.PaintTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using Darkshot.Gdi32;
using System.Threading;

namespace Darkshot
{
    public partial class CaptureForm : Form
    {
        Bitmap _bitmap;
        Color _color = Color.Red;
        Color _mark = Color.Yellow;
        PaintToolWorkarea _area;
        PaintToolType _toolType;
        PaintTool _tool;
        List<PaintTool> _actionsTodo = new List<PaintTool>();
        List<PaintTool> _actionsUndo = new List<PaintTool>();

        public Color CurrentColor
        {
            get
            {
                return _toolType == PaintToolType.Marker ? _mark : _color;
            }
            set
            {
                if (_toolType == PaintToolType.Marker)
                    _mark = value;
                else
                    _color = value;
            }
        }

        public CaptureForm()
        {
            InitializeCaptureBitmap();
            InitializeComponent();
#if DEBUG
            TopMost = false;
#endif
            Location = SystemInformation.VirtualScreen.Location;
            Size = SystemInformation.VirtualScreen.Size;
            ClientSize = SystemInformation.VirtualScreen.Size;
            MaximumSize = SystemInformation.VirtualScreen.Size;
            MinimumSize = SystemInformation.VirtualScreen.Size;

            RefreshColorIcon();
            Invalidate();

            toolsPaint.Visible = false;
            toolsPaint.Cursor = Cursors.Default;
            toolsApp.Visible = false;
            toolsApp.Cursor = Cursors.Default;

            toolExit.Click += (s, e) => Close();
            toolCopy.Click += (s, e) => CopyToClipboard();
            toolSave.Click += (s, e) => SaveAs();
            toolUndo.Click += (s, e) => Undo();
            toolRedo.Click += (s, e) => Redo();
        }

        void InitializeCaptureBitmap()
        {
            var rect = SystemInformation.VirtualScreen;
            _area = new PaintToolWorkarea();

            _bitmap = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(_bitmap))
                g.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
        }

        public bool IsPainting()
        {
            return GetPaintTool() != null && _toolType != PaintToolType.None;
        }

        public void CopyToClipboard()
        {
            GetPaintTool()?.RaiseComplete();
            if (_area.IsEmpty)
                return;
            using (var image = BuildImage(ImageFormat.Png))
                Clipboard.SetImage(image);
            Close();
        }

        public void SaveAs()
        {
            GetPaintTool()?.RaiseComplete();
            if (_area.IsEmpty)
                return;
            if (saveFileDialog.Tag?.ToString() == true.ToString())
                return;
            saveFileDialog.Tag = true.ToString();
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;
            if (saveFileDialog.FileName == string.Empty)
                return;
            try
            {
                using (var image = BuildImage(ImageFormat.Png))
                    image.Save(saveFileDialog.FileName);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            saveFileDialog.Tag = string.Empty;
            Close();
        }

        private Image BuildImage(ImageFormat format)
        {
            var roi = _area.Roi;
            if (roi.X < 0)
            {
                roi.Width = roi.Width + roi.X;
                roi.X = 0;
            }
            if (roi.Y < 0)
            {
                roi.Height = roi.Height + roi.Y;
                roi.Y = 0;
            }
            if (roi.X + roi.Width > _bitmap.Width)
            {
                roi.Width = _bitmap.Width - roi.X;
            }
            if (roi.Y + roi.Height > _bitmap.Height)
            {
                roi.Height = _bitmap.Height - roi.Y;
            }

            Image image;

            using (var form = new Form())
            {
                form.Opacity = 0;
                form.Location = new Point(-Location.X, -Location.Y);
                form.Size = Size;
                form.Visible = true;

                using (var g = form.CreateGraphics())
                    OnPaint(form, new PaintEventArgs(g, roi));
                using (var bitmap = form.CaptureControl(roi))
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, format);
                    image = Image.FromStream(stream);
                }
            }

            GC.Collect();
            return image;
        }

        public void Undo()
        {
            if (_actionsTodo.Count == 0 && _tool == null)
                return;
            if (_tool != null)
            {
                _actionsUndo.Add(_tool);
                _tool = null;
            }
            else
            {
                var i = _actionsTodo.Count - 1;
                _actionsUndo.Add(_actionsTodo[i]);
                _actionsTodo.RemoveAt(i);
            }
            Invalidate();
        }

        public void Redo()
        {
            if (_actionsUndo.Count == 0)
                return;
            var i = _actionsUndo.Count - 1;
            _actionsTodo.Add(_actionsUndo[i]);
            _actionsUndo.RemoveAt(i);
            Invalidate();
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            HideTools();
            GetPaintTool(true)?.RaiseMouseDown(this, e);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            GetPaintTool()?.RaiseMouseMove(this, e);
            DisplayTools(e);
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            GetPaintTool()?.RaiseMouseUp(this, e);
            DisplayTools(e);
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Black);
            e.Graphics.DrawImageUnscaled(_bitmap, Point.Empty);
            foreach (var item in _actionsTodo)
                item.RaisePaint(this, e);
            _tool?.RaisePaint(this, e);
            if (sender == this)
                _area.RaisePaint(this, e);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            GetPaintTool()?.RaiseKeyDown(this, e);
        }

        private void OnToolColorClick(object sender, EventArgs e)
        {
            colorDialog.Color = CurrentColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
                CurrentColor = colorDialog.Color;
            RefreshColorIcon();
        }

        private void OnToolPaintClick(object sender, EventArgs e)
        {
            GetPaintTool()?.RaiseComplete();
            if (string.IsNullOrEmpty(((ToolStripItem)sender).Tag?.ToString()))
                return;
            foreach (var item in toolsPaint.Items)
            {
                if (item == sender)
                    continue;
                if (string.IsNullOrEmpty(((ToolStripItem)item).Tag?.ToString()))
                    continue;
                (item as ToolStripButton).Checked = false;
            }
            var func = sender as ToolStripButton;
            func.Checked = !func.Checked;
            _toolType = func.Checked ? (PaintToolType)func.Tag : PaintToolType.None;
            Cursor = PaintTool.GetDefaultCursor(_toolType);
            RefreshColorIcon();
        }

        private void RefreshColorIcon()
        {
            using (var g = Graphics.FromImage(toolColor.Image))
            {
                using (var brush = new SolidBrush(CurrentColor))
                    g.FillRectangle(brush, new Rectangle(0, 0, 32, 32));
                toolColor.Invalidate();
            }
        }

        private void DisplayTools(MouseEventArgs e)
        {
            if (_area.IsEmpty)
            {
                toolTipSelRegion.ShowAlways = true;
                toolTipSelRegion.Show("Выберите область", this, e.X + 10, e.Y + 10);
                toolsPaint.Visible = false;
                toolsApp.Visible = false;
                return;
            }

            if (!_area.IsCreating && !toolsPaint.Visible && _tool == null)
            {
                int margin = 7;

                var paintLeft = _area.Roi.X + _area.Roi.Width + margin;
                paintLeft = Math.Max(paintLeft, margin);
                paintLeft = Math.Min(paintLeft, _bitmap.Width - toolsPaint.Width - margin);

                var appLeft = _area.Roi.X + _area.Roi.Width - toolsApp.Width;
                appLeft = Math.Min(appLeft, paintLeft - toolsApp.Width - margin);
                appLeft = Math.Max(appLeft, margin);

                var appTop = _area.Roi.Y + _area.Roi.Height + margin;
                appTop = Math.Min(appTop, _bitmap.Height - toolsApp.Height - margin);
                appTop = Math.Max(appTop, margin);

                var paintTop = _area.Roi.Y + _area.Roi.Height - toolsPaint.Height;
                paintTop = Math.Min(paintTop, appTop - toolsPaint.Height - margin);
                paintTop = Math.Max(paintTop, margin);

                if (paintLeft < appLeft + toolsApp.Width + margin &&
                    appTop < paintTop + toolsPaint.Height + margin)
                    appLeft = paintLeft + toolsPaint.Width + margin;

                toolsPaint.Location = new Point(paintLeft, paintTop);
                toolsPaint.Visible = true;

                toolsApp.Location = new Point(appLeft, appTop);
                toolsApp.Visible = true;
            }
        }
        private void HideTools()
        {
            if (toolTipSelRegion != null)
            {
                toolTipSelRegion.Dispose();
                toolTipSelRegion = null;
            }
            if (toolsPaint.Visible)
                toolsPaint.Visible = false;
            if (toolsApp.Visible)
                toolsApp.Visible = false;
        }

        private PaintTool GetPaintTool(bool createIfNeed = false)
        {
            if (_toolType == PaintToolType.None)
                return _area;
            if (_tool == null && createIfNeed)
            {
                _tool = PaintTool.Create(_toolType, _color, _mark);
                _tool.Complete += (s, e) =>
                {
                    _actionsTodo.Add(_tool);
                    _actionsUndo.Clear();
                    _tool = null;
                    Cursor = PaintTool.GetDefaultCursor(_toolType);
                };
            }
            return _tool;
        }
    }
}
