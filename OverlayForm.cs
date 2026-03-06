using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CursorHighlight;

public class OverlayForm : Form
{
    // Win32 constants
    private const int GWL_EXSTYLE      = -20;
    private const int WS_EX_LAYERED    = 0x00080000;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const uint ULW_ALPHA       = 0x00000002;
    private const int SWP_NOACTIVATE   = 0x0010;
    private const int SWP_NOZORDER     = 0x0004;
    private static readonly IntPtr HWND_TOPMOST = new(-1);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct SIZE { public int cx; public int cy; }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BLENDFUNCTION
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }

    [DllImport("user32.dll")] private static extern bool GetCursorPos(out POINT lpPoint);
    [DllImport("user32.dll")] private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pptSrc, uint crKey, ref BLENDFUNCTION pblend, uint dwFlags);
    [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("gdi32.dll")]  private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll")]  private static extern bool DeleteDC(IntPtr hdc);
    [DllImport("gdi32.dll")]  private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);
    [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr ho);

    private AppSettings _settings;

    public OverlayForm(AppSettings settings)
    {
        _settings = settings;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        // Size will be set during first Redraw call
        SetStyle(ControlStyles.UserPaint, false);
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
            return cp;
        }
    }

    // Block standard paint — all drawing goes through UpdateLayeredWindow
    protected override void OnPaintBackground(PaintEventArgs e) { }
    protected override void OnPaint(PaintEventArgs e) { }

    public void ApplySettings(AppSettings settings)
    {
        _settings = settings;
        Redraw();
    }

    // Erase the overlay — call when the highlight is disabled
    public void Clear()
    {
        if (!IsHandleCreated) return;
        using var bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var destPoint = new POINT { X = -10, Y = -10 };
        var size      = new SIZE { cx = 1, cy = 1 };
        var srcPoint  = new POINT { X = 0, Y = 0 };
        var blend     = new BLENDFUNCTION { BlendOp = 0, SourceConstantAlpha = 0, AlphaFormat = 1 };

        IntPtr screenDc  = GetDC(IntPtr.Zero);
        IntPtr memDc     = CreateCompatibleDC(screenDc);
        IntPtr hBitmap   = bmp.GetHbitmap(Color.FromArgb(0));
        IntPtr oldBitmap = SelectObject(memDc, hBitmap);
        try
        {
            UpdateLayeredWindow(Handle, screenDc, ref destPoint, ref size,
                memDc, ref srcPoint, 0, ref blend, ULW_ALPHA);
        }
        finally
        {
            SelectObject(memDc, oldBitmap);
            DeleteObject(hBitmap);
            DeleteDC(memDc);
            ReleaseDC(IntPtr.Zero, screenDc);
        }
    }

    public void Redraw()
    {
        if (!IsHandleCreated) return;

        int diameter = _settings.CircleDiameter;
        int margin = 4;
        int w = diameter + margin * 2;
        int h = diameter + margin * 2;

        GetCursorPos(out POINT cursor);

        // Offset so the circle covers the arrow cursor body (extends ~12px right, ~20px down from hotspot)
        var destPoint = new POINT { X = cursor.X - w / 2 + 6, Y = cursor.Y - h / 2 + 10 };
        var size = new SIZE { cx = w, cy = h };
        var srcPoint = new POINT { X = 0, Y = 0 };
        var blend = new BLENDFUNCTION
        {
            BlendOp = 0,   // AC_SRC_OVER
            BlendFlags = 0,
            SourceConstantAlpha = 255,
            AlphaFormat = 1 // AC_SRC_ALPHA
        };

        using var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.FromArgb(0, 0, 0, 0));
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var fillColor = Color.FromArgb(_settings.AlphaValue, _settings.HighlightColor);
            using var brush = new SolidBrush(fillColor);
            g.FillEllipse(brush, margin, margin, diameter, diameter);
        }

        IntPtr screenDc = GetDC(IntPtr.Zero);
        IntPtr memDc = CreateCompatibleDC(screenDc);
        IntPtr hBitmap = bmp.GetHbitmap(Color.FromArgb(0));
        IntPtr oldBitmap = SelectObject(memDc, hBitmap);

        try
        {
            SetWindowPos(Handle, HWND_TOPMOST, destPoint.X, destPoint.Y, w, h,
                SWP_NOACTIVATE);
            UpdateLayeredWindow(Handle, screenDc, ref destPoint, ref size,
                memDc, ref srcPoint, 0, ref blend, ULW_ALPHA);
        }
        finally
        {
            SelectObject(memDc, oldBitmap);
            DeleteObject(hBitmap);
            DeleteDC(memDc);
            ReleaseDC(IntPtr.Zero, screenDc);
        }
    }
}
