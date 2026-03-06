using System.Drawing;
using System.Windows.Forms;

namespace CursorHighlight;

public class SettingsForm : Form
{
    public event Action<AppSettings>? SettingsChanged;

    private readonly CheckBox _enableCheck;
    private readonly TrackBar _sizeTrack;
    private readonly Label _sizeValueLabel;
    private readonly Panel _colorSwatch;
    private readonly Label _colorHexLabel;
    private readonly TrackBar _opacityTrack;
    private readonly Label _opacityValueLabel;

    private Color _selectedColor;
    private readonly bool _hasAskedStartup;

    private static readonly Color BgColor    = Color.FromArgb(24, 24, 24);
    private static readonly Color AccentText = Color.FromArgb(200, 200, 200);
    private static readonly Color DimText    = Color.FromArgb(120, 120, 120);

    public SettingsForm(AppSettings settings)
    {
        _selectedColor   = settings.HighlightColor;
        _hasAskedStartup = settings.HasAskedStartup;

        Text = "CursorHighlight";
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(400, 336);
        BackColor = BgColor;
        ForeColor = AccentText;
        Font = new Font("Segoe UI", 10.5f);

        const int labelX  = 22;
        const int labelW  = 76;
        const int ctrlX   = 108;
        const int ctrlW   = 238;
        const int valX    = 352;
        const int valW    = 42;
        const int rowH    = 58;
        const int trackH  = 38;
        const int swatchH = 40;

        int y = 16;

        // ── Enable toggle ────────────────────────────────────────────────────
        _enableCheck = new CheckBox
        {
            Text = "Highlight enabled",
            Checked = settings.IsEnabled,
            Left = labelX, Top = y + 2, Width = 220, Height = 28,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = AccentText,
            BackColor = BgColor
        };
        _enableCheck.CheckedChanged += (_, _) => FireChanged();

        // ── Size ────────────────────────────────────────────────────────────
        y += 48;
        AddRowLabel("Size", labelX, y + 10, labelW);
        _sizeValueLabel = MakeValueLabel($"{settings.CircleDiameter}px", valX, y + 10, valW);
        _sizeTrack = new TrackBar
        {
            Minimum = 20, Maximum = 200, Value = settings.CircleDiameter,
            TickFrequency = 20, SmallChange = 2, LargeChange = 20,
            Left = ctrlX, Top = y, Width = ctrlW, Height = trackH,
            BackColor = BgColor
        };
        _sizeTrack.ValueChanged += (_, _) =>
        {
            _sizeValueLabel.Text = $"{_sizeTrack.Value}px";
            FireChanged();
        };

        // ── Color ────────────────────────────────────────────────────────────
        y += rowH;
        AddRowLabel("Color", labelX, y + 12, labelW);
        _colorSwatch = new Panel
        {
            BackColor = _selectedColor,
            Left = ctrlX, Top = y, Width = ctrlW, Height = swatchH,
            Cursor = Cursors.Hand
        };
        _colorSwatch.Click += PickColor;
        _colorHexLabel = new Label
        {
            Text = ColorToHex(_selectedColor),
            Left = ctrlX, Top = y, Width = ctrlW, Height = swatchH,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(180, 0, 0, 0),
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _colorHexLabel.Click += PickColor;

        // ── Opacity ──────────────────────────────────────────────────────────
        y += rowH;
        AddRowLabel("Opacity", labelX, y + 10, labelW);
        _opacityValueLabel = MakeValueLabel($"{settings.OpacityPercent}%", valX, y + 10, valW);
        _opacityTrack = new TrackBar
        {
            Minimum = 10, Maximum = 90, Value = settings.OpacityPercent,
            TickFrequency = 10, SmallChange = 5, LargeChange = 10,
            Left = ctrlX, Top = y, Width = ctrlW, Height = trackH,
            BackColor = BgColor
        };
        _opacityTrack.ValueChanged += (_, _) =>
        {
            _opacityValueLabel.Text = $"{_opacityTrack.Value}%";
            FireChanged();
        };

        // ── Run at startup ───────────────────────────────────────────────────
        y += rowH + 8;
        var startupCheck = new CheckBox
        {
            Text = "Run at Windows startup",
            Checked = AppSettings.IsStartupEnabled(),
            Left = labelX, Top = y, Width = 240, Height = 26,
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = DimText,
            BackColor = BgColor
        };
        startupCheck.CheckedChanged += (_, _) => AppSettings.SetStartup(startupCheck.Checked);

        // ── Close button ─────────────────────────────────────────────────────
        y += 42;
        var closeBtn = new Button
        {
            Text = "Close",
            Left = (400 - 120) / 2, Top = y,
            Width = 120, Height = 36,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = AccentText,
            Cursor = Cursors.Hand
        };
        closeBtn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
        closeBtn.Click += (_, _) => Close();

        Controls.AddRange(new Control[]
        {
            _enableCheck,
            _sizeTrack, _sizeValueLabel,
            _colorSwatch, _colorHexLabel,
            _opacityTrack, _opacityValueLabel,
            startupCheck,
            closeBtn
        });
    }

    private void AddRowLabel(string text, int x, int y, int w)
    {
        Controls.Add(new Label
        {
            Text = text,
            Left = x, Top = y, Width = w, Height = 22,
            ForeColor = DimText,
            Font = new Font("Segoe UI", 9f)
        });
    }

    private Label MakeValueLabel(string text, int x, int y, int w)
    {
        return new Label
        {
            Text = text,
            Left = x, Top = y, Width = w, Height = 22,
            ForeColor = AccentText,
            TextAlign = ContentAlignment.MiddleRight
        };
    }

    private void PickColor(object? sender, EventArgs e)
    {
        using var dlg = new ColorDialog { Color = _selectedColor, FullOpen = true };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _selectedColor = dlg.Color;
            _colorSwatch.BackColor = _selectedColor;
            _colorHexLabel.Text = ColorToHex(_selectedColor);
            FireChanged();
        }
    }

    private void FireChanged()
    {
        SettingsChanged?.Invoke(new AppSettings
        {
            IsEnabled      = _enableCheck.Checked,
            CircleDiameter = _sizeTrack.Value,
            ColorHex       = ColorToHex(_selectedColor),
            OpacityPercent = _opacityTrack.Value,
            HasAskedStartup = _hasAskedStartup
        });
    }

    private static string ColorToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
}
