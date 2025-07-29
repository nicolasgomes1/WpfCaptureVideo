using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfRecorder.ViewModels;

namespace WpfRecorder;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel = new();
    
    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        // Handle key press events
        this.KeyDown += MainWindow_KeyDown;
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.F1) return;
        // Toggle help visibility
        _viewModel.IsHelpVisible = !_viewModel.IsHelpVisible;
        e.Handled = true; // Prevent default F1 behavior
    }

}