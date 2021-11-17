using System;
using System.Drawing;
using System.Windows.Forms;

namespace Darkshot.PaintTools
{
    abstract class PaintToolFactory
    {

        public static IPaintTool Create(PaintToolType func, Color color, Color bg)
        {
            switch (func)
            {
                case PaintToolType.Pen:
                    return new PaintToolPen(color);
                case PaintToolType.Line:
                    return new PaintToolLine(color);
                case PaintToolType.Arrow:
                    return new PaintToolArrow(color);
                case PaintToolType.Rectangle:
                    return new PaintToolRectangle(color);
                case PaintToolType.FilledRectangle:
                    return new PaintToolFilledRectangle(color);
                case PaintToolType.Text:
                    return new PaintToolText(color);
                case PaintToolType.Marker:
                    return new PaintToolMarker(bg);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
