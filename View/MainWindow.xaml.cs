using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ClickLogger.ViewModel;

namespace ClickLogger.View
{
    public partial class MainWindow : Window
    {
        private MainViewModel? ViewModel => DataContext as MainViewModel;
        private Storyboard _pulseStoryboard;
        
        private readonly ImageSource _recOverlay = BitmapFrame.Create(new Uri("pack://application:,,,/recOverlay.ico", UriKind.Absolute));


        public MainWindow()
        {
            InitializeComponent();
            _pulseStoryboard = (Storyboard)this.Resources["PulseAnimation"];

            // position on loaded
            Loaded += MainWindow_Loaded;
            
            // Subscribe to the ViewModel's property changes to handle UI updates (animation)
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var wa = SystemParameters.WorkArea;
                const double margin = 10; // gap from screen edges
                Left = wa.Right - ActualWidth - margin;
                Top = wa.Bottom - ActualHeight - margin;
            }), DispatcherPriority.Loaded);
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Only handle the IsRecording property change to control the animation
            if (e.PropertyName == nameof(MainViewModel.IsRecording) && ViewModel != null)
            {
                if (ViewModel.IsRecording)
                {
                    _pulseStoryboard.Begin();
                    
                    TaskbarInfo.Overlay = _recOverlay;
                    TaskbarInfo.Description = "Recording…";

                }
                else
                {
                    _pulseStoryboard.Stop();
                    PulseEffect.Opacity = 0; // Ensure glow is off

                    TaskbarInfo.Overlay = null;
                    TaskbarInfo.Description = "Ready for recording";
                }
            }
        }

        // Only for Window drag - a non-bindable UI concern
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }
    }
}
