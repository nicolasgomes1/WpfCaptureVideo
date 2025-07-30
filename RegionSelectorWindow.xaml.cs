using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using SkiaSharp.Views.Desktop;
using Point = System.Windows.Point;

namespace WpfRecorder;

public partial class RegionSelectorWindow : Window
{
    public Rect SelectedRegion { get; private set; }

    private Point _startPoint;
    private Point _endPoint;
    private bool _isDrawing;

    private readonly SKElement _skElement;

    
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    private const int SM_XVIRTUALSCREEN = 76;
    private const int SM_YVIRTUALSCREEN = 77;
    private const int SM_CXVIRTUALSCREEN = 78;
    private const int SM_CYVIRTUALSCREEN = 79;
    
    public RegionSelectorWindow()
    {
        InitializeComponent();

        Cursor = Cursors.Cross;

        // Get full virtual screen bounds (all monitors + taskbar)
        int left = GetSystemMetrics(SM_XVIRTUALSCREEN);
        int top = GetSystemMetrics(SM_YVIRTUALSCREEN);
        int width = GetSystemMetrics(SM_CXVIRTUALSCREEN);
        int height = GetSystemMetrics(SM_CYVIRTUALSCREEN);

        Left = left;
        Top = top;
        Width = width;
        Height = height;

        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        Topmost = true;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;

        // Your SkiaSharp setup and event handlers here
        _skElement = new SKElement();
        _skElement.PaintSurface += OnPaintSurface;
        Content = _skElement;

        MouseLeftButtonDown += (s, e) =>
        {
            _startPoint = e.GetPosition(this);
            _isDrawing = true;
        };

        MouseMove += (s, e) =>
        {
            if (_isDrawing)
            {
                _endPoint = e.GetPosition(this);
                _skElement.InvalidateVisual();
            }
        };

        MouseLeftButtonUp += (s, e) =>
        {
            _endPoint = e.GetPosition(this);
            _isDrawing = false;

            double x = Math.Min(_startPoint.X, _endPoint.X);
            double y = Math.Min(_startPoint.Y, _endPoint.Y);
            double w = Math.Abs(_startPoint.X - _endPoint.X);
            double h = Math.Abs(_startPoint.Y - _endPoint.Y);

            SelectedRegion = new Rect(x, y, w, h);
            DialogResult = true;
        };
    }


    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (!_isDrawing) return;

        float startX = (float)Math.Min(_startPoint.X, _endPoint.X);
        float startY = (float)Math.Min(_startPoint.Y, _endPoint.Y);
        float endX = (float)Math.Max(_startPoint.X, _endPoint.X);
        float endY = (float)Math.Max(_startPoint.Y, _endPoint.Y);

        var selectedRect = new SKRect(startX, startY, endX, endY);

        // Draw dimmed overlay
        using (var dimPaint = new SKPaint
               {
                   Color = new SKColor(0, 0, 0, 128), // semi-transparent black
                   Style = SKPaintStyle.Fill
               })
        {
            canvas.DrawRect(new SKRect(0, 0, e.Info.Width, e.Info.Height), dimPaint);
        }

        // Clear the selection area (hole punch)
        canvas.SaveLayer();
        using (var clearPaint = new SKPaint
               {
                   BlendMode = SKBlendMode.Clear
               })
        {
            canvas.DrawRect(selectedRect, clearPaint);
        }
        canvas.Restore();

        // Draw the selection border
        using (var borderPaint = new SKPaint
               {
                   Color = SKColors.Aqua,
                   Style = SKPaintStyle.Stroke,
                   StrokeWidth = 2,
                   IsAntialias = true
               })
        {
            canvas.DrawRect(selectedRect, borderPaint);
        }
    }

}
