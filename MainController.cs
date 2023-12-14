using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;

namespace Darkshot
{
    class MainController : ApplicationContext, IDisposable
    {
        NotifyIcon _trayIcon;
        GlobalKeyboardHook _globalKeyboardHook;
        bool _isCaptured;
        CaptureForm _captureForm;
        Stopwatch _keyboardTimer;

        public MainController()
        {
            _keyboardTimer = new Stopwatch();
            _keyboardTimer.Start();
            _isCaptured = false;
            _globalKeyboardHook = new GlobalKeyboardHook();
            _globalKeyboardHook.KeyboardPressed += OnKeyPressed;
            _trayIcon = new NotifyIcon()
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Сделать скриншот", Capture),
                    new MenuItem("О программе", About),
                    new MenuItem("Закрыть", Exit)
                }),
                Visible = true,
                Text = Assembly.GetExecutingAssembly().GetName().Name
            };
            _trayIcon.DoubleClick += Capture;
        }

        private void Capture(object sender, EventArgs e)
        {
            var rectNative = NativeVirtualScreen.Bounds;
            var rectVirtual = SystemInformation.VirtualScreen;
            _isCaptured = true;
            _captureForm = new CaptureForm();
            _captureForm.ShowDialog();
            _captureForm.Dispose();
            _captureForm = null;
            _isCaptured = false;
            GC.Collect();
        }


        void About(object sender, EventArgs e)
        {
            var position = System.Windows.Forms.Cursor.Position;
            var screen = Screen.FromPoint(position);
            using (var form = new AboutForm(screen))
                form.ShowDialog();
            GC.Collect();
        }

        void Exit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            _globalKeyboardHook.Dispose();
            _captureForm?.Close();
            Application.Exit();
        }

        void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            if (_keyboardTimer.ElapsedMilliseconds < 70)
                return;

            if (_isCaptured)
            {
                if (_captureForm?.IsPainting() == true && !Keyboard.IsKeyDown(Key.Escape))
                    return;
                if (Keyboard.IsKeyDown(Key.Escape))
                    _captureForm?.Close();
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.C))
                    _captureForm?.CopyToClipboard();
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.S))
                    _captureForm?.SaveAs();
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Z))
                    _captureForm?.Undo();
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Y))
                    _captureForm?.Redo();
                else
                    return;
                GC.Collect();
                _keyboardTimer.Restart();
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.OemTilde))
                    Capture(sender, e);
                if (Keyboard.IsKeyDown(Key.LWin) && Keyboard.IsKeyDown(Key.Escape))
                    Capture(sender, e);
                else if (Keyboard.IsKeyDown(Key.PrintScreen))
                    Capture(sender, e);
            }
        }

    }
}