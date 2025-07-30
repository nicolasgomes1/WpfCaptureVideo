using System.IO;
using ScreenRecorderLib;
using Serilog;

namespace WpfRecorder.Services;

public class RecorderWrapper : IDisposable
{
    private readonly ILogger _logger = Log.ForContext<RecorderWrapper>();

    
    private Recorder _rec = null!;
    private RecorderStatus _status = RecorderStatus.Idle;

    private string VideoOptions()
    {
        var currentDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        
        string videoPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Recorder_{currentDate}.mp4");
        _logger.Information("Recording to: {videoPath}", videoPath);
        var options = new RecorderOptions
        {
            VideoEncoderOptions = new VideoEncoderOptions
            {
                Bitrate = 4000 * 1000,
                Framerate = 30,
                IsFixedFramerate = false,
                Quality = 70
            },
            AudioOptions = new AudioOptions
            {
                IsAudioEnabled = true
            },
            OutputOptions = new OutputOptions
            {
                RecorderMode = RecorderMode.Video
            }
        };

        _rec = Recorder.CreateRecorder(options);
        _rec.OnRecordingComplete += Rec_OnRecordingComplete;
        _rec.OnRecordingFailed += Rec_OnRecordingFailed;
        _rec.OnStatusChanged += Rec_OnStatusChanged;

        return videoPath;
    }

    public void CreateRecording()
    {
        try
        {
            var videoPath = VideoOptions();
            _rec.Record(videoPath);
        }
        catch (Exception ex)
        {
            _logger.Error("Error creating recording: {a}", ex.Message);
            _logger.Error("Stack trace: {a}", ex.StackTrace);
        }
    }

    public async Task EndRecordingAsync()
    {
        try
        {
            if (_status == RecorderStatus.Recording || _status == RecorderStatus.Paused)
            {
                _rec.Stop();
                await Task.Delay(2000); // Give time for finalizing
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Error ending recording: {a}", ex.Message);
        }
    }

    public void PauseRecording()
    {
        try
        {
            if (_status == RecorderStatus.Recording)
            {
                _rec.Pause();
            }
            else
            {
                _logger.Warning("Cannot pause. Recorder is not in Recording state.");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Error pausing recording: {a}", ex.Message);

        }
    }

    public void ResumeRecording()
    {
        try
        {
            if (_status == RecorderStatus.Paused)
            {
                _rec.Resume();
            }
            else
            {
                Console.WriteLine("Cannot resume. Recorder is not in Paused state.");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Error resuming recording: {a}", ex.Message);
        }
    }

    private void Rec_OnRecordingComplete(object? sender, RecordingCompleteEventArgs e)
    {
        _logger.Information("Recording complete: {a}", e.FilePath);
        if (File.Exists(e.FilePath))
        {
            var fileInfo = new FileInfo(e.FilePath);
            _logger.Information("File size: {a} bytes", fileInfo.Length);
        }
        else
        {
            _logger.Error("File not found at {a}", e.FilePath);

        }
    }

    private void Rec_OnRecordingFailed(object? sender, RecordingFailedEventArgs e)
    {
        _logger.Error("Recording failed: {a}", e.Error);
    }

    private void Rec_OnStatusChanged(object? sender, RecordingStatusEventArgs e)
    {
        _status = e.Status;
        _logger.Information("Recording status changed: {a}", _status);
    }

    public void Dispose()
    {
        _rec?.Dispose();
    }
}
