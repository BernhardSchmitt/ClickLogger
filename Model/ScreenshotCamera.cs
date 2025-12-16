using System.Drawing;
using System.Windows.Forms;

namespace ClickLogger.Model
{
    public class ScreenshotCamera
    {
        public static Bitmap TakeScreenshotAt(int x, int y, int size)
        {
            Bitmap bitmap = new(size, size);
            
            // Calculate the top-left corner with x,y at center
            int left = x - size / 2;
            int top = y - size / 2;
            
            // Adjust if boundaries are exceeded
            if (left < 0) left = 0;
            if (top < 0) top = 0;
            if (Screen.PrimaryScreen != null && left + size > Screen.PrimaryScreen.Bounds.Width)
            left = Screen.PrimaryScreen.Bounds.Width - size;
            if (Screen.PrimaryScreen != null && top + size > Screen.PrimaryScreen.Bounds.Height)
            top = Screen.PrimaryScreen.Bounds.Height - size;
            
            using (Graphics g = Graphics.FromImage(bitmap))
            {
            g.CopyFromScreen(left, top, 0, 0, new Size(size, size));
            }
            return bitmap;
        }
        
    }
}