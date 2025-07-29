using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using Serilog.Core;

namespace WpfRecorder.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ILogger _logger = Log.ForContext<MainWindowViewModel>();

    [ObservableProperty] public partial bool IsStartButtonEnabled { get; set; } = true;
    [ObservableProperty] public partial bool IsPauseButtonEnabled { get; set; } = false;
    [ObservableProperty] public partial bool IsResumeButtonEnabled { get; set; } = false;
    [ObservableProperty] public partial bool IsSaveButtonEnabled { get; set; } = false;
    [ObservableProperty] public partial bool IsCancelButtonEnabled { get; set; } = false;

    
    [RelayCommand]
    private async Task StartRecording()
    {
        _logger.Information("Start recording");
        IsStartButtonEnabled = false;
        IsResumeButtonEnabled = false;
        IsPauseButtonEnabled = true;
        IsSaveButtonEnabled = true;
        IsCancelButtonEnabled = true;
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task PauseRecording()
    {
        _logger.Information("Pause recording");
        IsResumeButtonEnabled = true;
        IsStartButtonEnabled = false;
        IsPauseButtonEnabled = false;

        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task ResumeRecording()
    {
        _logger.Information("Resume recording");
        IsSaveButtonEnabled = true;
        IsResumeButtonEnabled = false;
        IsPauseButtonEnabled = true;
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task SaveRecording()
    {
        _logger.Information("Save recording");
        IsStartButtonEnabled = true;
        IsResumeButtonEnabled = false;
        IsSaveButtonEnabled = false;
        IsPauseButtonEnabled = false;
        IsCancelButtonEnabled = false;
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task CancelRecording()
    {
        _logger.Information("Cancel recording");
        IsStartButtonEnabled = true;
        IsPauseButtonEnabled = false;
        IsResumeButtonEnabled = false;
        IsSaveButtonEnabled = false;
        IsCancelButtonEnabled = false;
        await Task.CompletedTask;
    }
    
    
    
}