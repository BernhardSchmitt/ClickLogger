using System.Drawing;

namespace ClickLogger.Model
{
    public interface ISaveScreenshot
    {
        void Save(Bitmap screenshot, string filePath);
    }
}