using System.Drawing;
using System.Windows.Forms;

namespace CursorHighlight;

public class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly OverlayForm _overlay;
    private readonly System.Windows.Forms.Timer _timer;
    private SettingsForm? _settingsForm;

    public TrayApplicationContext()
    {
        var settings = AppSettings.Load();

        _overlay = new OverlayForm(settings);
        _overlay.Show();

        var menu = new ContextMenuStrip();
        menu.Items.Add("Settings", null, OpenSettings);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitApp());

        _trayIcon = new NotifyIcon
        {
            Icon = CreateTrayIcon(),
            ContextMenuStrip = menu,
            Visible = true,
            Text = "CursorHighlight"
        };
        _trayIcon.DoubleClick += OpenSettings;

        _trayIcon.ShowBalloonTip(2000, "CursorHighlight", "Running in system tray", ToolTipIcon.None);

        _timer = new System.Windows.Forms.Timer { Interval = 16 };
        _timer.Tick += (_, _) => _overlay.Redraw();
        if (settings.IsEnabled)
            _timer.Start();
        else
            _overlay.Clear();

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

        if (settings.IsEnabled)
            _timer.Start();
        else
        {
            _timer.Stop();
            _overlay.Clear();
        }
    }

    private void ExitApp()
    {
        _timer.Stop();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Application.Exit();
    }

    private static Icon CreateTrayIcon()
    {
        using var bmp = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.Transparent);
            using var brush = new SolidBrush(Color.FromArgb(255, 255, 255, 0));
            g.FillEllipse(brush, 1, 1, 13, 13);
            using var pen = new System.Drawing.Pen(Color.FromArgb(180, 200, 200, 0), 1f);
            g.DrawEllipse(pen, 1, 1, 13, 13);
        }
        return Icon.FromHandle(bmp.GetHicon());
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
