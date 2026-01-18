using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;

namespace ClickLogger.Model
{
    public class Locator
    {
        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern IntPtr WindowFromPoint(Point pt);

            [DllImport("user32.dll")]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        }

        public static IntPtr GetWindowHandleFromPoint(int x, int y)
        {
            return NativeMethods.WindowFromPoint(new Point(x, y));
        }

        public static string GetWindowTitleFromWindowHandle(IntPtr hWnd)
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            if (NativeMethods.GetWindowText(hWnd, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return string.Empty;
        }

        public static string GetAutomationIdAndNameFromWindowHandle(IntPtr hWnd)
        {
            try
            {
                AutomationElement element = AutomationElement.FromHandle(hWnd);
                var automationId = element.Current.AutomationId;
                var name = element.Current.Name;
                
                return !string.IsNullOrEmpty(name) ? $"{automationId}|{name}" : $"{automationId}{name}";
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetProcessNameFromWindowHandle(IntPtr hWnd)
        {
            NativeMethods.GetWindowThreadProcessId(hWnd, out uint processId);
            try
            {
                var process = System.Diagnostics.Process.GetProcessById((int)processId);
                return process.ProcessName;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}