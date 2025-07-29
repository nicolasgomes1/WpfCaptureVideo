using System.IO;
using ScreenRecorderLib;

namespace WpfRecorder.Services;

public class RecorderWrapper : IDisposable
{
    private Recorder _rec;
    private RecorderStatus _status = RecorderStatus.Idle;

    private string VideoOptions()
    {
        string videoPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test1.mp4");
        Console.WriteLine($"Recording to: {videoPath}");

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
                IsAudioEnabled = false
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
            Console.WriteLine($"Error creating recording: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
            Console.WriteLine($"Error stopping recording: {ex.Message}");
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
                Console.WriteLine("Cannot pause. Recorder is not in Recording state.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error pausing recording: {ex.Message}");
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
            Console.WriteLine($"Error resuming recording: {ex.Message}");
        }
    }

    private void Rec_OnRecordingComplete(object? sender, RecordingCompleteEventArgs e)
    {
        Console.WriteLine($"Recording complete: {e.FilePath}");
        if (File.Exists(e.FilePath))
        {
            var fileInfo = new FileInfo(e.FilePath);
            Console.WriteLine($"File size: {fileInfo.Length} bytes");
        }
        else
        {
            Console.WriteLine($"Warning: File not found at {e.FilePath}");
        }
    }

    private void Rec_OnRecordingFailed(object? sender, RecordingFailedEventArgs e)
    {
        Console.WriteLine($"Recording failed: {e.Error}");
    }

    private void Rec_OnStatusChanged(object? sender, RecordingStatusEventArgs e)
    {
        _status = e.Status;
        Console.WriteLine($"Recording status changed: {_status}");
    }

    public void Dispose()
    {
        _rec?.Dispose();
    }
}
