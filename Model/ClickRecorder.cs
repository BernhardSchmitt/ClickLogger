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
        private MouseEventArgs? _dragStartEvent;
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
            _globalHook.MouseClick += OnClick;
            _globalHook.MouseDoubleClick += OnDoubleClick;
            _globalHook.MouseDragStarted += OnDragStart;
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
                _globalHook.MouseClick -= OnClick;
                _globalHook.MouseDoubleClick -= OnDoubleClick;
                _globalHook.MouseDragStarted -= OnDragStart;
                _globalHook.MouseDragFinished -= OnDragEnd;
                _globalHook.Dispose();
                _globalHook = null;
            }

            _csvWriter?.Close();
            _csvWriter?.Dispose();
            _csvWriter = null;
        }

        private void OnClick(object? sender, MouseEventArgs e)
        {
            string eventType = $"{e.Clicks}x {e.Button} Click";
            string eventParameter = $"{e.X};{e.Y}";

            LogEvent(eventType, eventParameter, e);
        }

        private void OnDoubleClick(object? sender, MouseEventArgs e)
        {
            string eventType = $"{e.Clicks}x {e.Button} Clicks";
            string eventParameter = $"{e.X};{e.Y}";

            LogEvent(eventType, eventParameter, e);
        }

        private void OnDragStart(object? sender, MouseEventArgs e)
        {
            _dragStartEvent = e;
            if (_globalHook != null)
            {
                _globalHook.MouseDragFinished += OnDragEnd;
            }
        }

        private void OnDragEnd(object? sender, MouseEventArgs e)
        {
            if (_dragStartEvent != null)
            {
                string eventType = $"{e.Button} drag";
                string eventParameter = $"{_dragStartEvent.X};{_dragStartEvent.Y} to {e.X};{e.Y}";

                LogEvent(eventType, eventParameter, _dragStartEvent, e);
            }

            _dragStartEvent = null;
            if (_globalHook != null)
            {
                _globalHook.MouseDragFinished -= OnDragEnd;
            }
        }
        private void LogEvent(string eventType, string eventParameter, MouseEventArgs startEventArgs, MouseEventArgs? endEventArgs = null)
        {
            DateTime dateTime = DateTime.Now;
            string timestampCsv = dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            
            // Check blacklist
            var locatedWindowHandle = Locator.GetWindowHandleFromPoint(startEventArgs.X, startEventArgs.Y);
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

            // Take screenshot if enabled and log event
            if (_screenshotCamera != null && _saveScreenshot != null)
            {
                Bitmap screenshot = ScreenshotCamera.TakeScreenshotAt(startEventArgs, endEventArgs);
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