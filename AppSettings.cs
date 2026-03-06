using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Win32;

namespace CursorHighlight;

public class AppSettings
{
    public int CircleDiameter { get; set; } = 64;
    public string ColorHex { get; set; } = "#FFFF00";
    public int OpacityPercent { get; set; } = 50;
    public bool IsEnabled { get; set; } = true;
    public bool HasAskedStartup { get; set; } = false;

    [JsonIgnore]
    public Color HighlightColor => ColorTranslator.FromHtml(ColorHex);

    [JsonIgnore]
    public byte AlphaValue => (byte)(OpacityPercent * 255 / 100);

    private static string SettingsPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CursorHighlight",
            "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var s = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                s.Clamp();
                return s;
            }
        }
        catch { }
        return new AppSettings();
    }

    private void Clamp()
    {
        CircleDiameter = Math.Clamp(CircleDiameter, 10, 400);
        OpacityPercent = Math.Clamp(OpacityPercent, 10, 90);

        // Validate hex colour — fall back to default if unreadable
        try { ColorTranslator.FromHtml(ColorHex); }
        catch { ColorHex = "#FFFF00"; }
    }

    public static void Save(AppSettings s)
    {
        var dir = Path.GetDirectoryName(SettingsPath)!;
        Directory.CreateDirectory(dir);
        File.WriteAllText(SettingsPath,
            JsonSerializer.Serialize(s, new JsonSerializerOptions { WriteIndented = true }));
    }

    // ── Windows startup registry ─────────────────────────────────────────────
    private const string RunRegKey  = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppRegName = "CursorHighlight";

    public static bool IsStartupEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunRegKey);
        return key?.GetValue(AppRegName) != null;
    }

    public static void SetStartup(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunRegKey, writable: true);
        if (key == null) return;
        if (enable)
        {
            var path = Environment.ProcessPath;
            if (string.IsNullOrEmpty(path)) return;
            key.SetValue(AppRegName, path);
        }
        else
            key.DeleteValue(AppRegName, throwOnMissingValue: false);
    }
}
