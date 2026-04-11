using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DigitalScope.Core;

public sealed class HotkeyManager : IDisposable
{
    [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const uint MOD_ALT      = 0x0001;
    private const uint MOD_CONTROL  = 0x0002;
    private const uint MOD_SHIFT    = 0x0004;
    private const uint MOD_WIN      = 0x0008;
    private const uint MOD_NOREPEAT = 0x4000;

    private const int WM_HOTKEY = 0x0312;

    private IntPtr      _hwnd;
    private HwndSource? _source;
    private readonly Dictionary<int, Action> _callbacks = new();
    private int _nextId = 9000;

    public void Attach(Window window)
    {
        _hwnd   = new WindowInteropHelper(window).EnsureHandle();
        _source = HwndSource.FromHwnd(_hwnd);
        _source.AddHook(WndProc);
        AppLogger.Info("HotkeyManager attached to window handle.");
    }

    public int Register(string hotkey, Action callback)
    {
        if (!ParseHotkey(hotkey, out uint mods, out uint vk))
        {
            AppLogger.Warn($"Cannot parse hotkey: '{hotkey}'");
            return -1;
        }

        int id = _nextId++;
        if (!RegisterHotKey(_hwnd, id, mods | MOD_NOREPEAT, vk))
        {
            AppLogger.Warn($"RegisterHotKey failed for '{hotkey}' (id={id})");
            return -1;
        }

        _callbacks[id] = callback;
        AppLogger.Info($"Hotkey registered: '{hotkey}' → id={id}");
        return id;
    }

    public void Unregister(int id)
    {
        if (_callbacks.Remove(id))
            UnregisterHotKey(_hwnd, id);
    }

    public void UnregisterAll()
    {
        foreach (var id in _callbacks.Keys.ToList())
            UnregisterHotKey(_hwnd, id);
        _callbacks.Clear();
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            if (_callbacks.TryGetValue(id, out var cb))
            {
                cb.Invoke();
                handled = true;
            }
        }
        return IntPtr.Zero;
    }

    private static bool ParseHotkey(string hotkey, out uint mods, out uint vk)
    {
        mods = 0;
        vk   = 0;

        var parts = hotkey.Split('+', StringSplitOptions.RemoveEmptyEntries);
        string? keyPart = null;

        foreach (var p in parts)
        {
            switch (p.Trim().ToLowerInvariant())
            {
                case "ctrl":  case "control": mods |= MOD_CONTROL; break;
                case "shift":                 mods |= MOD_SHIFT;   break;
                case "alt":                   mods |= MOD_ALT;     break;
                case "win":                   mods |= MOD_WIN;     break;
                default:      keyPart = p.Trim(); break;
            }
        }

        if (keyPart is null) return false;

        if (keyPart.Length == 1)
        {
            vk = (uint)char.ToUpperInvariant(keyPart[0]);
            return true;
        }

        vk = keyPart.ToUpperInvariant() switch
        {
            "F1"  => 0x70, "F2"  => 0x71, "F3"  => 0x72, "F4"  => 0x73,
            "F5"  => 0x74, "F6"  => 0x75, "F7"  => 0x76, "F8"  => 0x77,
            "F9"  => 0x78, "F10" => 0x79, "F11" => 0x7A, "F12" => 0x7B,
            "TAB" => 0x09, "SPACE" => 0x20, "RETURN" => 0x0D,
            "INSERT" => 0x2D, "DELETE" => 0x2E,
            "HOME" => 0x24, "END" => 0x23,
            "PAGEUP" => 0x21, "PAGEDOWN" => 0x22,
            _ => 0
        };

        return vk != 0;
    }

    public void Dispose()
    {
        UnregisterAll();
        _source?.RemoveHook(WndProc);
        _source?.Dispose();
    }
}
