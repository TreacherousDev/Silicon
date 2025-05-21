using System;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;


namespace Silicon
{
    public partial class SiliconForm
    {

        private bool isRightDragging = false;
        private Point lastMousePos;
        private Dictionary<Keys, Action> keyBindings = new Dictionary<Keys, Action>();

        private readonly object keyMonitorLock = new object();
        private HashSet<Keys> keysToMonitor = new HashSet<Keys>()
        {
            Keys.W, Keys.S, Keys.A, Keys.D, Keys.Q, Keys.E,
            Keys.ShiftKey, Keys.ControlKey,
            Keys.Up, Keys.Down, Keys.Left, Keys.Right,
            Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11
        };


        private void InitializeKeyBindings()
        {
            keyBindings = new Dictionary<Keys, Action>
            {
                [Keys.F1] = () => Preset1Button_Click(null, EventArgs.Empty),
                [Keys.F2] = () => Preset2Button_Click(null, EventArgs.Empty),
                [Keys.F3] = () => Preset3Button_Click(null, EventArgs.Empty),
                [Keys.F4] = () => Preset4Button_Click(null, EventArgs.Empty),
                [Keys.F5] = () => FreecamSwitch.Switched = !FreecamSwitch.Switched,
                [Keys.F6] = () => AddAnimationFrameButton_Click(null, EventArgs.Empty),
                [Keys.F7] = GoToPreviousFrame,
                [Keys.F8] = GoToNextFrame,
                [Keys.F9] = () => PlayAnimationButton_Click(null, EventArgs.Empty),
                [Keys.F10] = () => HideNametagsSwitch.Switched = !HideNametagsSwitch.Switched,
                [Keys.F11] = () => HideUserInterfaceSwitch.Switched = !HideUserInterfaceSwitch.Switched
            };
        }

        private void InitMouseDrag()
        {
            mouseHook.OnRightDown += (Point pos) =>
            {
                if (IsCubicWindowFocused())
                {
                    isRightDragging = true;
                    lastMousePos = pos;
                }
            };

            mouseHook.OnMouseMove += (Point pos) =>
            {
                if (isRightDragging)
                {
                    int dx = pos.X - lastMousePos.X;
                    int dy = pos.Y - lastMousePos.Y;

                    float sensitivity = (float)CameraRotateSpeedSlider.Value / 1000;
                    currentCameraYaw += dx * sensitivity;
                    currentCameraPitch += dy * sensitivity;
                    currentCameraPitch = Clamp(currentCameraPitch, -89f, 89f);
                    targetCameraYaw = currentCameraYaw;
                    targetCameraPitch = currentCameraPitch;

                    lastMousePos = pos;
                }
            };

            mouseHook.OnRightUp += (Point pos) =>
            {
                isRightDragging = false;
            };
        }

        private void InitMouseScroll()
        {
            mouseHook.OnScrollDown += () =>
            {
                if (!IsCubicWindowFocused())
                    return;

                if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    CameraDistanceSlider.Value = Math.Min(300, CameraDistanceSlider.Value + 1);
                }
                else
                {
                    CameraFOVSlider.Value = Math.Min(135, CameraFOVSlider.Value + 1);
                }
            };

            mouseHook.OnScrollUp += () =>
            {
                if (!IsCubicWindowFocused())
                    return;

                if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    CameraDistanceSlider.Value = Math.Max(2, CameraDistanceSlider.Value - 1);
                }
                else
                {
                    CameraFOVSlider.Value = Math.Max(10, CameraFOVSlider.Value - 1);
                }
            };
        }

        // This function gets called in the main update loop
        // It handles the movement and rotation of the camera when freecam is activated
        // Here starts the code edited by Hispano
        private void StartKeyPolling()
        {
            keyPollingThread = new Thread(() =>
            {
                while (isRunning)
                {
                    UpdateKeyStates();
                    Thread.Sleep(5);
                }
            })
            {
                IsBackground = true
            };
            keyPollingThread.Start();
        }

        public void SetKeysToMonitor(IEnumerable<Keys> newKeys)
        {
            lock (keyMonitorLock)
            {
                keysToMonitor = new HashSet<Keys>(newKeys);
            }
        }

        private void UpdateKeyStates()
        {
            if (!IsCubicWindowFocused() && !IsSiliconWindowFocused())
                return;

            HashSet<Keys> currentKeys;
            lock (keyMonitorLock)
            {
                currentKeys = new HashSet<Keys>(keysToMonitor); // Copy to avoid locking during the loop
            }

            foreach (var key in currentKeys)
            {
                bool isPressed = (GetAsyncKeyState(key) & 0x8000) != 0;

                if (isPressed)
                {
                    if (!pressedKeys.Contains(key))
                    {
                        pressedKeys.Add(key);
                        HandleKeyDown(key);
                    }
                }
                else
                {
                    if (pressedKeys.Contains(key))
                    {
                        pressedKeys.Remove(key);
                        if (pressedKeys.Count == 0)
                        {
                            Thread.Sleep(10);
                        }
                    }
                }
            }
        }


        private void HandleKeyDown(Keys key)
        {
            if (FreecamSwitch.InvokeRequired)
            {
                FreecamSwitch.Invoke(new Action(() => HandleKeyDown(key)));
                return;
            }
            if (HideNametagsSwitch.InvokeRequired)
            {
                HideNametagsSwitch.Invoke(new Action(() => HandleKeyDown(key)));
                return;
            }
            if (HideUserInterfaceSwitch.InvokeRequired)
            {
                HideUserInterfaceSwitch.Invoke(new Action(() => HandleKeyDown(key)));
                return;
            }

            if (keyBindings.TryGetValue(key, out Action action))
            {
                action.Invoke();
            }

            //switch (key)
            //{
            //    case Keys.F1:
            //        Preset1Button_Click(null, EventArgs.Empty);
            //        break;
            //    case Keys.F2:
            //        Preset2Button_Click(null, EventArgs.Empty);
            //        break;
            //    case Keys.F3:
            //        Preset3Button_Click(null, EventArgs.Empty);
            //        break;
            //    case Keys.F4:
            //        Preset4Button_Click(null, EventArgs.Empty);
            //        break;
            //    case Keys.F5:
            //        FreecamSwitch.Switched = !FreecamSwitch.Switched;
            //        break;
            //    case Keys.F6:
            //        AddAnimationFrameButton_Click(null, EventArgs.Empty);
            //        break;
            //    case Keys.F7:
            //        GoToPreviousFrame();
            //        break;
            //    case Keys.F8:
            //        GoToNextFrame();
            //        break;
            //    case Keys.F9:
            //        PlayAnimationButton_Click(null, EventArgs.Empty);
            //        break;
            //    case Keys.F10:
            //        HideNametagsSwitch.Switched = !HideNametagsSwitch.Switched;
            //        break;
            //    case Keys.F11:
            //        HideUserInterfaceSwitch.Switched = !HideUserInterfaceSwitch.Switched;
            //        break;
            //}
        }


    }
}
