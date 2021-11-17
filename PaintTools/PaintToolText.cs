using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Darkshot.PaintTools
{
    class PaintToolText : IPaintTool
    {
        Point _pointStart;
        Brush _brush;
        Font _font;
        StringBuilder _text;
        bool _init;

        enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }
        [DllImport("user32.dll")]
        static extern bool GetKeyboardState(byte[] lpKeyState);
        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, MapType uMapType);
        [DllImport("user32.dll")]
        static extern int ToUnicode(
             uint wVirtKey,
             uint wScanCode,
             byte[] lpKeyState,
             [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
             StringBuilder pwszBuff,
             int cchBuff,
             uint wFlags);

        static char GetCharFromKey(int virtualKey)
        {
            char ch = ' ';

            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            uint scanCode = MapVirtualKey((uint)virtualKey, MapType.MAPVK_VK_TO_VSC);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            if (result == 0 || result == -1)
                return ch;
            return stringBuilder[0];
        }

        public PaintToolText(Color color)
        {
            _brush = new SolidBrush(color);
            _font = new Font(FontFamily.GenericSansSerif, 16, FontStyle.Regular, GraphicsUnit.Pixel);
            _pointStart = Point.Empty;
            _text = new StringBuilder();
            _text.Append("|");
        }
        Cursor IPaintTool.GetCursor()
        {
            return Cursors.IBeam;
        }

        bool IPaintTool.ProcessKeyDown(KeyEventArgs e)
        {
            if (!_init)
            {
                _text.Clear();
                _init = true;
            }
            if (e.KeyCode == Keys.Enter)
            {
                _text.AppendLine();
            }
            else if (e.KeyCode == Keys.Back)
            {
                if (_text.Length > 0)
                    _text.Remove(_text.Length - 1, 1);
            }
            else
            {
                var c = GetCharFromKey(e.KeyValue);
                if (Char.IsControl(c))
                    return false;
                _text.Append(c);
            }
            return true;
        }

        void IPaintTool.Paint(Graphics g)
        {
            if (_text.Length == 0)
                return;
            g.DrawString(_text.ToString(), _font, _brush, _pointStart);
        }

        bool IPaintTool.ProcessMouseDown(MouseEventArgs e)
        {
            _pointStart = e.Location;
            _pointStart.X = _pointStart.X + Cursor.Current.Size.Width / 5;
            _pointStart.Y = _pointStart.Y - Cursor.Current.Size.Height * 2 / 5;
            return true;
        }

        bool IPaintTool.ProcessMouseMove(MouseEventArgs e)
        {
            return false;
        }

        bool IPaintTool.ProcessMouseUp(MouseEventArgs e)
        {
            return false;
        }

        Rectangle IPaintTool.GetBounds()
        {
            return SystemInformation.VirtualScreen;
        }
    }
}
