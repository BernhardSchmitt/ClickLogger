using System.IO;
using Gma.System.MouseKeyHook;

namespace ClickLogger.Model
{
    public class ClickRecorder : IDisposable
    {
        private IKeyboardMouseEvents? _globalHook;
        private StreamWriter? _csvWriter;

        // Event raised when the recording state changes
        public event EventHandler<bool>? RecordingStateChanged;

        public bool IsRecording { get; private set; }

        public ClickRecorder()
        {
        }

        public void StartRecording(string path, string file)
        {
            // Check pre-reqs
            if (IsRecording) return;
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(file))
            {
                throw new InvalidOperationException("Folder path and file name must be set before recording.");
            }
            string filePath = Path.Combine(path, file);
            
            if (File.Exists(filePath))
            {
                throw new InvalidOperationException("File already exists. Please choose a different file path.");
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Let's go
            IsRecording = true;
            RecordingStateChanged?.Invoke(this, true);

            // Initialize CSV and write header
            _csvWriter = new StreamWriter(filePath, append: true) { AutoFlush = true };
            _csvWriter.WriteLine("Timestamp,Event,EventParameter");
            

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

        private void LogEvent(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string eventType = e.Clicks >= 2 ? "Double Click" : $"{e.Button} Click";
            string eventParameter = $"{e.X};{e.Y}";

            _csvWriter?.WriteLine($"{timestamp},{eventType},{eventParameter}");
        }

        public void Dispose()
        {
            StopRecording();
        }
    }
}