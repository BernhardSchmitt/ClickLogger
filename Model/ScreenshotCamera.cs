using System.Drawing;
using System.Windows.Forms;

namespace ClickLogger.Model
{
    public class ScreenshotCamera
    {
        private static readonly Color _overlayColor = Color.Red;

        public static Bitmap TakeScreenshotAt(MouseEventArgs startEventArgs, MouseEventArgs? endEventArgs = null, int minSize = 400)
        {
            // extend screenshot size for drag events if necessary
            int size = minSize;
            if (endEventArgs != null)
            {
                int deltaX = Math.Abs(endEventArgs.X - startEventArgs.X);
                int deltaY = Math.Abs(endEventArgs.Y - startEventArgs.Y);
                int requiredSize = Math.Max(deltaX, deltaY) + 100; // add some padding
                if (requiredSize > minSize)
                {
                    size = requiredSize;
                }
            }
            Bitmap bitmap = new(size, size);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // cursor position should be at center of screenshot if surrounding space allows
                // calculate offset for top-left of screenshot
                Point screenshotOffset = GetScreenshotOffset(startEventArgs.X, startEventArgs.Y, size);
                g.CopyFromScreen(startEventArgs.X - screenshotOffset.X, startEventArgs.Y - screenshotOffset.Y, 0, 0, new Size(size, size));

                if (endEventArgs == null)
                {
                    // It's a click event
                    DrawClickOverlay(g, new Point(screenshotOffset.X, screenshotOffset.Y), startEventArgs.Button, startEventArgs.Clicks);
                }
                else
                {
                    // It's a drag event
                    Point startPoint = new Point(startEventArgs.X - (startEventArgs.X - screenshotOffset.X), startEventArgs.Y - (startEventArgs.Y - screenshotOffset.Y));
                    Point endPoint = new Point(endEventArgs.X - (startEventArgs.X - screenshotOffset.X), endEventArgs.Y - (startEventArgs.Y - screenshotOffset.Y));
                    // only endEvent contains button info
                    DrawDragOverlay(g, startPoint, endPoint, endEventArgs.Button);
                }
            }
            return bitmap;
        }

        private static void DrawClickOverlay(Graphics g, Point center, MouseButtons button, int clicks)
        {
            // Draw overlay at click position
            Color semiTransparentColor = Color.FromArgb(128, _overlayColor.R, _overlayColor.G, _overlayColor.B);
            int innerRadius = 10;

            using (SolidBrush brush = new SolidBrush(semiTransparentColor))
            {
                int diameter = innerRadius * 2;
                int rectX = center.X - innerRadius;
                int rectY = center.Y - innerRadius;

                g.FillEllipse(brush, rectX, rectY, diameter, diameter);
            }

            // Draw outer half circle(s) to indicate left or right mouse button click(s)
            int outerRadius = 30;
            int deltaRadius = 8;

            for (int i = 0; i < clicks; i++)
            {
                if (button == MouseButtons.Left)
                {
                    // Left half circle
                    using (Pen pen = new Pen(_overlayColor, 2))
                    {
                        g.DrawArc(pen, center.X - outerRadius / 2, center.Y - outerRadius / 2, outerRadius, outerRadius, 90, 180);
                    }
                }
                else if (button == MouseButtons.Right)
                {
                    // Right half circle
                    using (Pen pen = new Pen(_overlayColor, 2))
                    {
                        g.DrawArc(pen, center.X - outerRadius / 2, center.Y - outerRadius / 2, outerRadius, outerRadius, 270, 180);
                    }
                }
                outerRadius += deltaRadius;
            }
        }

        private static void DrawDragOverlay(Graphics g, Point start, Point end, MouseButtons button)
        {
            // Draw overlay at start position to indicate which button was used
            Point center = start;
            Color semiTransparentColor = Color.FromArgb(128, _overlayColor.R, _overlayColor.G, _overlayColor.B);
            int innerRadius = 10;

            using (SolidBrush brush = new SolidBrush(semiTransparentColor))
            {
                int diameter = innerRadius * 2;
                int rectX = center.X - innerRadius;
                int rectY = center.Y - innerRadius;

                g.FillEllipse(brush, rectX, rectY, diameter, diameter);
            }

            // Draw outer half circle(s) to indicate left or right mouse button click(s)
            int outerRadius = 30;
            
            if (button == MouseButtons.Left)
            {
                // Left half circle
                using (Pen pen = new Pen(_overlayColor, 2))
                {
                    g.DrawArc(pen, center.X - outerRadius / 2, center.Y - outerRadius / 2, outerRadius, outerRadius, 90, 180);
                }
            }
            else if (button == MouseButtons.Right)
            {
                // Right half circle
                using (Pen pen = new Pen(_overlayColor, 2))
                {
                    g.DrawArc(pen, center.X - outerRadius / 2, center.Y - outerRadius / 2, outerRadius, outerRadius, 270, 180);
                }
            }
            
            // Draw line with arrow from start to end
            using (Pen pen = new(_overlayColor, 3))
            {
                g.DrawLine(pen, start, end);
                
                // Draw arrow head
                float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
                float arrowSize = 15;
                
                PointF arrowPoint1 = new(
                    end.X - arrowSize * (float)Math.Cos(angle - Math.PI / 6),
                    end.Y - arrowSize * (float)Math.Sin(angle - Math.PI / 6)
                );
                PointF arrowPoint2 = new(
                    end.X - arrowSize * (float)Math.Cos(angle + Math.PI / 6),
                    end.Y - arrowSize * (float)Math.Sin(angle + Math.PI / 6)
                );
                
                g.DrawLine(pen, end, arrowPoint1);
                g.DrawLine(pen, end, arrowPoint2);
            }
        }

        private static Point GetScreenshotOffset(int cursorX, int cursorY, int size)
        {
            // Get the total bounding box of all displays (the virtual canvas)
            Rectangle virtualScreenBounds = SystemInformation.VirtualScreen;

            // 1. Calculate the initial top-left corner of the screenshot area
            int left = cursorX - size / 2;
            int top = cursorY - size / 2;

            // 2. Adjust if boundaries are exceeded (Boundary checks for the virtual canvas)

            // Check against the LEFT edge of the virtual canvas (usually 0, but can be negative)
            if (left < virtualScreenBounds.Left)
            {
                left = virtualScreenBounds.Left;
            }

            // Check against the TOP edge of the virtual canvas (usually 0, but can be negative)
            if (top < virtualScreenBounds.Top)
            {
                top = virtualScreenBounds.Top;
            }

            // Check against the RIGHT edge of the virtual canvas
            if (left + size > virtualScreenBounds.Right)
            {
                // Adjust 'left' so the right edge of the screenshot aligns with the virtual screen's right edge
                left = virtualScreenBounds.Right - size;
            }

            // Check against the BOTTOM edge of the virtual canvas
            if (top + size > virtualScreenBounds.Bottom)
            {
                // Adjust 'top' so the bottom edge of the screenshot aligns with the virtual screen's bottom edge
                top = virtualScreenBounds.Bottom - size;
            }

            return new Point(cursorX - left, cursorY - top);
        }
    }
}