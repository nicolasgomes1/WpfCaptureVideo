using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using Serilog.Core;

namespace WpfRecorder.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ILogger _logger = Log.ForContext<MainWindowViewModel>();
    
    [RelayCommand]
    private async Task StartRecording()
    {
        _logger.Information("Start recording");
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task PauseRecording()
    {
        _logger.Information("Pause recording");
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task ResumeRecording()
    {
        _logger.Information("Resume recording");
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task SaveRecording()
    {
        _logger.Information("Save recording");
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task CancelRecording()
    {
        _logger.Information("Cancel recording");
        await Task.CompletedTask;
    }
    
    
    
}