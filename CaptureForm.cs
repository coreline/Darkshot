﻿using Darkshot.PaintTools;
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
using Darkshot.Controls;
using System.Linq;

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
            Location = NativeVirtualScreen.Bounds.Location;
            Size = NativeVirtualScreen.Bounds.Size;
            ClientSize = NativeVirtualScreen.Bounds.Size;
            MaximumSize = NativeVirtualScreen.Bounds.Size;
            MinimumSize = NativeVirtualScreen.Bounds.Size;

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
            var scale = (int)(100 * Screen.PrimaryScreen.Bounds.Width
                                  / System.Windows.SystemParameters.PrimaryScreenWidth);
            if (scale < 1) // Для инициализации масштаба, иначе разрешение формы не корректное
                new Exception("Wrong screen scale");
            var rect = NativeVirtualScreen.Bounds;
            _area = new PaintToolWorkarea();

            _bitmap = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(_bitmap))
                g.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
        }

        public bool IsPainting()
        {
            return GetPaintTool() != null && _toolType != PaintToolType.Workarea;
        }

        public void CopyToClipboard()
        {
            GetPaintTool()?.RaiseComplete(this);
            if (_area.IsEmpty)
                return;
            using (var image = BuildImage(ImageFormat.Png))
                Clipboard.SetImage(image);
            Close();
        }

        public void SaveAs()
        {
            GetPaintTool()?.RaiseComplete(this);
            if (_area.IsEmpty)
                return;
            if (saveFileDialog.Tag?.ToString() == true.ToString())
                return;

            saveFileDialog.Tag = true.ToString();
            var dialogResult = saveFileDialog.ShowDialog();
            saveFileDialog.Tag = string.Empty;

            if (dialogResult != DialogResult.OK)
                return;
            if (string.IsNullOrEmpty(saveFileDialog.FileName))
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
            Close();
        }

        private Image BuildImage(ImageFormat format)
        {
            return BuildImageRoi(format, _area.Roi);
        }

        private Image BuildImageRoi(ImageFormat format, Rectangle roi)
        {
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
                form.Visible = true;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Location = this.Location;
                form.Size = this.Size;
                form.ClientSize = this.Size;
                form.MaximumSize = this.Size;
                form.MinimumSize = this.Size;

                using (var g = form.CreateGraphics())
                {
                    g.DrawImageUnscaled(_bitmap, Point.Empty);
                    foreach (var item in _actionsTodo)
                        item.RaisePaint(this, new PaintEventArgs(g, roi));
                    _tool?.RaisePaint(this, new PaintEventArgs(g, roi));
                }

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

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            GetPaintTool()?.RaiseKeyDown(this, e);
            RefreshToolsLocation();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            GetPaintTool()?.RaiseKeyUp(this, e);
            RefreshToolsLocation();
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            GetPaintTool()?.RaiseKeyPress(this, e);
            RefreshToolsLocation();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            PaintTool.Initialize();
            e.Graphics.Clear(Color.Black);
            e.Graphics.DrawImageUnscaled(_bitmap, Point.Empty);
            foreach (var item in _actionsTodo)
                item.RaisePaint(this, e);
            _tool?.RaisePaint(this, e);
            _area.RaisePaint(this, e);
            SetDefaultCursor();
        }

        private void OnToolPaintClick(object sender, EventArgs e)
        {
            GetPaintTool()?.RaiseComplete(this);
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
            _toolType = func.Checked ? (PaintToolType)func.Tag : PaintToolType.Workarea;
            SetDefaultCursor();
            RefreshColorIcon();
        }

        private void OnToolColorClick(object sender, EventArgs e)
        {
            var margin = 3;
            var right = toolsPaint.Left - margin;
            var bottom = toolsApp.Top - margin;

            using (var dialog = new CustomColorDialog(right, bottom))
            {
                dialog.Color = CurrentColor;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    CurrentColor = dialog.Color;
                    SetDefaultCursor();
                }
            }
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
                toolTipSelRegion.Show("Выберите область", this, e.X + 15, e.Y + 30);
                toolsPaint.Visible = false;
                toolsApp.Visible = false;
                return;
            }

            if (!_area.IsCreating && !toolsPaint.Visible && _tool == null)
            {
                RefreshToolsLocation();

                toolsPaint.Visible = true;
                toolsApp.Visible = true;
            }
        }

        private void RefreshToolsLocation()
        {
            const int margin = 7;

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

            var paintToolsLocation = new Point(paintLeft, paintTop);
            if (toolsPaint.Location != paintToolsLocation)
                toolsPaint.Location = paintToolsLocation;

            var appToolsLocation = new Point(appLeft, appTop);
            if (toolsApp.Location != appToolsLocation)
                toolsApp.Location = appToolsLocation;
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
            if (_toolType == PaintToolType.Workarea)
                return _area;
            if (_tool == null && createIfNeed)
            {
                _tool = CreatePaintTool();
                _tool.Complete += (s, e) =>
                {
                    _actionsTodo.Add(_tool);
                    _actionsUndo.Clear();
                    _tool = null;
                    SetDefaultCursor();
                };
            }
            return _tool;
        }

        private void SetDefaultCursor()
        {
            Cursor = _toolType == PaintToolType.Workarea
                ? Cursors.Default
                : CreatePaintTool().GetDefaultCursor();
        }

        private PaintTool CreatePaintTool()
        {
            return PaintTool.Create(_toolType, CurrentColor);
        }
    }
}
