using System;
using System.Drawing;
using System.IO;

namespace ClickLogger.Model
{
    public class SaveScreenshotJpg : ISaveScreenshot
    {
        public void Save(Bitmap screenshot, string filePath)
        {
            if (screenshot == null)
                throw new ArgumentNullException(nameof(screenshot));
            
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"The directory '{directory}' does not exist.");

            screenshot.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
        }
    }
}