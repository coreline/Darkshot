using System.Drawing;
using System.Windows.Forms;

namespace Darkshot.PaintTools
{
    enum PaintToolType
    {
        None,
        Pen,
        Line,
        Arrow,
        Rectangle,
        FilledRectangle,
        Text,
        Marker
    }

    interface IPaintTool
    {
        Cursor GetCursor();

        Rectangle GetBounds();

        bool ProcessMouseDown(MouseEventArgs e);

        bool ProcessMouseMove(MouseEventArgs e);

        bool ProcessMouseUp(MouseEventArgs e);

        bool ProcessKeyDown(KeyEventArgs e);

        void Paint(Graphics g);

    }
}
