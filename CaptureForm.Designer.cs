using Darkshot.PaintTools;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Darkshot
{
    partial class CaptureForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CaptureForm));
            this.toolTipSelRegion = new System.Windows.Forms.ToolTip(this.components);
            this.toolsPaint = new System.Windows.Forms.ToolStrip();
            this.toolColor = new System.Windows.Forms.ToolStripButton();
            this.toolCopy = new System.Windows.Forms.ToolStripButton();
            this.toolExit = new System.Windows.Forms.ToolStripButton();
            this.toolsApp = new System.Windows.Forms.ToolStrip();
            this.toolUndo = new System.Windows.Forms.ToolStripButton();
            this.toolRedo = new System.Windows.Forms.ToolStripButton();
            this.toolSave = new System.Windows.Forms.ToolStripButton();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.toolPen = new System.Windows.Forms.ToolStripButton();
            this.toolLine = new System.Windows.Forms.ToolStripButton();
            this.toolArrow = new System.Windows.Forms.ToolStripButton();
            this.toolRectangle = new System.Windows.Forms.ToolStripButton();
            this.toolFill = new System.Windows.Forms.ToolStripButton();
            this.toolText = new System.Windows.Forms.ToolStripButton();
            this.toolMarker = new System.Windows.Forms.ToolStripButton();
            this.toolsPaint.SuspendLayout();
            this.toolsApp.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolsPaint
            // 
            this.toolsPaint.Dock = System.Windows.Forms.DockStyle.None;
            this.toolsPaint.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolsPaint.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolPen,
            this.toolLine,
            this.toolArrow,
            this.toolRectangle,
            this.toolFill,
            this.toolText,
            this.toolMarker,
            this.toolColor});
            this.toolsPaint.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Table;
            this.toolsPaint.Location = new System.Drawing.Point(9, 36);
            this.toolsPaint.Name = "toolsPaint";
            this.toolsPaint.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolsPaint.Size = new System.Drawing.Size(25, 216);
            this.toolsPaint.TabIndex = 0;
            // 
            // toolColor
            // 
            this.toolColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolColor.Image = ((System.Drawing.Image)(resources.GetObject("toolColor.Image")));
            this.toolColor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolColor.Name = "toolColor";
            this.toolColor.Size = new System.Drawing.Size(24, 24);
            this.toolColor.Text = "Цвет";
            this.toolColor.Click += new System.EventHandler(this.OnToolColorClick);
            // 
            // toolCopy
            // 
            this.toolCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolCopy.Image = ((System.Drawing.Image)(resources.GetObject("toolCopy.Image")));
            this.toolCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolCopy.Name = "toolCopy";
            this.toolCopy.Size = new System.Drawing.Size(24, 24);
            this.toolCopy.Text = "Копировать (Ctrl + C)";
            // 
            // toolExit
            // 
            this.toolExit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolExit.Image = ((System.Drawing.Image)(resources.GetObject("toolExit.Image")));
            this.toolExit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolExit.Name = "toolExit";
            this.toolExit.Size = new System.Drawing.Size(24, 24);
            this.toolExit.Text = "Выход (Esc)";
            // 
            // toolsApp
            // 
            this.toolsApp.Dock = System.Windows.Forms.DockStyle.None;
            this.toolsApp.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolsApp.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolUndo,
            this.toolRedo,
            this.toolCopy,
            this.toolSave,
            this.toolExit});
            this.toolsApp.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolsApp.Location = new System.Drawing.Point(9, 9);
            this.toolsApp.Name = "toolsApp";
            this.toolsApp.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolsApp.Size = new System.Drawing.Size(121, 27);
            this.toolsApp.TabIndex = 1;
            // 
            // toolUndo
            // 
            this.toolUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolUndo.Image = ((System.Drawing.Image)(resources.GetObject("toolUndo.Image")));
            this.toolUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolUndo.Name = "toolUndo";
            this.toolUndo.Size = new System.Drawing.Size(24, 24);
            this.toolUndo.Text = "Отменить (Ctrl + Z)";
            // 
            // toolRedo
            // 
            this.toolRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolRedo.Image = ((System.Drawing.Image)(resources.GetObject("toolRedo.Image")));
            this.toolRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolRedo.Name = "toolRedo";
            this.toolRedo.Size = new System.Drawing.Size(24, 24);
            this.toolRedo.Text = "Вернуть (Ctrl + Y)";
            // 
            // toolSave
            // 
            this.toolSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolSave.Image = ((System.Drawing.Image)(resources.GetObject("toolSave.Image")));
            this.toolSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolSave.Name = "toolSave";
            this.toolSave.Size = new System.Drawing.Size(24, 24);
            this.toolSave.Text = "Сохранить (Ctrl + S)";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "Изображение в формате PNG (*.png)|*.png";
            // 
            // toolPen
            // 
            this.toolPen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolPen.Image = ((System.Drawing.Image)(resources.GetObject("toolPen.Image")));
            this.toolPen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolPen.Name = "toolPen";
            this.toolPen.Size = new System.Drawing.Size(24, 24);
            this.toolPen.Tag = Darkshot.PaintTools.PaintToolType.Pen;
            this.toolPen.Text = "Карандаш";
            this.toolPen.Click += new System.EventHandler(this.OnToolPaintClick);
            // 
            // toolLine
            // 
            this.toolLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolLine.Image = ((System.Drawing.Image)(resources.GetObject("toolLine.Image")));
            this.toolLine.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolLine.Name = "toolLine";
            this.toolLine.Size = new System.Drawing.Size(24, 24);
            this.toolLine.Tag = Darkshot.PaintTools.PaintToolType.Line;
            this.toolLine.Text = "Линия";
            this.toolLine.Click += new System.EventHandler(this.OnToolPaintClick);
            // 
            // toolArrow
            // 
            this.toolArrow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolArrow.Image = ((System.Drawing.Image)(resources.GetObject("toolArrow.Image")));
            this.toolArrow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolArrow.Name = "toolArrow";
            this.toolArrow.Size = new System.Drawing.Size(24, 24);
            this.toolArrow.Tag = Darkshot.PaintTools.PaintToolType.Arrow;
            this.toolArrow.Text = "Стрелка";
            this.toolArrow.Click += new System.EventHandler(this.OnToolPaintClick);
            // 
            // toolRectangle
            // 
            this.toolRectangle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolRectangle.Image = ((System.Drawing.Image)(resources.GetObject("toolRectangle.Image")));
            this.toolRectangle.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolRectangle.Name = "toolRectangle";
            this.toolRectangle.Size = new System.Drawing.Size(24, 24);
            this.toolRectangle.Tag = Darkshot.PaintTools.PaintToolType.Rectangle;
            this.toolRectangle.Text = "Прямоугольник";
            this.toolRectangle.Click += new System.EventHandler(this.OnToolPaintClick);
            // 
            // toolFill
            // 
            this.toolFill.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolFill.Image = ((System.Drawing.Image)(resources.GetObject("toolFill.Image")));
            this.toolFill.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolFill.Name = "toolFill";
            this.toolFill.Size = new System.Drawing.Size(24, 24);
            this.toolFill.Tag = Darkshot.PaintTools.PaintToolType.FilledRectangle;
            this.toolFill.Text = "Залитый прямоугольник";
            this.toolFill.Click += new System.EventHandler(this.OnToolPaintClick);
            // 
            // toolText
            // 
            this.toolText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolText.Image = ((System.Drawing.Image)(resources.GetObject("toolText.Image")));
            this.toolText.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolText.Name = "toolText";
            this.toolText.Size = new System.Drawing.Size(24, 24);
            this.toolText.Tag = Darkshot.PaintTools.PaintToolType.Text;
            this.toolText.Text = "Текст";
            this.toolText.Click += new System.EventHandler(this.OnToolPaintClick);
            // 
            // toolMarker
            // 
            this.toolMarker.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolMarker.Image = ((System.Drawing.Image)(resources.GetObject("toolMarker.Image")));
            this.toolMarker.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolMarker.Name = "toolMarker";
            this.toolMarker.Size = new System.Drawing.Size(24, 24);
            this.toolMarker.Tag = Darkshot.PaintTools.PaintToolType.Marker;
            this.toolMarker.Text = "Маркер";
            this.toolMarker.Click += new System.EventHandler(this.OnToolPaintClick);
            // 
            // CaptureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(489, 361);
            this.Controls.Add(this.toolsApp);
            this.Controls.Add(this.toolsPaint);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CaptureForm";
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Screenshot";
            this.TopMost = true;
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnKeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMouseUp);
            this.toolsPaint.ResumeLayout(false);
            this.toolsPaint.PerformLayout();
            this.toolsApp.ResumeLayout(false);
            this.toolsApp.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTipSelRegion;
        private System.Windows.Forms.ToolStrip toolsPaint;
        private System.Windows.Forms.ToolStripButton toolPen;
        private System.Windows.Forms.ToolStripButton toolLine;
        private System.Windows.Forms.ToolStripButton toolArrow;
        private System.Windows.Forms.ToolStripButton toolRectangle;
        private System.Windows.Forms.ToolStripButton toolFill;
        private System.Windows.Forms.ToolStripButton toolText;
        private System.Windows.Forms.ToolStripButton toolCopy;
        private System.Windows.Forms.ToolStripButton toolExit;
        private System.Windows.Forms.ToolStripButton toolMarker;
        private System.Windows.Forms.ToolStripButton toolColor;
        private ToolStrip toolsApp;
        private ToolStripButton toolUndo;
        private ToolStripButton toolRedo;
        private ToolStripButton toolSave;
        private SaveFileDialog saveFileDialog;
    }
}

