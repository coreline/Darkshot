using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Darkshot
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var scale = (int)(100 * Screen.PrimaryScreen.Bounds.Width
                                  / System.Windows.SystemParameters.PrimaryScreenWidth);
            if (scale < 1) // Для инициализации масштаба, иначе разрешение формы не корректное
                new Exception("Wrong screen scale");

            var assembly = typeof(Program).Assembly;
            var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            var appGuid = attribute.Value;

            using (var mutex = new Mutex(false, "Global\\" + appGuid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("Приложение уже запущено", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainController());
            }

        }
    }
}
