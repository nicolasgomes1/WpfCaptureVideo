using System.Windows;
using System.Windows.Input;
using WpfRecorder.ViewModels;

namespace WpfRecorder;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;

        UpdateWindowTitle();

        
        // Handle key press events
        KeyDown += MainWindow_KeyDown;
        IsEnabledChanged += OnIsEnabledChanged;
        StateChanged += OnStateChanged;

    }

    
    private void UpdateWindowTitle()
    {
        // Check if the window is enabled to determine user authorization status
        if (IsEnabled)
        {
            _viewModel.TitleName = "Recorder";
        }
        else
        {
            _viewModel.TitleName = "Recorder (Unauthorized User)";
        }
    }

    private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        UpdateWindowTitle();
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        UpdateWindowTitle();
    }

    
    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.F1) return;
        // Toggle help visibility
        _viewModel.IsHelpVisible = !_viewModel.IsHelpVisible;
        e.Handled = true; // Prevent default F1 behavior
    }
}