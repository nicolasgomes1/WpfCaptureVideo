using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
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
    private readonly ScreenShootWrapper _screen = new();
    public MainWindowViewModel()
    {
        SettingsService.LoadFromJson(ref _videoPath, ref _picturePath);
        _logger.Information("Temporary Count {a}",_screen.GetTempImageCount());
    }

    
    // Timer and time tracking fields
    private PeriodicTimer? _recordingTimer;
    private DateTime _recordingStartTime;
    private TimeSpan _pausedDuration = TimeSpan.Zero;
    private DateTime _pauseStartTime;
    private CancellationTokenSource? _timerCancellationTokenSource;

    // Observable property for the elapsed time display
    [ObservableProperty] public partial string ElapsedTime { get; set; } = "00:00:00";
    [ObservableProperty] public partial string TitleName { get; set; } = "Recorder";
    
    [ObservableProperty] public partial bool IsStartButtonEnabled { get; set; } = true;
    [ObservableProperty] public partial bool IsPauseButtonEnabled { get; set; } = false;
    [ObservableProperty] public partial bool IsResumeButtonEnabled { get; set; } = false;
    [ObservableProperty] public partial bool IsSaveButtonEnabled { get; set; } = false;
    [ObservableProperty] public partial bool IsCancelButtonEnabled { get; set; } = false;
    
    public bool IsExportToPdfVisible => _screen.GetTempImageCount() > 0;

    private void UpdateIsExportToPdfVisible()
    {
        OnPropertyChanged(nameof(IsExportToPdfVisible));
    }

    [ObservableProperty] public partial bool IsHelpVisible { get; set; } = false;

    
    private string _videoPath = string.Empty;
    public string VideoPath 
    { 
        get => _videoPath;
        set
        {
            if (SetProperty(ref _videoPath, value))
            {
                SettingsService.UpdateJsonKey("SaveDirectories", "VideoDir", value);
            }
        }
    }

    private string _picturePath = string.Empty;
    public string PicturePath 
    { 
        get => _picturePath;
        set
        {
            if (SetProperty(ref _picturePath, value))
            {
                SettingsService.UpdateJsonKey("SaveDirectories", "PictureDir", value);
            }
        }
    }
    


    
    [RelayCommand]
    private async Task StartRecording()
    {
        var videoPath = "";
        var currentDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        if (IsVideoPathValid())
        {
            videoPath = Path.Combine(VideoPath, $"Recorder_{currentDate}.mp4");

        }
        else
        {
            videoPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Recorder_{currentDate}.mp4");
            _logger.Information("Recording to: {videoPath}", videoPath);

        }
        _recorder.CreateRecording(videoPath);
        // Initialize time tracking
        _recordingStartTime = DateTime.Now;
        _pausedDuration = TimeSpan.Zero;
        await StartTimer();

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
        var screenShootPath = "";
        var currentDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        if (IsPicturePathValid())
        {
            screenShootPath = Path.Combine(PicturePath, $"Capture_{currentDate}.png");
        }
        else
        {
            screenShootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Capture_{currentDate}.png");
            _logger.Information("Saving ScreenShoot to: {ScreenShootPath}", screenShootPath);
        }
        
        
        _screen.TakeScreenshot(screenShootPath);
        _logger.Information("Saved Screenshoot to: {a}", screenShootPath);
        await Task.CompletedTask;
    }

    private bool IsPicturePathValid()
    {
        return !string.IsNullOrWhiteSpace(PicturePath) && Directory.Exists(PicturePath);
    }

    private bool IsVideoPathValid()
    {
        return !string.IsNullOrWhiteSpace(VideoPath) && Directory.Exists(VideoPath);
    }
    
    

    [RelayCommand]
    private async Task TakeScreenShootRegion()
    {
        var screenShootPath = "";
        var currentDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        
        _logger.Information("Selecting a region for screenshot");

        await Task.Yield(); // Ensure async context
    
        var selector = new RegionSelectorWindow();
        if (selector.ShowDialog() == true)
        {
            var rect = selector.SelectedRegion;

            if (IsPicturePathValid())
            {
                screenShootPath = Path.Combine(PicturePath, $"CaptureP_{currentDate}.png");
                _logger.Information("Screenshot saved to {Path}", screenShootPath);
            }
            else
            {
                screenShootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"CaptureP_{currentDate}.png");
                _logger.Information("Screenshot saved to {Path}", screenShootPath);
            }
            
            ScreenShootWrapper.TakeScreenshotRegion(screenShootPath, (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }
        else
        {
            _logger.Information("Region selection cancelled");
        }
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

    [ObservableProperty] public partial int MultipleScreenShoots { get; set; } = 0;
    
    
    [RelayCommand]
    public async Task TakeMultipleScreenshoot()
    {
        UpdateIsExportToPdfVisible();
        try
        {
            
            var count = _screen.TakeScreenshotToTemp();
            MultipleScreenShoots = count;
            _logger.Information("Taking multiple screenshots. Total stored: {Count}", count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error taking screenshot to temporary storage");
        }
        await Task.CompletedTask;
    }


    [RelayCommand]
    public async Task ExportToPdf()
    {

        try
        {
            if (MultipleScreenShoots == 0)
            {
                _logger.Warning("No screenshots to export");
                return;
            }

            var currentDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string pdfPath;

            if (IsPicturePathValid())
            {
                pdfPath = Path.Combine(PicturePath, $"Screenshots_{currentDate}.pdf");
            }
            else
            {
                pdfPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Screenshots_{currentDate}.pdf");
            }

            _screen.ExportTempImagesToPdf(pdfPath);
            _screen.ClearTempImages();
            MultipleScreenShoots = 0;
            
            _logger.Information("Exported {Count} screenshots to PDF: {Path}", MultipleScreenShoots, pdfPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error exporting screenshots to PDF");
        }
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    public async Task ClearTempScreenshots()
    {
        _screen.ClearTempImages();
        MultipleScreenShoots = 0;
        _logger.Information("Cleared temporary screenshots");
        await Task.CompletedTask;
    }



}