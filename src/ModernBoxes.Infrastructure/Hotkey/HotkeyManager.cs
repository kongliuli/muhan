using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace ModernBoxes.Infrastructure
{
    public class HotkeyManager : IDisposable
    {
        private static HotkeyManager? _instance;
        public static HotkeyManager Instance => _instance ??= new HotkeyManager();

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;

        private IntPtr _hwnd;
        private HwndSource? _source;
        private int _nextId = 1;
        private readonly Dictionary<int, Action> _actions = new();
        private readonly Dictionary<int, (uint mod, uint key)> _registered = new();

        public void Initialize(Window window)
        {
            _hwnd = new WindowInteropHelper(window).Handle;
            _source = HwndSource.FromHwnd(_hwnd);
            _source?.AddHook(WndProc);
        }

        public bool RegisterHotkeyAction(uint modifiers, uint key, Action action)
        {
            if (_registered.Values.Any(r => r.mod == modifiers && r.key == key))
                return false;

            int id = _nextId++;
            if (RegisterHotKey(_hwnd, id, modifiers, key))
            {
                _actions[id] = action;
                _registered[id] = (mod: modifiers, key: key);
                return true;
            }
            _nextId--;
            return false;
        }

        public void UnregisterAll()
        {
            foreach (var id in _actions.Keys)
                UnregisterHotKey(_hwnd, id);
            _actions.Clear();
            _registered.Clear();
        }

        public static uint KeyToVk(Key key) => (uint)KeyInterop.VirtualKeyFromKey(key);

        public static bool TryParseHotkeyString(string hotkey, out uint modifiers, out uint vk)
        {
            modifiers = 0;
            vk = 0;

            if (string.IsNullOrWhiteSpace(hotkey))
                return false;

            var parts = hotkey.Split('+');
            if (parts.Length < 2)
                return false;

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                switch (trimmed)
                {
                    case "Ctrl":
                        modifiers |= MOD_CONTROL;
                        break;
                    case "Shift":
                        modifiers |= MOD_SHIFT;
                        break;
                    case "Alt":
                        modifiers |= MOD_ALT;
                        break;
                    case "Win":
                        modifiers |= MOD_WIN;
                        break;
                    default:
                        if (trimmed.Length == 1 && char.IsLetterOrDigit(trimmed[0]))
                        {
                            vk = (uint)KeyInterop.VirtualKeyFromKey(
                                (Key)Enum.Parse(typeof(Key), trimmed.ToUpper()));
                        }
                        else if (Enum.TryParse<Key>(trimmed, true, out var k))
                        {
                            vk = (uint)KeyInterop.VirtualKeyFromKey(k);
                        }
                        else
                        {
                            return false;
                        }
                        break;
                }
            }

            return modifiers > 0 && vk > 0;
        }

        public static string KeyComboToString(uint modifiers, uint vk)
        {
            var parts = new System.Text.StringBuilder();
            if ((modifiers & MOD_CONTROL) != 0) parts.Append("Ctrl+");
            if ((modifiers & MOD_SHIFT) != 0) parts.Append("Shift+");
            if ((modifiers & MOD_ALT) != 0) parts.Append("Alt+");
            if ((modifiers & MOD_WIN) != 0) parts.Append("Win+");
            var key = KeyInterop.KeyFromVirtualKey((int)vk);
            parts.Append(key.ToString());
            return parts.ToString();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (_actions.TryGetValue(id, out var action))
                {
                    action?.Invoke();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            _source?.RemoveHook(WndProc);
            UnregisterAll();
            _source?.Dispose();
        }
    }
}
