using System.Drawing;
using System.Windows.Forms;

namespace ClickLogger.Model
{
    public class ScreenshotCamera
    {
        public static Bitmap TakeScreenshotAt(int x, int y, int size)
        {
            Bitmap bitmap = new(size, size);
            
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                Point screenshotTopLeft = GetScreenshotTopLeft(x, y, size);
                g.CopyFromScreen(screenshotTopLeft.X, screenshotTopLeft.Y, 0, 0, new Size(size, size));
            }
            return bitmap;
        }
        
        private static Point GetScreenshotTopLeft(int x, int y, int size)
        {
            int left = x - size / 2;
            int top = y - size / 2;

            // Adjust if boundaries are exceeded
            if (left < 0) left = 0;
            if (top < 0) top = 0;
            if (Screen.PrimaryScreen != null && left + size > Screen.PrimaryScreen.Bounds.Width)
                left = Screen.PrimaryScreen.Bounds.Width - size;
            if (Screen.PrimaryScreen != null && top + size > Screen.PrimaryScreen.Bounds.Height)
                top = Screen.PrimaryScreen.Bounds.Height - size;

            return new Point(left, top);
        }
    }
}