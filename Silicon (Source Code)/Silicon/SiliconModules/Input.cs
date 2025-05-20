using System;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;


namespace Silicon
{
    public partial class SiliconForm
    {

        private bool isRightDragging = false;
        private Point lastMousePos;

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

                    float sensitivity = 0.15f;
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
                    Thread.Sleep(10);
                }
            })
            {
                IsBackground = true
            };
            keyPollingThread.Start();
        }

        private void UpdateKeyStates()
        {
            if (!IsCubicWindowFocused() && !IsSiliconWindowFocused())
                return;

            Keys[] keysToMonitor = new Keys[]
            {
                Keys.W, Keys.S, Keys.A, Keys.D, Keys.Q, Keys.E,
                Keys.ShiftKey, Keys.ControlKey,
                Keys.Up, Keys.Down, Keys.Left, Keys.Right,
                Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11
            };

            foreach (var key in keysToMonitor)
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

            switch (key)
            {
                case Keys.F1:
                    Preset1Button_Click(null, EventArgs.Empty);
                    break;
                case Keys.F2:
                    Preset2Button_Click(null, EventArgs.Empty);
                    break;
                case Keys.F3:
                    Preset3Button_Click(null, EventArgs.Empty);
                    break;
                case Keys.F4:
                    Preset4Button_Click(null, EventArgs.Empty);
                    break;
                case Keys.F5:
                    FreecamSwitch.Switched = !FreecamSwitch.Switched;
                    break;
                case Keys.F6:
                    AddAnimationFrameButton_Click(null, EventArgs.Empty);
                    break;
                case Keys.F7:
                    GoToPreviousFrame();
                    break;
                case Keys.F8:
                    GoToNextFrame();
                    break;
                case Keys.F9:
                    PlayAnimationButton_Click(null, EventArgs.Empty);
                    break;
                case Keys.F10:
                    HideNametagsSwitch.Switched = !HideNametagsSwitch.Switched;
                    break;
                case Keys.F11:
                    HideUserInterfaceSwitch.Switched = !HideUserInterfaceSwitch.Switched;
                    break;
            }
        }


    }
}
