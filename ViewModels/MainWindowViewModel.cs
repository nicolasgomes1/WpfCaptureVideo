using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using Serilog.Core;
using WpfRecorder.Services;

namespace WpfRecorder.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ILogger _logger = Log.ForContext<MainWindowViewModel>();
    private readonly RecorderWrapper _recorder = new();

    // Timer and time tracking fields
    private PeriodicTimer? _recordingTimer;
    private DateTime _recordingStartTime;
    private TimeSpan _pausedDuration = TimeSpan.Zero;
    private DateTime _pauseStartTime;
    private CancellationTokenSource? _timerCancellationTokenSource;

    // Observable property for the elapsed time display
    [ObservableProperty] public partial string ElapsedTime { get; set; } = "00:00:00";

    
    [ObservableProperty] public partial bool IsStartButtonEnabled { get; set; } = true;
    [ObservableProperty] public partial bool IsPauseButtonEnabled { get; set; } = false;
    [ObservableProperty] public partial bool IsResumeButtonEnabled { get; set; } = false;
    [ObservableProperty] public partial bool IsSaveButtonEnabled { get; set; } = false;
    [ObservableProperty] public partial bool IsCancelButtonEnabled { get; set; } = false;

    
    [ObservableProperty] public partial bool IsHelpVisible { get; set; } = false;

    
    [RelayCommand]
    private async Task StartRecording()
    {
        // Initialize time tracking
        _recordingStartTime = DateTime.Now;
        _pausedDuration = TimeSpan.Zero;
        await StartTimer();

        _recorder.CreateRecording();
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
        // Track pause start time and stop timer
        _pauseStartTime = DateTime.Now;
        await StopTimer();

        _recorder.PauseRecording();
        _logger.Information("Pause recording");
        IsResumeButtonEnabled = true;
        IsStartButtonEnabled = false;
        IsPauseButtonEnabled = false;

        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task ResumeRecording()
    {
        // Add paused duration and restart timer
        _pausedDuration += DateTime.Now - _pauseStartTime;
        await StartTimer();

        _recorder.ResumeRecording();
        _logger.Information("Resume recording");
        IsSaveButtonEnabled = true;
        IsResumeButtonEnabled = false;
        IsPauseButtonEnabled = true;
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task SaveRecording()
    {
        // Stop timer and reset
        await StopTimer();
        ResetTimer();


        await _recorder.EndRecordingAsync();
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
        
        // Stop timer and reset
        await StopTimer();
        ResetTimer();

        IsStartButtonEnabled = true;
        IsPauseButtonEnabled = false;
        IsResumeButtonEnabled = false;
        IsSaveButtonEnabled = false;
        IsCancelButtonEnabled = false;
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task TakeScreenShoot()
    {
        var currentDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        string screenShootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Capture_{currentDate}.png");
        _logger.Information("Saving ScreenShoot to: {ScreenShootPath}", screenShootPath);
        var screen = new ScreenShootWrapper();
        screen.TakeScreenshot(screenShootPath);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task TakeScreenShootRegion()
    {
        _logger.Information("Taking a screenshoot of a region");
        await Task.CompletedTask;
    }
    
    
    // Timer management methods
    private async Task StartTimer()
    {
        await StopTimer(); // Ensure any existing timer is stopped
        
        _timerCancellationTokenSource = new CancellationTokenSource();
        _recordingTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        
        // Start the timer loop in a background task
        _ = Task.Run(async () =>
        {
            try
            {
                while (await _recordingTimer.WaitForNextTickAsync(_timerCancellationTokenSource.Token))
                {
                    UpdateElapsedTime();
                }
            }
            catch (OperationCanceledException)
            {
                // Timer was cancelled, this is expected
            }
        }, _timerCancellationTokenSource.Token);
    }
    
    private async Task StopTimer()
    {
        if (_timerCancellationTokenSource != null)
        {
            await _timerCancellationTokenSource.CancelAsync();
            _timerCancellationTokenSource.Dispose();
            _timerCancellationTokenSource = null;
        }
        
        _recordingTimer?.Dispose();
        _recordingTimer = null;
    }
    
    private void ResetTimer()
    {
        ElapsedTime = "00:00:00";
        _pausedDuration = TimeSpan.Zero;
    }
    
    private void UpdateElapsedTime()
    {
        var elapsed = DateTime.Now - _recordingStartTime - _pausedDuration;
        ElapsedTime = elapsed.ToString(@"hh\:mm\:ss");
    }

}