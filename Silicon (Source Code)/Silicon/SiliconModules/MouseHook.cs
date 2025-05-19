using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Silicon
{
    public class GlobalMouseHook : IDisposable
    {
        public event Action OnScrollUp;
        public event Action OnScrollDown;
        public event Action<Point> OnMouseMove;
        public event Action<Point> OnRightDown;
        public event Action<Point> OnRightUp;

        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
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
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                Point pt = new Point(hookStruct.pt.x, hookStruct.pt.y);

                switch ((int)wParam)
                {
                    case WM_MOUSEMOVE:
                        OnMouseMove?.Invoke(pt);
                        break;

                    case WM_RBUTTONDOWN:
                        OnRightDown?.Invoke(pt);
                        break;

                    case WM_RBUTTONUP:
                        OnRightUp?.Invoke(pt);
                        break;

                    case WM_MOUSEWHEEL:
                        int delta = Marshal.ReadInt32(lParam, 8);
                        short wheelDelta = (short)((delta >> 16) & 0xffff);
                        if (wheelDelta > 0)
                            OnScrollUp?.Invoke();
                        else if (wheelDelta < 0)
                            OnScrollDown?.Invoke();
                        break;
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookId);
        }

        #region WinAPI

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

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
