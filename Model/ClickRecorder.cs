using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;

namespace ClickLogger.Model
{
    public class ClickRecorder : IDisposable
    {
        private IKeyboardMouseEvents? _globalHook;
        private StreamWriter? _csvWriter;
        private readonly ScreenshotCamera? _screenshotCamera;
        private readonly ISaveScreenshot? _saveScreenshot;

        private readonly Blacklist _blacklist = new Blacklist();

        public event EventHandler<bool>? RecordingStateChanged;

        public bool IsRecording { get; private set; }

        public string LogPath { get; private set; } = string.Empty;

        public ClickRecorder(ScreenshotCamera? screenshotCamera = null, ISaveScreenshot? saveScreenshot = null)
        {
            _screenshotCamera = screenshotCamera;
            _saveScreenshot = saveScreenshot;
        }

        public void StartRecording(string path)
        {
            // Check pre-reqs
            if (IsRecording) return;
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("Folder path and file name must be set before recording.");
            }
            string filePath = Path.Combine(path, GetLogFileName());
            
            if (File.Exists(filePath))
            {
                throw new InvalidOperationException("File already exists. Please choose a different file path.");
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            LogPath = path;

            _blacklist.Initialize();

            // Let's go
            IsRecording = true;
            RecordingStateChanged?.Invoke(this, true);

            // Initialize CSV and write header
            _csvWriter = new StreamWriter(filePath, append: true) { AutoFlush = true };
            _csvWriter.WriteLine("Timestamp,ProcessName,WindowTitle,Event,EventParameter,Screenshot");
            

            // Hook Global Mouse Events
            _globalHook = Hook.GlobalEvents();
            _globalHook.MouseClick += LogEvent;
            _globalHook.MouseDoubleClick += LogEvent;
        }

        public void StopRecording()
        {
            if (!IsRecording) return;

            IsRecording = false;
            RecordingStateChanged?.Invoke(this, false);
            LogPath = string.Empty;

            // Cleanup
            if (_globalHook != null)
            {
                _globalHook.MouseClick -= LogEvent;
                _globalHook.MouseDoubleClick -= LogEvent;
                _globalHook.Dispose();
                _globalHook = null;
            }

            _csvWriter?.Close();
            _csvWriter?.Dispose();
            _csvWriter = null;
        }

        private void LogEvent(object? sender, MouseEventArgs e)
        {
            DateTime dateTime = DateTime.Now;
            string timestampCsv = dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string eventType = $"{e.Clicks}x {e.Button} Click";
            string eventParameter = $"{e.X};{e.Y}";

            var locatedWindowHandle = Locator.GetWindowHandleFromPoint(e.X, e.Y);
            string processName = string.Empty;
            string windowTitle = string.Empty;
            
            if (locatedWindowHandle != IntPtr.Zero)
            {
                processName = Locator.GetProcessNameFromWindowHandle(locatedWindowHandle);
                windowTitle = Locator.GetWindowTitleFromWindowHandle(locatedWindowHandle);
                if (_blacklist.IsBlacklisted(processName, windowTitle))
                {
                    // ignore this one
                    return;
                }
            }

            if (_screenshotCamera != null && _saveScreenshot != null)
            {
                Bitmap screenshot = ScreenshotCamera.TakeScreenshotAt(e, 400);
                string screenshotFileName = $"{dateTime.ToString("yyyyMMdd_HHmmssfff")}.{GetScreenshotFileExtension()}";
                string screenshotFilePath = Path.Combine(LogPath, screenshotFileName);
                _saveScreenshot.Save(screenshot, screenshotFilePath);
                _csvWriter?.WriteLine($"{timestampCsv},{processName},{windowTitle},{eventType},{eventParameter},{screenshotFileName}");
            }
            else
            {
                _csvWriter?.WriteLine($"{timestampCsv},{processName},{windowTitle},{eventType},{eventParameter},");
            }
        }

        private static string GetLogFileName()
        {
            return "ClickLog.csv";
        }

        private static string GetScreenshotFileExtension()
        {
            return "jpg";
        }

        public void Dispose()
        {
            StopRecording();
        }
    }
}