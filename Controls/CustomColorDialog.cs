using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Darkshot.Controls
{
    internal class CustomColorDialog : ColorDialog
    {
        #region private const
        //Windows Message Constants
        private const Int32 WM_INITDIALOG = 0x0110;

        //uFlag Constants
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint UFLAGS = SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW;
        #endregion

        #region private readonly
        //Windows Handle Constants
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private static readonly IntPtr HWND_TOP = new IntPtr(0);
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        #endregion

        #region private vars
        //Module vars
        private int _right;
        private int _bottom;
        #endregion

        #region private static methods imports
        //WinAPI definitions


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left; //Leftmost coordinate
            public int Top; //Top coordinate
            public int Right; //The rightmost coordinate
            public int Bottom; //The bottom coordinate
        }

        /// <summary>
        /// Sets the window pos.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <param name="hWndInsertAfter">The h WND insert after.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="cx">The cx.</param>
        /// <param name="cy">The cy.</param>
        /// <param name="uFlags">The u flags.</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        #endregion

        #region public constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorDialogExtension"/> class.
        /// </summary>
        /// <param name="right">The X position</param>
        /// <param name="bottom">The Y position</param>
        /// <param name="title">The title of the windows. If set to null(by default), the title will not be changed</param>
        public CustomColorDialog(int right, int bottom)
        {
            FullOpen = false;
            _right = right;
            _bottom = bottom;
        }
        #endregion

        #region protected override methods
        /// <summary>
        /// Defines the common dialog box hook procedure that is overridden to add specific functionality to a common dialog box.
        /// </summary>
        /// <param name="hWnd">The handle to the dialog box window.</param>
        /// <param name="msg">The message being received.</param>
        /// <param name="wparam">Additional information about the message.</param>
        /// <param name="lparam">Additional information about the message.</param>
        /// <returns>
        /// A zero value if the default dialog box procedure processes the message; a nonzero value if the default dialog box procedure ignores the message.
        /// </returns>
        protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            IntPtr hookProc = base.HookProc(hWnd, msg, wparam, lparam);
            if (msg != WM_INITDIALOG)
                return hookProc;

            RECT rect;
            GetWindowRect(hWnd, out rect);

            const int margin = 3;
            var w = rect.Right - rect.Left;
            var h = rect.Bottom - rect.Top;
            var x = NativeVirtualScreen.Bounds.X + _right - w;
            var y = NativeVirtualScreen.Bounds.Y + _bottom - h;
            x = Math.Max(x, NativeVirtualScreen.Bounds.X + margin);
            y = Math.Max(y, NativeVirtualScreen.Bounds.Y + margin);
            SetWindowPos(hWnd, HWND_TOP, x, y, 0, 0, UFLAGS);

            return hookProc;
        }
        #endregion

    }
}
