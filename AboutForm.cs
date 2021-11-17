using System;
using System.Drawing;
using System.Windows.Forms;

namespace Darkshot
{
    public partial class AboutForm : Form
    {
        public AboutForm(Screen screen)
        {
            InitializeComponent();

            var x = screen.Bounds.X + (screen.Bounds.Width - Width) / 2;
            var y = screen.Bounds.Y + (screen.Bounds.Height - Height) / 2;
            Location = new Point(x, y);
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            textBoxDescription.SelectionStart = textBoxDescription.Text.Length;
            textBoxDescription.SelectionLength = 0;
            pictureBoxIcon.Image = Icon.ExtractAssociatedIcon(Application.ExecutablePath).ToBitmap();
            pictureBoxIcon.Focus();
        }
    }
}
