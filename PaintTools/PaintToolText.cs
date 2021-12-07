using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Darkshot.PaintTools
{
    class PaintToolText : PaintTool
    {
        class CanvasTextBox : TextBox
        {
            public CanvasTextBox()
            {
                BorderStyle = BorderStyle.None;
                Font = new Font(FontFamily.GenericSansSerif, 20, FontStyle.Regular, GraphicsUnit.Pixel);
                Location = Point.Empty;
                Margin = new Padding(0);
                Multiline = true;
                Size = Size.Empty;
                WordWrap = false;
                ShortcutsEnabled = true;
            }
            protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            {
                if (keyData == (Keys.Back | Keys.Control))
                {
                    for (int i = this.SelectionStart - 1; i > 0; i--)
                    {
                        switch (Text.Substring(i, 1))
                        {    //set up any stopping points you want
                            case " ":
                            case ";":
                            case ",":
                            case "/":
                            case "\\":
                                Text = Text.Remove(i, SelectionStart - i);
                                SelectionStart = i;
                                return true;
                            case "\n":
                                Text = Text.Remove(i - 1, SelectionStart - i);
                                SelectionStart = i;
                                return true;
                        }
                    }
                    Clear();        //in case you never hit a stopping point, the whole textbox goes blank
                    return true;
                }
                else
                {
                    return base.ProcessCmdKey(ref msg, keyData);
                }
            }
        }

        const int PEN_WIDTH = 2;
        Pen _penFirst;
        Pen _penSecond;
        Color _color;
        bool _completed = false;
        bool _initialized = false;
        bool _resizing = false;
        bool _moving = false;
        Point _startPoint;
        Point _movePoint;
        Control _canvas;
        string _text;
        Font _font;
        Rectangle _rect;
        Rectangle _boundsDash;
        Rectangle _boundsMove;
        CanvasTextBox _textBox;
        Size _defaultSize;
        Size _userSize;

        public PaintToolText(Color color)
        {
            Paint += (s, e) => { onPaint(e.Graphics); };
            MouseDown += (s, e) => { onMouseDown(s as Control, e); };
            MouseMove += (s, e) => { onMouseMove(s as Control, e); };
            MouseUp += (s, e) => { onMouseUp(s as Control, e); };
            KeyDown += (s, e) => { onKeyDown(s as Control, e); };
            Complete += (s, e) => { onComplete(); };

            _color = color;
            var dashWidth = 4f;
            _penFirst = new Pen(Color.Black, PEN_WIDTH)
            {
                DashPattern = new float[] { dashWidth, dashWidth }
            };
            _penSecond = new Pen(Color.White, PEN_WIDTH)
            {
                DashOffset = dashWidth,
                DashPattern = new float[] { dashWidth, dashWidth }
            };
            _textBox = new CanvasTextBox();
            _textBox.TextChanged += (s, e) => { onTextChanged(s, e); };
            _textBox.ForeColor = color;
            _font = _textBox.Font;
            _defaultSize = new Size(200, _font.Height * 3);
        }

        void onKeyDown(Control canvas, KeyEventArgs e)
        {
        }

        void onPaint(Graphics g)
        {
            if (_initialized && !_completed)
            {
                g.DrawRectangle(_penFirst, _boundsDash);
                g.DrawRectangle(_penSecond, _boundsDash);
                return;
            }
            if (string.IsNullOrWhiteSpace(_text))
                return;
            TextFormatFlags flags = TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
            TextRenderer.DrawText(g, _text, _font, _rect, _color, flags);
            //g.DrawString(_text, _font, Brushes.Red, _rect, StringFormat.GenericTypographic);
        }

        void onMouseDown(Control canvas, MouseEventArgs e)
        {
            if (!_initialized)
            {
                _canvas = canvas;
                _initialized = true;
                _resizing = true;
                _startPoint = e.Location;
                _textBox.Location = _startPoint;
                _canvas.Controls.Add(_textBox);
                _textBox.Focus();
                return;
            }
            if (IsMovablePosition(e.Location)) 
            {
                _moving = true;
                _movePoint = e.Location;
                return;
            }
            RaiseComplete();
        }

        void onMouseMove(Control canvas, MouseEventArgs e)
        {
            var point = e.Location;
            if (!_initialized)
            {
                canvas.Cursor = Cursors.IBeam;
                return;
            }
            if (_resizing)
            {
                _textBox.Location = new Point(Math.Min(_startPoint.X, point.X), Math.Min(_startPoint.Y, point.Y));
                _textBox.Size = new Size(Math.Abs(_startPoint.X - point.X), Math.Abs(_startPoint.Y - point.Y));
                RecalcBounds();
                return;
            }
            if (_moving)
            {
                var x = _startPoint.X + point.X - _movePoint.X;
                var y = _startPoint.Y + point.Y - _movePoint.Y;
                _textBox.Location = new Point(x, y);
                RecalcBounds();
                return;
            }

            canvas.Cursor = IsMovablePosition(point) ? Cursors.SizeAll : Cursors.Default;
        }

        void onMouseUp(Control canvas, MouseEventArgs e)
        {
            if (_resizing)
            {
                _resizing = false;
                _startPoint = _textBox.Location;
                if (_textBox.Width < _defaultSize.Width)
                    _textBox.Width = _defaultSize.Width;
                if (_textBox.Height < _defaultSize.Height)
                    _textBox.Height = _defaultSize.Height;
                _userSize = _textBox.Size;
                RecalcBounds();
                return;
            }
            if (_moving)
            {
                _moving = false;
                _startPoint = _textBox.Location;
                RecalcBounds();
                return;
            }
            if (_initialized)
            {
                RaiseComplete();
                return;
            }
        }
        void onTextChanged(object sender, EventArgs e)
        {
            var margin = 10;
            Size size = TextRenderer.MeasureText(_textBox.Text, _textBox.Font);
            size.Width = Math.Max(size.Width + margin, _userSize.Width);
            size.Height = Math.Max(size.Height + margin, _userSize.Height);
            if (_textBox.Size == size)
                return;
            _textBox.Size = size;
            var roi = new Rectangle(Point.Empty, SystemInformation.VirtualScreen.Size);
            Invalidate(_canvas, roi);
            RecalcBounds();
        }

        void onComplete()
        {
            _completed = true;
            _text = _textBox.Text;
            _canvas?.Controls.Remove(_textBox);
            _textBox.Dispose();
        }

        public override Rectangle GetBounds()
        {
            var bounds = _boundsDash.IsEmpty
                ? new Rectangle(_textBox.Location, _textBox.Size)
                : _boundsDash;
            bounds.Inflate(10 + 2 * PEN_WIDTH, 10 + 2 * PEN_WIDTH);
            return bounds;
        }

        public override Cursor GetDefaultCursor()
        {
            return Cursors.IBeam;
        }

        void RecalcBounds()
        {
            _rect = new Rectangle(_textBox.Location, _textBox.Size);
            _boundsDash = new Rectangle(_rect.X - PEN_WIDTH,
                                     _rect.Y - PEN_WIDTH,
                                     _rect.Width + 2 * PEN_WIDTH - 1,
                                     _rect.Height + 2 * PEN_WIDTH - 1);
            _boundsMove = _boundsDash;
            _boundsMove.Inflate(10, 10);
        }

        bool IsPointInRect(Rectangle rect, Point point)
        {
            return point.X >= rect.X && point.X <= rect.X + rect.Width
                && point.Y >= rect.Y && point.Y <= rect.Y + rect.Height;
        }

        bool IsMovablePosition(Point point)
        {
            return IsPointInRect(_boundsMove, point) && !IsPointInRect(_rect, point);
        }
    }
}
