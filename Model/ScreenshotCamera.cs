using System.Drawing;
using System.Windows.Forms;

namespace ClickLogger.Model
{
    public class ScreenshotCamera
    {
        public static Bitmap TakeScreenshotAt(MouseEventArgs e, int size)
        {
            Bitmap bitmap = new(size, size);
            
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // cursor position should be at center of screenshot if surrounding space allows
                // calculate offset for top-left of screenshot
                Point screenshotOffset = GetScreenshotOffset(e.X, e.Y, size);
                g.CopyFromScreen(e.X - screenshotOffset.X, e.Y - screenshotOffset.Y, 0, 0, new Size(size, size));

                // Draw overlay at click position
                Color overlayColor = Color.Red;
                Color semiTransparentColor = Color.FromArgb(128, overlayColor.R, overlayColor.G, overlayColor.B);
                int innerRadius = 10;

                using (SolidBrush brush = new SolidBrush(semiTransparentColor))
                {
                    int diameter = innerRadius * 2;
                    int rectX = screenshotOffset.X - innerRadius;
                    int rectY = screenshotOffset.Y - innerRadius;

                    g.FillEllipse(brush, rectX, rectY, diameter, diameter);
                }

                // Draw outer half circle(s) to indicate left or right mouse button click(s)
                int outerRadius = 30;
                int deltaRadius = 8;
                
                for (int i = 0; i < e.Clicks; i++)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        // Left half circle
                        using (Pen pen = new Pen(overlayColor, 2))
                        {
                            g.DrawArc(pen, screenshotOffset.X - outerRadius / 2, screenshotOffset.Y  - outerRadius / 2, outerRadius, outerRadius, 90, 180);
                        }
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        // Right half circle
                        using (Pen pen = new Pen(overlayColor, 2))
                        {
                            g.DrawArc(pen, screenshotOffset.X - outerRadius / 2, screenshotOffset.Y  - outerRadius / 2, outerRadius, outerRadius, 270, 180);
                        }
                    }
                    outerRadius += deltaRadius;
                }
            }
            return bitmap;
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