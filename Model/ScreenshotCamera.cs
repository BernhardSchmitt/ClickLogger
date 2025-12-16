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
                int innerRadius = 6;
                using (Pen pen = new Pen(overlayColor, 2))
                {
                    g.DrawArc(pen, screenshotOffset.X - innerRadius / 2, screenshotOffset.Y  - innerRadius / 2, innerRadius, innerRadius, 0, 360);
                }

                // Draw outer half circle to indicate left or right mouse button
                int outerRadius = 40;
                
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
            }
            return bitmap;
        }

        private static Point GetScreenshotOffset(int cursorX, int cursorY, int size)
        {
            int left = cursorX - size / 2;
            int top = cursorY - size / 2;

            // Adjust if boundaries are exceeded
            if (left < 0) left = 0;
            if (top < 0) top = 0;
            if (Screen.PrimaryScreen != null && left + size > Screen.PrimaryScreen.Bounds.Width)
                left = Screen.PrimaryScreen.Bounds.Width - size;
            if (Screen.PrimaryScreen != null && top + size > Screen.PrimaryScreen.Bounds.Height)
                top = Screen.PrimaryScreen.Bounds.Height - size;

            return new Point(cursorX - left, cursorY - top);
        }
    }
}