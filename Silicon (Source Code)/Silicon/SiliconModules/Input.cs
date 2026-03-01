using DocumentFormat.OpenXml.Office.PowerPoint.Y2022.M03.Main;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Silicon
{
    public partial class SiliconForm
    {
        // ─── KeyAction ────────────────────────────────────────────────────────────
        //
        // One instance per action. Holds everything: display name, current bound
        // key, and the delegates to fire. input.cs builds the list; hotkeyui.cs
        // reads/writes BoundKey for save/load and the settings panel.

        public class KeyAction
        {
            public string DisplayName { get; }
            public Keys DefaultKey { get; }
            public Keys BoundKey { get; set; }
            public Action OnPress { get; }
            public Action OnRelease { get; }

            public KeyAction(string displayName, Keys defaultKey, Action onPress, Action onRelease)
            {
                DisplayName = displayName;
                DefaultKey = defaultKey;
                BoundKey = defaultKey;
                OnPress = onPress;
                OnRelease = onRelease;
            }
        }
        // ─── Movement State ───────────────────────────────────────────────────────

        public class MovementState
        {
            public bool Forward, Backward, Left, Right;
            public bool Up, Down;
            public bool YawLeft, YawRight, PitchUp, PitchDown;
            public bool RollLeft, RollRight;
        }

        private MovementState movementState = new MovementState();

        // ─── Action Definitions ───────────────────────────────────────────────────
        //
        // To add a new keybind, add a single entry here. That's it.
        // Fields: DisplayName, DefaultKey, OnPress, OnRelease

        private List<KeyAction> BuildActionList() => new List<KeyAction>
        {
            new KeyAction("Camera Move Forward",    Keys.W,          () => movementState.Forward   = true,  () => movementState.Forward   = false),
            new KeyAction("Camera Move Backward",   Keys.S,          () => movementState.Backward  = true,  () => movementState.Backward  = false),
            new KeyAction("Camera Move Left",       Keys.A,          () => movementState.Left      = true,  () => movementState.Left      = false),
            new KeyAction("Camera Move Right",      Keys.D,          () => movementState.Right     = true,  () => movementState.Right     = false),
            new KeyAction("Camera Move Down",       Keys.ShiftKey,   () => movementState.Down      = true,  () => movementState.Down      = false),
            new KeyAction("Camera Move Up",         Keys.ControlKey, () => movementState.Up        = true,  () => movementState.Up        = false),
            new KeyAction("Camera Pitch Up",        Keys.Up,         () => movementState.PitchUp   = true,  () => movementState.PitchUp   = false),
            new KeyAction("Camera Pitch Down",      Keys.Down,       () => movementState.PitchDown = true,  () => movementState.PitchDown = false),
            new KeyAction("Camera Yaw Left",        Keys.Left,       () => movementState.YawLeft   = true,  () => movementState.YawLeft   = false),
            new KeyAction("Camera Yaw Right",       Keys.Right,      () => movementState.YawRight  = true,  () => movementState.YawRight  = false),
            new KeyAction("Camera Roll Left",       Keys.E,          () => movementState.RollLeft  = true,  () => movementState.RollLeft  = false),
            new KeyAction("Camera Roll Right",      Keys.Q,          () => movementState.RollRight = true,  () => movementState.RollRight = false),
            new KeyAction("Reset Camera Roll",      Keys.Z,          () => ResetCameraRoll(), NoOp),
            new KeyAction("Default Preset 1",       Keys.F1,         () => Preset1Button_Click(null, EventArgs.Empty), NoOp),
            new KeyAction("Default Preset 2",       Keys.F2,         () => Preset2Button_Click(null, EventArgs.Empty), NoOp),
            new KeyAction("Default Preset 3",       Keys.F3,         () => Preset3Button_Click(null, EventArgs.Empty), NoOp),
            new KeyAction("Default Preset 4",       Keys.F4,         () => Preset4Button_Click(null, EventArgs.Empty), NoOp),
            new KeyAction("Toggle Freecam",         Keys.F5,         () => FreecamSwitch.Switched          = !FreecamSwitch.Switched,           NoOp),
            new KeyAction("Toggle Hide Nametags",   Keys.F6,         () => HideNametagsSwitch.Switched     = !HideNametagsSwitch.Switched,      NoOp),
            new KeyAction("Toggle Hide UI",         Keys.F7,         () => HideUserInterfaceSwitch.Switched= !HideUserInterfaceSwitch.Switched, NoOp),
            new KeyAction("Add New Frame",          Keys.F8,         () => AddAnimationFrameButton_Click(null, EventArgs.Empty), NoOp),
            new KeyAction("Go to Previous Frame",   Keys.F9,         () => GoToPreviousFrame(), NoOp),
            new KeyAction("Go to Next Frame",       Keys.F10,        () => GoToNextFrame(), NoOp),
            new KeyAction("Play / Stop Animation",  Keys.F11,        () => PlayAnimationButton_Click(null, EventArgs.Empty), NoOp),
            new KeyAction("Record Animation",       Keys.F12,        () => RecordAnimationButton_Click(null, EventArgs.Empty), NoOp),

        };

        // ─── Runtime Binding State ────────────────────────────────────────────────

        private List<KeyAction> actions;                                         // ordered list; index matches saved JSON
        private Dictionary<Keys, KeyAction> keyMap;                              // fast lookup by bound key
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private static readonly Action NoOp = () => { };
        private static readonly Keys[] ALL_MONITORED_KEYS = Enum.GetValues(typeof(Keys)).Cast<Keys>().ToArray();

        private void InitializeKeyBindings()
        {
            actions = BuildActionList();
            RebuildKeyMap();
        }

        // Rebuilds the O(1) lookup dictionary from the current actions list.
        private void RebuildKeyMap()
        {
            keyMap = new Dictionary<Keys, KeyAction>();
            foreach (var action in actions)
            {
                if (action.BoundKey != Keys.None)
                    keyMap[action.BoundKey] = action;
            }
        }

        // ─── Mouse Drag ───────────────────────────────────────────────────────────

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
                if (!isRightDragging) return;

                int dx = pos.X - lastMousePos.X;
                int dy = pos.Y - lastMousePos.Y;

                float sensitivity = (float)CameraRotateSpeedSlider.Value / 1000;
                currentCameraYaw += dx * sensitivity;
                currentCameraPitch += dy * sensitivity;
                currentCameraPitch = Clamp(currentCameraPitch, -89f, 89f);
                targetCameraYaw = currentCameraYaw;
                targetCameraPitch = currentCameraPitch;
                lastMousePos = pos;
            };

            mouseHook.OnRightUp += (Point pos) => isRightDragging = false;
        }

        // ─── Mouse Scroll ─────────────────────────────────────────────────────────

        private void InitMouseScroll()
        {
            mouseHook.OnScrollDown += () =>
            {
                if (!IsCubicWindowFocused()) return;
                if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                    CameraDistanceSlider.Value = Math.Min(300, CameraDistanceSlider.Value + 1);
                else
                    CameraFOVSlider.Value = Math.Min(135, CameraFOVSlider.Value + 1);
            };

            mouseHook.OnScrollUp += () =>
            {
                if (!IsCubicWindowFocused()) return;
                if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                    CameraDistanceSlider.Value = Math.Max(2, CameraDistanceSlider.Value - 1);
                else
                    CameraFOVSlider.Value = Math.Max(10, CameraFOVSlider.Value - 1);
            };
        }

        // ─── Key Polling ──────────────────────────────────────────────────────────

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
            { IsBackground = true };
            keyPollingThread.Start();
        }

        private void UpdateKeyStates()
        {
            if (!IsCubicWindowFocused() && !IsSiliconWindowFocused())
                return;

            var currentPressed = new HashSet<Keys>();

            foreach (var key in ALL_MONITORED_KEYS)
            {
                if ((GetAsyncKeyState(key) & 0x8000) == 0) continue;
                currentPressed.Add(key);
                if (!pressedKeys.Contains(key))
                    HandleKeyDown(key);
            }

            foreach (var key in pressedKeys)
                if (!currentPressed.Contains(key))
                    HandleKeyUp(key);

            pressedKeys = currentPressed;

            if (pressedKeys.Count == 0)
                Thread.Sleep(10);
        }

        private void HandleKeyDown(Keys key)
        {
            if (isChatting) return;
            if (InvokeRequired) { Invoke(new Action(() => HandleKeyDown(key))); return; }
            if (keyMap.TryGetValue(key, out var action)) action.OnPress();
        }

        private void HandleKeyUp(Keys key)
        {
            if (IsDisposed || Disposing) return;
            if (isChatting) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(() => HandleKeyUp(key))); }
                catch (ObjectDisposedException) { }
                catch (InvalidOperationException) { }
                return;
            }
            if (keyMap.TryGetValue(key, out var action)) action.OnRelease();
        }
    }
}