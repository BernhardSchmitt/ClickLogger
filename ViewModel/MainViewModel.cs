using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ClickLogger.Model;

namespace ClickLogger.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ClickRecorder _recorder;

        private bool _isRecording;
        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                if (_isRecording != value)
                {
                    _isRecording = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RecordButtonText));
                }
            }
        }

        private string _folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClickLogger");
        public string FolderPath
        {
            get => _folderPath;
            set
            {
                if (_folderPath != value)
                {
                    _folderPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _fileName = "";
        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string RecordButtonText => IsRecording ? "STOP" : "REC";

        public ICommand ToggleRecordCommand { get; }
        public ICommand OpenInExplorerCommand { get; }
        public ICommand QuitCommand { get; }

        public MainViewModel()
        {
            _recorder = new ClickRecorder(new ScreenshotCamera(), new SaveScreenshotJpg());
            _recorder.RecordingStateChanged += (s, isRec) => IsRecording = isRec;

            ToggleRecordCommand = new RelayCommand(ToggleRecord, CanToggleRecord);
            OpenInExplorerCommand = new RelayCommand(OpenInExplorer);
            QuitCommand = new RelayCommand(Quit);
        }

        private bool CanToggleRecord(object parameter) => !string.IsNullOrEmpty(FolderPath);

        private void ToggleRecord(object parameter)
        {
            if (IsRecording)
            {
                _recorder.StopRecording();
                FileName = "";
            }
            else
            {
                try
                {
                    _recorder.StartRecording(GetSessionPath());
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message);
                    FileName = "";
                }
            }
        }

        private string GetSessionPath()
        {
            return Path.Combine(FolderPath, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        }

        private void OpenInExplorer(object parameter)
        {
            if (Directory.Exists(FolderPath))
            {
            System.Diagnostics.Process.Start("explorer.exe", FolderPath);
            }
            else
            {
            MessageBox.Show("Folder does not exist.");
            }
        }

        private void Quit(object parameter)
        {
            if (IsRecording)
            {
                _recorder.StopRecording();
                FileName = "";
            }
            Application.Current.Shutdown();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}