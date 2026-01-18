using System.Drawing;
using System.Windows.Forms;

namespace ClickLogger.Model
{
    public class ScreenshotCamera
    {
        private static readonly Color _overlayColor = Color.Red;

        public static Bitmap TakeScreenshotAt(MouseEventArgs startEventArgs, MouseEventArgs? endEventArgs = null, int minSize = 400)
        {
            Size size = new(minSize, minSize);
            if (endEventArgs != null)
            {
                int deltaX = Math.Abs(endEventArgs.X - startEventArgs.X);
                int deltaY = Math.Abs(endEventArgs.Y - startEventArgs.Y);
                int padding = 200; // Increased padding for arrow visibility
                size = new Size(Math.Max(minSize, deltaX + padding), Math.Max(minSize, deltaY + padding));
            }

            Bitmap bitmap = new(size.Width, size.Height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                if (endEventArgs == null)
                {
                    // Single click screenshot
                    Point offset = GetScreenshotOffset(startEventArgs.X, startEventArgs.Y, size);
                    // Copy from screen using screen coordinates (Cursor - Offset)
                    g.CopyFromScreen(startEventArgs.X - offset.X, startEventArgs.Y - offset.Y, 0, 0, size);
                    DrawClickOverlay(g, offset, startEventArgs.Button, startEventArgs.Clicks);
                }
                else
                {
                    // Drag screenshot
                    int midX = (startEventArgs.X + endEventArgs.X) / 2;
                    int midY = (startEventArgs.Y + endEventArgs.Y) / 2;
                    Point offset = GetScreenshotOffset(midX, midY, size);
                    
                    g.CopyFromScreen(midX - offset.X, midY - offset.Y, 0, 0, size);

                    // Map screen points to bitmap local coordinates
                    Point localStart = new(startEventArgs.X - (midX - offset.X), startEventArgs.Y - (midY - offset.Y));
                    Point localEnd = new(endEventArgs.X - (midX - offset.X), endEventArgs.Y - (midY - offset.Y));
                    
                    DrawDragOverlay(g, localStart, localEnd, endEventArgs.Button);
                }
            }
            return bitmap;
        }

        private static void DrawClickOverlay(Graphics g, Point center, MouseButtons button, int clicks)
        {
            Color semiTransparentColor = Color.FromArgb(128, _overlayColor.R, _overlayColor.G, _overlayColor.B);
            int innerRadius = 10;

            using (SolidBrush brush = new SolidBrush(semiTransparentColor))
            {
                g.FillEllipse(brush, center.X - innerRadius, center.Y - innerRadius, innerRadius * 2, innerRadius * 2);
            }

            int outerRadius = 20;
            int deltaRadius = 8;

            using (Pen pen = new Pen(_overlayColor, 2))
            {
                for (int i = 0; i < clicks; i++)
                {
                    // Rectangle must encompass the full diameter
                    Rectangle rect = new Rectangle(center.X - outerRadius, center.Y - outerRadius, outerRadius * 2, outerRadius * 2);
                    
                    if (button == MouseButtons.Left)
                        g.DrawArc(pen, rect, 90, 180);
                    else if (button == MouseButtons.Right)
                        g.DrawArc(pen, rect, 270, 180);
                    
                    outerRadius += deltaRadius;
                }
            }
        }

        private static void DrawDragOverlay(Graphics g, Point start, Point end, MouseButtons button)
        {
            // Draw the click indicator at the start point
            DrawClickOverlay(g, start, button, 1);

            // Draw line with arrow
            using (Pen pen = new(_overlayColor, 3))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.DrawLine(pen, start, end);
                
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

        private static Point GetScreenshotOffset(int targetX, int targetY, Size size)
        {
            Rectangle virtualScreen = SystemInformation.VirtualScreen;

            int left = targetX - size.Width / 2;
            int top = targetY - size.Height / 2;

            // Clamp to screen boundaries
            if (left < virtualScreen.Left) left = virtualScreen.Left;
            if (top < virtualScreen.Top) top = virtualScreen.Top;
            if (left + size.Width > virtualScreen.Right) left = virtualScreen.Right - size.Width;
            if (top + size.Height > virtualScreen.Bottom) top = virtualScreen.Bottom - size.Height;

            // Return the relative position of the target within the resulting bitmap
            return new Point(targetX - left, targetY - top);
        }
    }
}