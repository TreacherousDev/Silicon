using System;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;

namespace Silicon
{
    public partial class SiliconForm
    {
        private System.Timers.Timer _updateTimer;

        // Persistent Functions (Required for the engine to function)
        readonly string cameraCoordinatesFunction = "90 90 90 90 90 90 90 90 90 90 90 90";
        readonly string adjustFogFunction = "90 90 90 90 90 90 90 90";

        readonly string cameraHeightInjection = "53 E8 00 00 00 00 5B F3 0F 5C 43 1B F3 0F 11 40 08 5B F3 0F 5C CB 8D 85 FC FE FF FF E9 CB BA 39 FF 66 66 A6 3F 00 00 00 00";
        readonly string cameraHeightFunctionEntry = "E9 19 45 C6 00 0F 1F 44 00 00";

        readonly string unlockCameraArrowsInjection = "50 E8 00 00 00 00 58 F3 0F 10 58 1F 0F 2F D8 58 0F 86 04 7E 39 FF C7 86 EC 09 00 00 00 00 B2 C2 E9 F5 7D 39 FF 00 00 B2 C2 00 00 00";
        readonly string unlockCameraArrowsFunctionEntry = "E9 ED 81 C6 00";

        readonly string unlockCameraRMBInjection = "50 E8 00 00 00 00 58 F3 0F 10 70 1F 0F 2F F1 58 0F 86 AA 94 3A FF C7 80 EC 09 00 00 00 00 B2 C2 E9 9B 94 3A FF 00 00 B2 C2 00 00 00";
        readonly string unlockCameraRMBFunctionEntry = "E9 47 6B C5 00";

        readonly string unlockCameraFOVInjection = "50 E8 00 00 00 00 58 53 8B 58 1A 89 9F E8 07 00 00 5B 58 F3 0F 10 87 E8 07 00 00 E9 22 BA 39 FF 00 00 04 42 FF FF FF FF";
        readonly string unlockCameraFOVFunctionEntry = "E9 C1 45 C6 00 0F 1F";

        readonly string adjustCameraDistanceInjection = "50 E8 00 00 00 00 58 F3 0F 59 40 24 F3 0F 59 58 24 F3 0F 59 60 24 58 E9 00 00 00 00 F3 0F 5C D0 F3 0F 10 40 08 E9 67 B8 39 FF 00 00 C8 41 FF FF FF FF";
        readonly string adjustCameraDistanceFunctionEntry = "E9 73 47 C6 00 0F 1F 40 00";

        readonly string upVectorInjection = "55 8B EC 8B 55 08 39 D1 0F 84 02 6D 25 FF 50 E8 00 00 00 00 58 53 8B 58 2B 89 1A 8B 58 2F 89 5A 04 8B 58 33 89 5A 08 5B 58 8B 02 89 01 8B 42 04 89 41 04 8B 42 08 89 41 08 8B C1 5D C2 04 00 00 00 00 00 00 00 00 00 00 00 80 BF FF FF FF FF";
        readonly string upVectorFunctionEntry = "E8 21 48 C6 00";

        readonly string overrideArrowHotkeysFunction = "90 90 90 90 90 90 90 90";

        // Revertable functions (Optional switch states available)
        readonly string cameraLookAtEditorInjection = "50 E8 00 00 00 00 58 F3 0F 11 58 5D F3 0F 11 48 61 F3 0F 11 40 65 F3 0F 10 58 4D F3 0F 10 48 51 F3 0F 10 40 55 58 50 E8 00 00 00 00 58 F3 0F 11 58 37 F3 0F 11 48 3B F3 0F 11 40 3F 53 8D 5E 10 89 58 33 5B 58 F3 0F 11 1E F3 0F 11 4E 04 E9 B1 80 39 FF 00 00 00 00 00 00 00 00 00 00 8C 42 40 D8 7D 10 00 00 00 00 00 00 00 00 00 00 8C 42 FF FF FF FF";
        readonly string cameraLookAtEditorFunctionEntry = "E9 00 7F C6 00 0F 1F 40 00";
        readonly string cameraLookAtEditorFunctionOriginal = "E9 26 7F C6 00 0F 1F 40 00";

        readonly string hidePlayerAvatarInjection = "53 E8 00 00 00 00 5B F3 0F 10 7D 08 F3 0F 5C BB 65 01 00 00 50 F3 0F 11 7B 6B 8B 43 6B 23 43 63 66 0F 6E F8 58 0F 2E 7B 67 0F 83 2F 00 00 00 F3 0F 10 7D 0C F3 0F 5C BB 69 01 00 00 50 F3 0F 11 7B 6B 8B 43 6B 23 43 63 66 0F 6E F8 58 0F 2E 7B 67 0F 83 07 00 00 00 C7 45 10 00 00 C8 C2 5B F3 0F 10 45 10 E9 18 EB 37 FF FF FF FF 7F 9A 99 99 3E 8C 10 C1 40 AA AA AA AA";
        readonly string hidePlayerAvatarFunctionEntry = "E9 7F 14 C8 00";
        readonly string hidePlayerAvatarFunctionOriginal = "F3 0F 10 45 10";

        readonly string disableMovement1Function = "90 90 90 90";
        readonly string disableMovement1Original = "F3 0F 11 02";
        readonly string disableMovement2Function = "90 90 90 90 90 90 90 90";
        readonly string disableMovement2Original = "F3 0F 11 87 80 00 00 00";


        // Base injection code caves and function jumps to allow Silicon to modify game behaviour
        private void InjectBaseFunctions()
        {
            m.WriteMemory("Cubic.exe+1BC7FE", "bytes", cameraCoordinatesFunction);
            m.WriteMemory("Cubic.exe+1BC915", "bytes", adjustFogFunction);

            // Special case for cameraLookAtEditor
            // Always inject, but skip assignment if deactivated (inject to the adress after assignment)
            m.WriteMemory("Cubic.exe+E20FDF", "bytes", cameraLookAtEditorInjection);
            m.WriteMemory("Cubic.exe+1B90DA", "bytes", cameraLookAtEditorFunctionOriginal);

            m.WriteMemory("Cubic.exe+E20D31", "bytes", cameraHeightInjection);
            m.WriteMemory("Cubic.exe+1BC813", "bytes", cameraHeightFunctionEntry);
            m.WriteMemory("Cubic.exe+E20D7A", "bytes", unlockCameraArrowsInjection);
            m.WriteMemory("Cubic.exe+1B8B88", "bytes", unlockCameraArrowsFunctionEntry);
            m.WriteMemory("Cubic.exe+E20DC8", "bytes", unlockCameraRMBInjection);
            m.WriteMemory("Cubic.exe+1CA27C", "bytes", unlockCameraRMBFunctionEntry);
            m.WriteMemory("Cubic.exe+E20E05", "bytes", unlockCameraFOVInjection);
            m.WriteMemory("Cubic.exe+1BC83F", "bytes", unlockCameraFOVFunctionEntry);
            m.WriteMemory("Cubic.exe+E20F82", "bytes", adjustCameraDistanceInjection);
            m.WriteMemory("Cubic.exe+1BC80A", "bytes", adjustCameraDistanceFunctionEntry);
            m.WriteMemory("Cubic.exe+E21097", "bytes", upVectorInjection);
            m.WriteMemory("Cubic.exe+1BC871", "bytes", upVectorFunctionEntry);

            m.WriteMemory("Cubic.exe+1B8AD2", "bytes", overrideArrowHotkeysFunction);
            m.WriteMemory("Cubic.exe+1B8B0E", "bytes", overrideArrowHotkeysFunction);

            //Revertable, injections only as set to false by default
            m.WriteMemory("Cubic.exe+E20ED7", "bytes", hidePlayerAvatarInjection);
        }

        private void UpdateMemoryOnTimerTick(object sender, ElapsedEventArgs e)
        {
            CheckAndUpdateMemory();
        }

        private void CheckAndUpdateMemory()
        {

            //if (isFreecamEnabled)
            //{
                HandleCameraController(currentCameraYaw);
            //}
            UpdateCameraRoll();

            uint intRotationAddress = m.ReadUInt("Cubic.exe+E2103E");
            string pitchAddress = (intRotationAddress + 4).ToString("X");
            string yawAddress = (intRotationAddress).ToString("X");
            string lookAtXAddress = (intRotationAddress - 16).ToString("X");
            string lookAtYAddress = (intRotationAddress - 12).ToString("X");
            string lookAtZAddress = (intRotationAddress - 8).ToString("X");

            InterpolateCameraMovement(lookAtXAddress, lookAtYAddress, lookAtZAddress);
            InterpolateCameraRotation(pitchAddress, yawAddress);
            InterpolateCameraFOV("Cubic.exe+E20E25");


            if (FreecamSwitch.Switched != isFreecamEnabled)
            {
                isFreecamEnabled = FreecamSwitch.Switched;
                if (isFreecamEnabled)
                {
                    // Start Freecam camera position and rotation at the current camera position and rotation
                    m.WriteMemory("Cubic.exe+1B90DA", "bytes", cameraLookAtEditorFunctionEntry);

                    m.WriteMemory("Cubic.exe+21AE4B", "bytes", disableMovement1Function);
                    m.WriteMemory("Cubic.exe+21AE7A", "bytes", disableMovement1Function);
                    m.WriteMemory("Cubic.exe+21ADE1", "bytes", disableMovement2Function);
                    m.WriteMemory("Cubic.exe+21AE18", "bytes", disableMovement2Function);

                }
                else
                {
                    m.WriteMemory("Cubic.exe+1B90DA", "bytes", cameraLookAtEditorFunctionOriginal);
                    m.WriteMemory("Cubic.exe+21AE4B", "bytes", disableMovement1Original);
                    m.WriteMemory("Cubic.exe+21AE7A", "bytes", disableMovement1Original);
                    m.WriteMemory("Cubic.exe+21ADE1", "bytes", disableMovement2Original);
                    m.WriteMemory("Cubic.exe+21AE18", "bytes", disableMovement2Original);
                    StopAnimation();
                }
            }

            if (HidePlayerModelSwitch.Switched != isHidePlayerModelEnabled)
            {
                isHidePlayerModelEnabled = HidePlayerModelSwitch.Switched;
                if (isHidePlayerModelEnabled == true)
                {
                    m.WriteMemory("Cubic.exe+19FA53", "bytes", hidePlayerAvatarFunctionEntry);
                }
                else
                {
                    m.WriteMemory("Cubic.exe+19FA53", "bytes", hidePlayerAvatarFunctionOriginal);
                }
            }


            if (HideUserInterfaceSwitch.Switched != isHideUserInterfaceEnabled)
            {
                isHideUserInterfaceEnabled = HideUserInterfaceSwitch.Switched;
                if (isHideUserInterfaceEnabled == true)
                {
                    m.WriteMemory("Cubic.exe+1BEB57", "bytes", "84");
                    m.WriteMemory("Cubic.exe+230C0F", "bytes", "C7 82 38 02 00 00 00 40 1C C6 90 90 90 90 90 90");
                }
                else
                {
                    m.WriteMemory("Cubic.exe+1BEB57", "bytes", "85");
                    m.WriteMemory("Cubic.exe+230C0F", "bytes", "F3 0F 11 82 38 02 00 00 F3 0F 58 8A 3C 02 00 00");
                }
            }


            if (HideNametagsSwitch.Switched != isHideNametagsEnabled)
            {
                isHideNametagsEnabled = HideNametagsSwitch.Switched;
                if (isHideNametagsEnabled == true)
                {
                    m.WriteMemory("Cubic.exe+1A7B37", "bytes", "E9 09 0F 00 00 90");
                }
                else
                {
                    m.WriteMemory("Cubic.exe+1A7B37", "bytes", "0F 85 08 0F 00 00");
                }
            }


            if (CameraFOVSlider.Value != cameraFOVSliderValue)
            {
                cameraFOVSliderValue = CameraFOVSlider.Value;
                targetCameraFOV = CameraFOVSlider.Value;
                //m.WriteMemory("Cubic.exe+E20E25", "float", CameraFOVSlider.Value.ToString());
            }

            if (CameraDistanceSlider.Value != cameraDistanceSliderValue)
            {
                cameraDistanceSliderValue = CameraDistanceSlider.Value;
                m.WriteMemory("Cubic.exe+E20FAC", "float", CameraDistanceSlider.Value.ToString());
            }

            if (GameFogSlider.Value != gameFogSliderValue)
            {
                gameFogSliderValue = GameFogSlider.Value;
                m.WriteMemory("Cubic.exe+2FFEC8", "float", GameFogSlider.Value.ToString());
            }

            // Code updates camera labels regardless of freecam toggle
            //if (FreecamSwitch.Switched == true)
            if (pressedKeys.Contains(Keys.Up) || pressedKeys.Contains(Keys.Down) || pressedKeys.Contains(Keys.Left)
                || pressedKeys.Contains(Keys.Right) || IsRightMouseButtonDown())
            {
                // Overwrite camera position, pitch and yaw using Silicon as the new controller
                // Handle vanilla screen orbiting
                if (IsRightMouseButtonDown())
                {
                    targetCameraPitch = m.ReadFloat(pitchAddress);
                    targetCameraYaw = m.ReadFloat(yawAddress);
                }
                else
                {
                    m.WriteMemory(pitchAddress, "bytes", ConvertDoubleToFloatBytes(currentCameraPitch));
                    m.WriteMemory(yawAddress, "bytes", ConvertDoubleToFloatBytes(currentCameraYaw));
                }
                m.WriteMemory("Cubic.exe+E21032", "bytes", ConvertDoubleToFloatBytes(currentCameraLookAtX));
                m.WriteMemory("Cubic.exe+E21036", "bytes", ConvertDoubleToFloatBytes(currentCameraLookAtY));
                m.WriteMemory("Cubic.exe+E2103A", "bytes", ConvertDoubleToFloatBytes(currentCameraLookAtZ)); 
            }
            else
            {
                targetCameraLookAtX = m.ReadFloat("Cubic.exe+E21042");
                targetCameraLookAtY = m.ReadFloat("Cubic.exe+E21046");
                targetCameraLookAtZ = m.ReadFloat("Cubic.exe+E2104A");
                targetCameraPitch = m.ReadFloat(pitchAddress);
                targetCameraYaw = m.ReadFloat(yawAddress);
            }
            // FOV editing enabled even with freecam disabled
            m.WriteMemory("Cubic.exe+E20E25", "bytes", ConvertDoubleToFloatBytes(currentCameraFOV));
            // Up vector override for camera roll
            m.WriteMemory("Cubic.exe+E210D6", "float", upVector.X.ToString());
            m.WriteMemory("Cubic.exe+E210DA", "float", upVector.Y.ToString());
            m.WriteMemory("Cubic.exe+E210DE", "float", upVector.Z.ToString());
            //Console.WriteLine($"Up: {upVector.X:F2}, {upVector.Y:F2}, {upVector.Z:F2}");


            string ConvertDoubleToFloatBytes(double num)
            {
                // Workaround for unexpected behaviour with the API WriteMemory "float".
                float floatValue = (float)num;
                byte[] byteArray = BitConverter.GetBytes(floatValue);
                string byteString = BitConverter.ToString(byteArray).Replace("-", " ");

                return byteString;
            }

            //UpdateLabel(CameraPositionDataLabel, $"X: {currentCameraLookAtX:F2} Y: {currentCameraLookAtY:F2} Z: {currentCameraLookAtZ:F2} Pitch: {currentCameraPitch:F2} Yaw: {currentCameraYaw:F2}", Color.Red);
            UpdateLabel(CameraLookAtInfoLabel, $"X: {currentCameraLookAtX:F2}\nY: {currentCameraLookAtY:F2}\nZ: {currentCameraLookAtZ:F2}", Color.White);
            UpdateLabel(CameraLookAtInfoLabel2, $"X: {currentCameraLookAtX:F2}\nY: {currentCameraLookAtY:F2}\nZ: {currentCameraLookAtZ:F2}", Color.White);
            UpdateLabel(CameraRotationInfoLabel, $"Pitch: {currentCameraPitch:F2}\nYaw: {currentCameraYaw:F2}\n🔎:  {cameraDistanceSliderValue} | {cameraFOVSliderValue}", Color.White);
            UpdateLabel(CameraRotationInfoLabel2, $"Pitch: {currentCameraPitch:F2}\nYaw: {currentCameraYaw:F2}\n🔎:  {cameraDistanceSliderValue} | {cameraFOVSliderValue}", Color.White);

        }
    }
}