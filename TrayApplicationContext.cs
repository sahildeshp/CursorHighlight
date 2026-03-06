using System.Drawing;
using System.Windows.Forms;

namespace CursorHighlight;

public class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly OverlayForm _overlay;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly ToolStripMenuItem _toggleItem;
    private SettingsForm? _settingsForm;

    public TrayApplicationContext()
    {
        var settings = AppSettings.Load();

        _overlay = new OverlayForm(settings);
        _overlay.Show();

        _toggleItem = new ToolStripMenuItem();
        _toggleItem.Click += ToggleHighlight;

        var menu = new ContextMenuStrip();
        menu.Items.Add(_toggleItem);
        menu.Items.Add("Settings", null, OpenSettings);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitApp());

        _trayIcon = new NotifyIcon
        {
            ContextMenuStrip = menu,
            Visible = true,
            Text = "CursorHighlight"
        };
        _trayIcon.DoubleClick += OpenSettings;

        _timer = new System.Windows.Forms.Timer { Interval = 16 };
        _timer.Tick += (_, _) => _overlay.Redraw();

        UpdateTrayState(settings.IsEnabled);

        _trayIcon.ShowBalloonTip(2000, "CursorHighlight", "Running in system tray", ToolTipIcon.None);

        // First-run: ask whether to start with Windows
        if (!settings.HasAskedStartup)
        {
            settings.HasAskedStartup = true;
            AppSettings.Save(settings);

            var answer = MessageBox.Show(
                "Would you like CursorHighlight to start automatically with Windows?\n\nYou can change this later in Settings.",
                "CursorHighlight",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            AppSettings.SetStartup(answer == DialogResult.Yes);
        }
    }

    private void ToggleHighlight(object? sender, EventArgs e)
    {
        var settings = AppSettings.Load();
        settings.IsEnabled = !settings.IsEnabled;
        AppSettings.Save(settings);
        _overlay.ApplySettings(settings);
        UpdateTrayState(settings.IsEnabled);

        if (settings.IsEnabled)
            _timer.Start();
        else
        {
            _timer.Stop();
            _overlay.Clear();
        }

        // Sync the open settings form if visible
        if (_settingsForm != null && !_settingsForm.IsDisposed)
            _settingsForm.SyncEnabled(settings.IsEnabled);
    }

    private void UpdateTrayState(bool enabled)
    {
        _toggleItem.Text = enabled ? "Disable Highlight" : "Enable Highlight";

        var oldIcon = _trayIcon.Icon;
        _trayIcon.Icon = CreateTrayIcon(enabled);
        oldIcon?.Dispose();

        if (enabled)
            _timer.Start();
        else
        {
            _timer.Stop();
            _overlay.Clear();
        }
    }

    private void OpenSettings(object? sender, EventArgs e)
    {
        if (_settingsForm == null || _settingsForm.IsDisposed)
        {
            _settingsForm = new SettingsForm(AppSettings.Load());
            _settingsForm.SettingsChanged += OnSettingsChanged;
            _settingsForm.Show();
        }
        else
        {
            _settingsForm.BringToFront();
            _settingsForm.Activate();
        }
    }

    private void OnSettingsChanged(AppSettings settings)
    {
        AppSettings.Save(settings);
        _overlay.ApplySettings(settings);
        UpdateTrayState(settings.IsEnabled);
    }

    private void ExitApp()
    {
        _timer.Stop();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Application.Exit();
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private static Icon CreateTrayIcon(bool enabled)
    {
        var fillColor = enabled
            ? Color.FromArgb(255, 255, 255, 0)   // bright yellow
            : Color.FromArgb(255, 120, 120, 120); // gray

        using var bmp = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.Transparent);
            using var brush = new SolidBrush(fillColor);
            g.FillEllipse(brush, 1, 1, 13, 13);
        }
        IntPtr hIcon = bmp.GetHicon();
        var icon = (Icon)Icon.FromHandle(hIcon).Clone();
        DestroyIcon(hIcon);
        return icon;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Dispose();
            _trayIcon.Dispose();
            _overlay.Dispose();
        }
        base.Dispose(disposing);
    }
}
