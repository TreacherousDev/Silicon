using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Silicon
{
    public class GlobalMouseHook : IDisposable
    {
        public event Action OnScrollUp;
        public event Action OnScrollDown;

        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEWHEEL = 0x020A;

        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelMouseProc _proc;

        public GlobalMouseHook()
        {
            _proc = HookCallback;
            _hookId = SetHook(_proc);
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_MOUSEWHEEL)
            {
                int delta = Marshal.ReadInt32(lParam, 8); // offset to mouseData
                short wheelDelta = (short)((delta >> 16) & 0xffff);
                if (wheelDelta > 0)
                    OnScrollUp?.Invoke();
                else if (wheelDelta < 0)
                    OnScrollDown?.Invoke();
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookId);
        }

        #region WinAPI

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion
    }
}
