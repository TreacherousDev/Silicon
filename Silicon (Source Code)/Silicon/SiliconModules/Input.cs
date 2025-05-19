using System;
using System.Threading;
using System.Windows.Forms;

namespace Silicon
{
    public partial class SiliconForm
    {
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
                Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8
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


        private bool IsRightMouseButtonDown()
        {
            if (!IsCubicWindowFocused())
                return false;

            return (GetAsyncKeyState(Keys.RButton) & 0x8000) != 0;
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
                    FreecamSwitch.Switched = !FreecamSwitch.Switched;
                    break;
                case Keys.F5:
                    AddAnimationFrameButton_Click(null, EventArgs.Empty);
                    break;
                case Keys.F6:
                    GoToPreviousFrame();
                    break;
                case Keys.F7:
                    GoToNextFrame();
                    break;
                case Keys.F8:
                    PlayAnimationButton_Click(null, EventArgs.Empty);
                    break;
            }
        }


    }
}
