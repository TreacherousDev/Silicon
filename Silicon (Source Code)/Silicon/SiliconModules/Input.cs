using System;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;


namespace Silicon
{
    public partial class SiliconForm
    {

        private bool isRightDragging = false;
        private Point lastMousePos;


        private HashSet<Keys> pressedKeys = new HashSet<Keys>();      
        private Dictionary<Keys, (Action onPress, Action onRelease)> keyBindings;       
        private object keyMonitorLock = new object();
        private static readonly Action NoOp = () => { };

        private static readonly Keys[] ALL_MONITORED_KEYS = GetAllMonitorableKeys();

        private static Keys[] GetAllMonitorableKeys()
        {
            var allKeys = Enum.GetValues(typeof(Keys)).Cast<Keys>().ToList();
            return allKeys.ToArray();
        }

        public class MovementState
        {
            public bool Forward, Backward, Left, Right;
            public bool Up, Down;
            public bool YawLeft, YawRight, PitchUp, PitchDown;
            public bool RollLeft, RollRight;
        }
        private MovementState movementState = new MovementState();

        private void InitializeKeyBindings()
        {
            keyBindings = new Dictionary<Keys, (Action onPress, Action onRelease)>
            {
                [Keys.W] = (() => movementState.Forward = true, () => movementState.Forward = false),
                [Keys.S] = (() => movementState.Backward = true, () => movementState.Backward = false),
                [Keys.A] = (() => movementState.Left = true, () => movementState.Left = false),
                [Keys.D] = (() => movementState.Right = true, () => movementState.Right = false),
                [Keys.ShiftKey] = (() => movementState.Down = true, () => movementState.Down = false),
                [Keys.ControlKey] = (() => movementState.Up = true, () => movementState.Up = false),
                [Keys.Up] = (() => movementState.PitchUp = true, () => movementState.PitchUp = false),
                [Keys.Down] = (() => movementState.PitchDown = true, () => movementState.PitchDown = false),
                [Keys.Left] = (() => movementState.YawLeft = true, () => movementState.YawLeft = false),
                [Keys.Right] = (() => movementState.YawRight = true, () => movementState.YawRight = false),
                [Keys.E] = (() => movementState.RollLeft = true, () => movementState.RollLeft = false),
                [Keys.Q] = (() => movementState.RollRight = true, () => movementState.RollRight = false),

                // Instant/toggle actions – use NoOp for release
                [Keys.F1] = (() => Preset1Button_Click(null, EventArgs.Empty), NoOp),
                [Keys.F2] = (() => Preset2Button_Click(null, EventArgs.Empty), NoOp),
                [Keys.F3] = (() => Preset3Button_Click(null, EventArgs.Empty), NoOp),
                [Keys.F4] = (() => Preset4Button_Click(null, EventArgs.Empty), NoOp),
                [Keys.F5] = (() => FreecamSwitch.Switched = !FreecamSwitch.Switched, NoOp),
                [Keys.F6] = (() => HideNametagsSwitch.Switched = !HideNametagsSwitch.Switched, NoOp),
                [Keys.F7] = (() => HideUserInterfaceSwitch.Switched = !HideUserInterfaceSwitch.Switched, NoOp),
                [Keys.F8] = (() => AddAnimationFrameButton_Click(null, EventArgs.Empty), NoOp),
                [Keys.F9] = (GoToPreviousFrame, NoOp),
                [Keys.F10] = (GoToNextFrame, NoOp),
                [Keys.F11] = (() => PlayAnimationButton_Click(null, EventArgs.Empty), NoOp)   
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
            // Check if our window has focus before processing keys
            if (!IsCubicWindowFocused() && !IsSiliconWindowFocused())
                return;

            HashSet<Keys> currentPressedKeys = new HashSet<Keys>();

            foreach (var key in ALL_MONITORED_KEYS)
            {
                bool isPressed = (GetAsyncKeyState(key) & 0x8000) != 0;

                if (isPressed)
                {
                    currentPressedKeys.Add(key);

                    // Detect newly pressed key
                    if (!pressedKeys.Contains(key))
                    {
                        HandleKeyDown(key);
                    }
                }
            }

            // Detect key releases
            foreach (var key in pressedKeys)
            {
                if (!currentPressedKeys.Contains(key))
                {
                    HandleKeyUp(key);
                }
            }

            // Update pressedKeys to current state
            pressedKeys = currentPressedKeys;

            // Small delay to prevent high CPU usage if no keys pressed
            if (pressedKeys.Count == 0)
            {
                Thread.Sleep(10);
            }
        }

        private void HandleKeyDown(Keys key)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => HandleKeyDown(key)));
                return;
            }

            if (keyBindings.TryGetValue(key, out var action))
            {
                action.onPress();
            }
        }

        void HandleKeyUp(Keys key)
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action(() => HandleKeyUp(key)));
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (InvalidOperationException)
                {
                    return;
                }
                return;
            }

            if (keyBindings.TryGetValue(key, out var action))
            {
                action.onRelease();
            }
        }
    }
}
