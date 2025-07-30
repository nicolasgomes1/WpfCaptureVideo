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
        // Handle key press events
        KeyDown += MainWindow_KeyDown;
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.F1) return;
        // Toggle help visibility
        _viewModel.IsHelpVisible = !_viewModel.IsHelpVisible;
        e.Handled = true; // Prevent default F1 behavior
    }
}