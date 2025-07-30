namespace WpfRecorder.Services;

using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

public class ScreenShootWrapper
{
    public void TakeScreenshot(string filePath)
    {
        // Get screen dimensions
        int screenWidth = GetSystemMetrics(0); // SM_CXSCREEN
        int screenHeight = GetSystemMetrics(1); // SM_CYSCREEN
        int bytesPerPixel = 4;

        int stride = screenWidth * bytesPerPixel;
        int bufferSize = stride * screenHeight;
        IntPtr screenDC = GetDC(IntPtr.Zero);
        IntPtr memDC = CreateCompatibleDC(screenDC);

        IntPtr hBitmap = CreateCompatibleBitmap(screenDC, screenWidth, screenHeight);
        IntPtr oldBitmap = SelectObject(memDC, hBitmap);

        BitBlt(memDC, 0, 0, screenWidth, screenHeight, screenDC, 0, 0, SRCCOPY);

        BITMAP bmp = new BITMAP();
        GetObject(hBitmap, Marshal.SizeOf(bmp), ref bmp);

        byte[] pixelData = new byte[bufferSize];

        BITMAPINFO bmi = new BITMAPINFO();
        bmi.bmiHeader.biSize = Marshal.SizeOf(typeof(BITMAPINFOHEADER));
        bmi.bmiHeader.biWidth = screenWidth;
        bmi.bmiHeader.biHeight = -screenHeight; // Top-down
        bmi.bmiHeader.biPlanes = 1;
        bmi.bmiHeader.biBitCount = 32;
        bmi.bmiHeader.biCompression = 0; // BI_RGB

        // Get raw pixel data
        GetDIBits(memDC, hBitmap, 0, (uint)screenHeight, pixelData, ref bmi, 0);

        // Cleanup GDI objects
        SelectObject(memDC, oldBitmap);
        DeleteObject(hBitmap);
        DeleteDC(memDC);
        ReleaseDC(IntPtr.Zero, screenDC);

        // Use SkiaSharp to encode as PNG
        var info = new SKImageInfo(screenWidth, screenHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var skBitmap = new SKBitmap();
        var handle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
        try
        {
            skBitmap.InstallPixels(info, handle.AddrOfPinnedObject(), info.RowBytes, null, () => handle.Free());
        }
        catch
        {
            handle.Free(); // In case of exception
            throw;
        }
        using var image = SKImage.FromBitmap(skBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(filePath);
        data.SaveTo(stream);
    }

    public static void TakeScreenshotRegion(string filePath, int x, int y, int width, int height)
    {
        int bytesPerPixel = 4;
        int stride = width * bytesPerPixel;
        int bufferSize = stride * height;

        IntPtr screenDC = GetDC(IntPtr.Zero);
        IntPtr memDC = CreateCompatibleDC(screenDC);
        IntPtr hBitmap = CreateCompatibleBitmap(screenDC, width, height);
        IntPtr oldBitmap = SelectObject(memDC, hBitmap);

        BitBlt(memDC, 0, 0, width, height, screenDC, x, y, SRCCOPY);

        byte[] pixelData = new byte[bufferSize];

        BITMAPINFO bmi = new BITMAPINFO();
        bmi.bmiHeader.biSize = Marshal.SizeOf(typeof(BITMAPINFOHEADER));
        bmi.bmiHeader.biWidth = width;
        bmi.bmiHeader.biHeight = -height;
        bmi.bmiHeader.biPlanes = 1;
        bmi.bmiHeader.biBitCount = 32;
        bmi.bmiHeader.biCompression = 0;

        GetDIBits(memDC, hBitmap, 0, (uint)height, pixelData, ref bmi, 0);

        SelectObject(memDC, oldBitmap);
        DeleteObject(hBitmap);
        DeleteDC(memDC);
        ReleaseDC(IntPtr.Zero, screenDC);

        var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var skBitmap = new SKBitmap();
        var handle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
        try
        {
            skBitmap.InstallPixels(info, handle.AddrOfPinnedObject(), info.RowBytes, null, () => handle.Free());
        }
        catch
        {
            handle.Free();
            throw;
        }

        using var image = SKImage.FromBitmap(skBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(filePath);
        data.SaveTo(stream);
    }

    
    private const int SRCCOPY = 0x00CC0020;

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest,
        int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern int GetObject(IntPtr hgdiobj, int cbBuffer, ref BITMAP lpvObject);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines,
        [Out] byte[] lpvBits, ref BITMAPINFO lpbi, uint uUsage);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAP
    {
        public int bmType;
        public int bmWidth;
        public int bmHeight;
        public int bmWidthBytes;
        public ushort bmPlanes;
        public ushort bmBitsPixel;
        public IntPtr bmBits;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public uint[] bmiColors;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFOHEADER
    {
        public int biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes;
        public short biBitCount;
        public int biCompression;
        public int biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public int biClrUsed;
        public int biClrImportant;
    }
}
