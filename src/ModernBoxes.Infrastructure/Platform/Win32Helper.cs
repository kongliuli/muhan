using System;
using System.Runtime.InteropServices;

namespace ModernBoxes.Infrastructure
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    public static class Win32Helper
    {
        public static bool IsWindows11OrGreater => Environment.OSVersion.Version.Build >= 22000;

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_MICA = 1029;
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        public static bool TryEnableMica(IntPtr hwnd)
        {
            if (!IsWindows11OrGreater) return false;

            try
            {
                int backdropType = 2;
                int hr = DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));
                if (hr == 0) return true;

                int mica = 1;
                hr = DwmSetWindowAttribute(hwnd, DWMWA_MICA, ref mica, sizeof(int));
                return hr == 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryEnableAcrylic(IntPtr hwnd)
        {
            try
            {
                int darkMode = 1;
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
                return true;
            }
            catch { return false; }
        }

        public static void EnableMaterial(IntPtr hwnd)
        {
            if (!TryEnableMica(hwnd))
                TryEnableAcrylic(hwnd);
        }
    }
}
