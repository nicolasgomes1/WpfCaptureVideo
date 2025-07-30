using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
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
        _skElement = new SKElement(); // Create SKElement here
        SetupWindow();
        SetupSkiaSharp();
        SetupEventHandlers();
    }

    private void SetupWindow()
    {
        // Get full virtual screen bounds (all monitors)
        int left = GetSystemMetrics(SM_XVIRTUALSCREEN);
        int top = GetSystemMetrics(SM_YVIRTUALSCREEN);
        int width = GetSystemMetrics(SM_CXVIRTUALSCREEN);
        int height = GetSystemMetrics(SM_CYVIRTUALSCREEN);

        // Set window to cover entire virtual screen
        Left = left;
        Top = top;
        Width = width;
        Height = height;

        Cursor = Cursors.Cross;
        Focusable = true;
        Focus();
    }

    private void SetupSkiaSharp()
    {
        _skElement.PaintSurface += OnPaintSurface;
        Content = _skElement;
    }

    private void SetupEventHandlers()
    {
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
        KeyDown += OnKeyDown;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(this);
        _endPoint = _startPoint;
        _isDrawing = true;
        CaptureMouse();
        _skElement.InvalidateVisual();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_isDrawing)
        {
            _endPoint = e.GetPosition(this);
            _skElement.InvalidateVisual();
        }
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDrawing) return;

        _endPoint = e.GetPosition(this);
        _isDrawing = false;
        ReleaseMouseCapture();

        // Calculate the selected rectangle
        double x = Math.Min(_startPoint.X, _endPoint.X);
        double y = Math.Min(_startPoint.Y, _endPoint.Y);
        double w = Math.Abs(_startPoint.X - _endPoint.X);
        double h = Math.Abs(_startPoint.Y - _endPoint.Y);

        // Only accept selection if it has meaningful size
        if (w > 5 && h > 5)
        {
            SelectedRegion = new Rect(x, y, w, h);
            DialogResult = true;
            Close();
        }
        else
        {
            // Reset if selection is too small
            _skElement.InvalidateVisual();
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
        }
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        
        canvas.Clear(SKColors.Transparent);

        // Draw semi-transparent overlay over entire screen
        using (var overlayPaint = new SKPaint
               {
                   Color = new SKColor(0, 0, 0, 100), // Semi-transparent black
                   Style = SKPaintStyle.Fill
               })
        {
            canvas.DrawRect(new SKRect(0, 0, info.Width, info.Height), overlayPaint);
        }

        if (_isDrawing || (_startPoint != _endPoint && !_isDrawing))
        {
            // Calculate selection rectangle
            float startX = (float)Math.Min(_startPoint.X, _endPoint.X);
            float startY = (float)Math.Min(_startPoint.Y, _endPoint.Y);
            float width = (float)Math.Abs(_startPoint.X - _endPoint.X);
            float height = (float)Math.Abs(_startPoint.Y - _endPoint.Y);

            var selectedRect = new SKRect(startX, startY, startX + width, startY + height);

            // Clear the selected area (make it transparent/normal)
            using (var clearPaint = new SKPaint
                   {
                       BlendMode = SKBlendMode.Clear
                   })
            {
                canvas.DrawRect(selectedRect, clearPaint);
            }

            // Draw selection border
            using (var borderPaint = new SKPaint
                   {
                       Color = SKColors.Red,
                       Style = SKPaintStyle.Stroke,
                       StrokeWidth = 2,
                       IsAntialias = true
                   })
            {
                canvas.DrawRect(selectedRect, borderPaint);
            }

            // Draw size information
            if (width > 50 && height > 20)
            {
                string sizeText = $"{width:F0} × {height:F0}";
                using (var textPaint = new SKPaint
                       {
                           Color = SKColors.White,
                           TextSize = 16,
                           IsAntialias = true,
                           Typeface = SKTypeface.Default
                       })
                {
                    var textBounds = new SKRect();
                    textPaint.MeasureText(sizeText, ref textBounds);
                    
                    float textX = startX + 5;
                    float textY = startY - 5;
                    
                    // Ensure text stays within screen bounds
                    if (textY < textBounds.Height) textY = startY + textBounds.Height + 5;
                    if (textX + textBounds.Width > info.Width) textX = info.Width - textBounds.Width - 5;
                    
                    canvas.DrawText(sizeText, textX, textY, textPaint);
                }
            }
        }
    }
}