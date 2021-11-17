using Darkshot.PaintTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Darkshot
{
    public partial class CaptureForm : Form
    {
        Bitmap _bitmap;
        Color _color = Color.Red;
        Color _mark = Color.Yellow;
        PaintToolWorkarea _area;
        PaintToolType _toolType;
        Rectangle _activeRegion;
        IPaintTool _tool;
        List<IPaintTool> _actionsTodo = new List<IPaintTool>();
        List<IPaintTool> _actionsUndo = new List<IPaintTool>();

        public bool IsTextTool { get { return _toolType == PaintToolType.Text; } }
        public bool IsMarkerTool { get { return _toolType == PaintToolType.Marker; } }
        public Color CurrentColor
        {
            get { return IsMarkerTool ? _mark : _color; }
            set { if (IsMarkerTool) _mark = value; else _color = value; }
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
            toolsApp.Visible = false;

            toolExit.Click += (s, e) => Close();
            toolCopy.Click += (s, e) => CopyToClipboard();
            toolSave.Click += (s, e) => SaveAs();
            toolUndo.Click += (s, e) => Undo();
            toolRedo.Click += (s, e) => Redo();
        }

        void InitializeCaptureBitmap()
        {
            var rect = SystemInformation.VirtualScreen;
            _activeRegion = rect;
            _area = new PaintToolWorkarea();

            _bitmap = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppRgb);
            using (var g = Graphics.FromImage(_bitmap))
                g.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
        }

        public void CopyToClipboard()
        {
            PushPaintAction();
            if (_area.IsEmpty)
                return;
            using (var image = BuildImage(ImageFormat.Png))
                Clipboard.SetImage(image);
            Close();
        }

        public void SaveAs()
        {
            PushPaintAction();
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

            using (var raw = _bitmap.Clone() as Bitmap)
            {
                using (var g = Graphics.FromImage(raw))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    foreach (var item in _actionsTodo)
                        item.Paint(g);
                }
                using (var bitmap = new Bitmap(roi.Width, roi.Height))
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(raw, 0, 0, roi, GraphicsUnit.Pixel);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        bitmap.Save(stream, format);
                        return Image.FromStream(stream);
                    }
                }
            }
        }

        public bool Undo()
        {
            if (_actionsTodo.Count == 0 && _tool == null)
                return false;
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
            Render();
            return true;
        }

        public bool Redo()
        {
            if (_actionsUndo.Count == 0)
                return false;
            var i = _actionsUndo.Count - 1;
            _actionsTodo.Add(_actionsUndo[i]);
            _actionsUndo.RemoveAt(i);
            Render();
            return true;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (IsTextTool)
                PushPaintAction();
            else
                HideTools();
            if (GetPaintTool(true)?.ProcessMouseDown(e) == true)
                Render();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            DisplayTools(e);
            var tool = GetPaintTool(false);
            if (tool?.ProcessMouseMove(e) == true)
                Render();
            this.Cursor = tool?.GetCursor() ?? Cursors.Default;
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            DisplayTools(e);
            if (GetPaintTool(false)?.ProcessMouseUp(e) == true)
                Render();
            if (!IsTextTool)
                PushPaintAction();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            var roi = _area.Roi;
            var size = _bitmap.Size;
            var darkRegions = new Rectangle[]
            {
                new Rectangle(0, 0, size.Width, roi.Y),
                new Rectangle(0, roi.Y, roi.X, roi.Height),
                new Rectangle(roi.X + roi.Width, roi.Y, size.Width - roi.X + roi.Width, roi.Height),
                new Rectangle(0, roi.Y + roi.Height, size.Width, size.Height - roi.Y - roi.Height)
            };
            e.Graphics.Clear(Color.Black);
            e.Graphics.DrawImageUnscaled(_bitmap, Point.Empty);
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            foreach (var item in _actionsTodo)
                item.Paint(e.Graphics);
            _tool?.Paint(e.Graphics);
            e.Graphics.SmoothingMode = SmoothingMode.Default;
            e.Graphics.InterpolationMode = InterpolationMode.Default;
            e.Graphics.CompositingQuality = CompositingQuality.Default;
            using (var brush = new SolidBrush(Color.FromArgb(127, Color.Black)))
                e.Graphics.FillRectangles(brush, darkRegions);
            _area.Paint(e.Graphics);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_area.IsEmpty)
                return;

            if (GetPaintTool(false)?.ProcessKeyDown(e) == true)
                Render();
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
            PushPaintAction();
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
            RefreshColorIcon();
        }

        private void Render()
        {
            IPaintTool tool;
            tool = GetPaintTool(false);
            if (tool == null || IsTextTool)
            {
                Invalidate();
                _activeRegion = SystemInformation.VirtualScreen;
                return;
            }
            var bounds = tool.GetBounds();
            var x = Math.Min(bounds.X, _activeRegion.X);
            var y = Math.Min(bounds.Y, _activeRegion.Y);
            var w = Math.Max(bounds.X + bounds.Width, _activeRegion.X + _activeRegion.Width) - x;
            var h = Math.Max(bounds.Y + bounds.Height, _activeRegion.Y + _activeRegion.Height) - y;
            var rect = new Rectangle(x, y, w, h);
            Invalidate(rect);
            _activeRegion = bounds;
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
                toolTipSelRegion.Show("Выберите область", this, e.X, e.Y + 40);
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

        private IPaintTool GetPaintTool(bool createIfNeed)
        {
            if (_toolType == PaintToolType.None)
                return _area;
            if (_tool == null && createIfNeed)
                _tool = PaintToolFactory.Create(_toolType, _color, _mark);
            return _tool;
        }

        private void PushPaintAction()
        {
            if (_tool == null)
                return;
            _actionsTodo.Add(_tool);
            _tool = null;
            _actionsUndo.Clear();
        }
    }
}
