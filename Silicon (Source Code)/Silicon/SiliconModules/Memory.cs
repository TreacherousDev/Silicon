using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
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

        readonly string cameraHeightInjection = "53 E8 00 00 00 00 5B F3 0F 5C 43 1B F3 0F 11 40 08 5B F3 0F 5C CB 8D 85 FC FE FF FF E9 89 F0 EA FF 66 66 A6 3F";
        readonly string cameraHeightFunctionEntry = "E9 5B 0F 15 00 0F 1F 44 00 00";

        readonly string unlockCameraArrowsInjection = "50 E8 00 00 00 00 58 F3 0F 10 58 1F 0F 2F D8 58 0F 86 C2 B3 EA FF C7 86 EC 09 00 00 00 00 B2 C2 E9 B3 B3 EA FF 00 00 B2 C2 00 00 00 00";
        readonly string unlockCameraArrowsFunctionEntry = "E9 2F 4C 15 00";

        readonly string unlockCameraRMBInjection = "50 E8 00 00 00 00 58 F3 0F 10 70 1F 0F 2F F1 58 0F 86 6C CA EB FF C7 80 EC 09 00 00 00 00 B2 C2 E9 5D CA EB FF 00 00 B2 C2 00 00 00 00";
        readonly string unlockCameraRMBFunctionEntry = "E9 85 35 14 00";

        readonly string unlockCameraFOVInjection = "50 E8 00 00 00 00 58 F3 0F 10 40 12 58 8D 85 D8 FE FF FF E9 EE EF EA FF 00 00 96 42 FF FF FF FF";
        readonly string unlockCameraFOVFunctionEntry = "E9 FB 0F 15 00 90";

        readonly string adjustCameraDistanceInjection = "50 E8 00 00 00 00 58 F3 0F 59 40 24 F3 0F 59 58 24 F3 0F 59 60 24 58 E9 00 00 00 00 F3 0F 5C D0 F3 0F 10 40 08 E9 25 EE EA FF 00 00 C8 41 FF FF FF FF";
        readonly string adjustCameraDistanceFunctionEntry = "E9 B5 11 15 00 0F 1F 40 00";

        readonly string upVectorInjection = "55 8B EC 8B 55 08 39 D1 0F 84 B4 92 D6 FF 50 E8 00 00 00 00 58 53 8B 58 2B 89 1A 8B 58 2F 89 5A 04 8B 58 33 89 5A 08 5B 58 8B 02 89 01 8B 42 04 89 41 04 8B 42 08 89 41 08 8B C1 5D C2 04 00 00 00 00 00 00 00 00 00 00 00 80 BF FF FF FF FF";
        readonly string upVectorFunctionEntry = "E8 63 12 15 00";

        readonly string overrideArrowHotkeysFunction = "90 90 90 90 90 90 90 90";
        readonly string overrideRightClickDragFunction = "90 90 90 90 90 90 90 90 90 90 90 90 90 90 90 90";


        // Revertable functions (Optional switch states available)
        readonly string cameraLookAtEditorInjection = "50 E8 00 00 00 00 58 F3 0F 11 58 5D F3 0F 11 48 61 F3 0F 11 40 65 F3 0F 10 58 4D F3 0F 10 48 51 F3 0F 10 40 55 58 50 E8 00 00 00 00 58 F3 0F 11 58 37 F3 0F 11 48 3B F3 0F 11 40 3F 53 8D 5E 10 89 58 33 5B 58 F3 0F 11 1E F3 0F 11 4E 04 E9 6F B6 EA FF 00 00 00 00 00 00 00 00 00 00 8C 42 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF FF FF FF";
        readonly string cameraLookAtEditorFunctionEntry = "E9 42 49 15 00 0F 1F 40 00";
        readonly string cameraLookAtEditorFunctionOriginal = "E9 68 49 15 00 0F 1F 40 00";

        readonly string hidePlayerAvatarInjection = "53 E8 00 00 00 00 5B F3 0F 10 7D 08 F3 0F 5C BB 65 01 00 00 50 F3 0F 11 7B 6B 8B 43 6B 23 43 63 66 0F 6E F8 58 0F 2E 7B 67 0F 83 2F 00 00 00 F3 0F 10 7D 0C F3 0F 5C BB 69 01 00 00 50 F3 0F 11 7B 6B 8B 43 6B 23 43 63 66 0F 6E F8 58 0F 2E 7B 67 0F 83 07 00 00 00 C7 45 10 00 00 C8 C2 5B F3 0F 10 45 10 E9 70 20 E9 FF FF FF FF 7F 9A 99 99 3E 00 00 00 00 AA AA AA AA";
        readonly string hidePlayerAvatarFunctionEntry = "E9 27 DF 16 00";
        readonly string hidePlayerAvatarFunctionOriginal = "F3 0F 10 45 10";

        readonly string disableMovement1Function = "90 90 90 90";
        readonly string disableMovement1Original = "F3 0F 11 02";
        readonly string disableMovement2Function = "90 90 90 90 90 90 90 90";
        readonly string disableMovement2Original = "F3 0F 11 87 80 00 00 00";


        //
        //
        // Required permissions: VirtualMemoryOperation and VirtualMemoryWrite
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);


        // Constants for memory allocation
        const uint MEM_COMMIT = 0x00001000;
        const uint MEM_RESERVE = 0x00002000;
        const uint PAGE_EXECUTE_READWRITE = 0x40; // Giving full permissions for this example
        const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
        int hexAddressAllocated = 0;
        private IntPtr allocatedMemoryPtr = IntPtr.Zero;

        public bool UnlockCubicMemoryRegion()
        {
            try
            {
                // 1. Get the process handle from the connected PID
                using (Process targetProcess = Process.GetProcessById(connectedPID))
                {
                    IntPtr hProcess = targetProcess.Handle;
                    IntPtr baseAddr = targetProcess.MainModule.BaseAddress;

                    // 2. Set target to Cubic.exe + 0x1000
                    IntPtr targetAddress = new IntPtr(baseAddr.ToInt64() + 0x30E77F);
                    UIntPtr size = new UIntPtr(1024); // 1KB

                    uint oldProtect;

                    // 3. Apply Read/Write/Execute (0x40)
                    if (VirtualProtectEx(hProcess, targetAddress, size, PAGE_EXECUTE_READWRITE, out oldProtect))
                    {
                        Console.WriteLine($"Successfully unlocked 0x{targetAddress.ToString("X")}");
                        Console.WriteLine($"Permissions changed from 0x{oldProtect:X} to 0x40 (RWX)");
                        return true;
                    }
                    else
                    {
                        int error = Marshal.GetLastWin32Error();
                        Console.WriteLine($"Failed to unlock memory. Error code: {error}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UnlockCubicMemoryRegion: {ex.Message}");
                return false;
            }
        }


        // Base injection code caves and function jumps to allow Silicon to modify game behaviour
        private void InjectBaseFunctions()
        {
            UnlockCubicMemoryRegion();

            m.WriteMemory("Cubic.exe+1BD80A", "bytes", cameraCoordinatesFunction);
            m.WriteMemory("Cubic.exe+1BD921", "bytes", adjustFogFunction);

            // Special case for cameraLookAtEditor
            // Always inject, but skip assignment if deactivated (inject to the adress after assignment)
            m.WriteMemory("Cubic.exe+30EA2D", "bytes", cameraLookAtEditorInjection);
            m.WriteMemory("Cubic.exe+1BA0E6", "bytes", cameraLookAtEditorFunctionOriginal);

            m.WriteMemory("Cubic.exe+30E77F", "bytes", cameraHeightInjection);
            m.WriteMemory("Cubic.exe+1BD81F", "bytes", cameraHeightFunctionEntry);
            m.WriteMemory("Cubic.exe+30E7C8", "bytes", unlockCameraArrowsInjection);
            m.WriteMemory("Cubic.exe+1B9B94", "bytes", unlockCameraArrowsFunctionEntry);
            m.WriteMemory("Cubic.exe+30E816", "bytes", unlockCameraRMBInjection);
            m.WriteMemory("Cubic.exe+1CB28C", "bytes", unlockCameraRMBFunctionEntry);
            m.WriteMemory("Cubic.exe+30E853", "bytes", unlockCameraFOVInjection);
            m.WriteMemory("Cubic.exe+1BD853", "bytes", unlockCameraFOVFunctionEntry);
            m.WriteMemory("Cubic.exe+30E9D0", "bytes", adjustCameraDistanceInjection);
            m.WriteMemory("Cubic.exe+1BD816", "bytes", adjustCameraDistanceFunctionEntry);
            m.WriteMemory("Cubic.exe+30EAE5", "bytes", upVectorInjection);
            m.WriteMemory("Cubic.exe+1BD87D", "bytes", upVectorFunctionEntry);

            m.WriteMemory("Cubic.exe+1B9ADE", "bytes", overrideArrowHotkeysFunction);
            m.WriteMemory("Cubic.exe+1B9B1A", "bytes", overrideArrowHotkeysFunction);
            m.WriteMemory("Cubic.exe+1CB22A", "bytes", overrideRightClickDragFunction);


            //Revertable, injections only as set to false by default
            m.WriteMemory("Cubic.exe+30E925", "bytes", hidePlayerAvatarInjection);
        }

        private void UpdateMemory()
        {
            HandleCameraController(currentCameraYaw);

            uint intRotationAddress = m.ReadUInt("Cubic.exe+30EA8C");
            string pitchAddress = (intRotationAddress + 4).ToString("X");
            string yawAddress = (intRotationAddress).ToString("X");

            isChatting = m.ReadInt("Cubic.exe+30EB8B") != 0;

            // Up Vector -> Roll
            m.WriteMemory("Cubic.exe+30EB24", "float", upVector.X.ToString());
            m.WriteMemory("Cubic.exe+30EB28", "float", upVector.Y.ToString());
            m.WriteMemory("Cubic.exe+30EB2C", "float", upVector.Z.ToString());
            // Pitch, Yaw
            m.WriteMemory(pitchAddress, "bytes", ConvertDoubleToFloatBytes(currentCameraPitch));
            m.WriteMemory(yawAddress, "bytes", ConvertDoubleToFloatBytes(currentCameraYaw));
            

            // FOV
            m.WriteMemory("Cubic.exe+30E86B", "bytes", ConvertDoubleToFloatBytes(currentCameraFOV));
            //Game Fog
            m.WriteMemory("Cubic.exe+300ED0", "bytes", ConvertDoubleToFloatBytes(currentCameraSightRange));

            if (FreecamSwitch.Switched == false)
            {
                // Look at Player
                targetCameraLookAtX = m.ReadFloat("Cubic.exe+30EA90");
                targetCameraLookAtY = m.ReadFloat("Cubic.exe+30EA94");
                targetCameraLookAtZ = m.ReadFloat("Cubic.exe+30EA98");
            }
            else
            {
                // Overwrite camera position using Silicon as the new controller
                // Look at Silicon Controlled Position
                m.WriteMemory("Cubic.exe+30EA80", "bytes", ConvertDoubleToFloatBytes(currentCameraLookAtX));
                m.WriteMemory("Cubic.exe+30EA84", "bytes", ConvertDoubleToFloatBytes(currentCameraLookAtY));
                m.WriteMemory("Cubic.exe+30EA88", "bytes", ConvertDoubleToFloatBytes(currentCameraLookAtZ));
            }

            string ConvertDoubleToFloatBytes(double num)
            {
                // Workaround for unexpected behaviour with the API WriteMemory "float".
                float floatValue = (float)num;
                byte[] byteArray = BitConverter.GetBytes(floatValue);
                string byteString = BitConverter.ToString(byteArray).Replace("-", " ");

                return byteString;
            }

            UpdateLabel(CameraLookAtInfoLabel, $"X: {currentCameraLookAtX:F2}\nY: {currentCameraLookAtY:F2}\nZ: {currentCameraLookAtZ:F2}", Color.White);
            UpdateLabel(CameraLookAtInfoLabel2, $"X: {currentCameraLookAtX:F2}\nY: {currentCameraLookAtY:F2}\nZ: {currentCameraLookAtZ:F2}", Color.White);
            UpdateLabel(CameraRotationInfoLabel, $"Pitch: {currentCameraPitch:F2}\nYaw: {currentCameraYaw:F2}\nRoll: {currentCameraRoll:F2}", Color.White);
            UpdateLabel(CameraRotationInfoLabel2, $"Pitch: {currentCameraPitch:F2}\nYaw: {currentCameraYaw:F2}\nRoll: {currentCameraRoll:F2}", Color.White);
            UpdateLabel(CameraZoomInfoLabel, $"Zoom: {(float)cameraDistanceSliderValue / 2}\nFOV: {currentCameraFOV:F1}", Color.White);
            UpdateLabel(CameraZoomInfoLabel2, $"Zoom: {(float)cameraDistanceSliderValue / 2}\nFOV: {currentCameraFOV:F1}", Color.White);
        }

        private void FreecamToggle(bool enabled)
        {
            if (enabled)
            {
                // Start Freecam camera position and rotation at the current camera position and rotation
                m.WriteMemory("Cubic.exe+1BA0E6", "bytes", cameraLookAtEditorFunctionEntry);
                m.WriteMemory("Cubic.exe+21BE4B", "bytes", disableMovement1Function);
                m.WriteMemory("Cubic.exe+21BE7A", "bytes", disableMovement1Function);
                m.WriteMemory("Cubic.exe+21BDE1", "bytes", disableMovement2Function);
                m.WriteMemory("Cubic.exe+21BE18", "bytes", disableMovement2Function);
            }
            else
            {
                m.WriteMemory("Cubic.exe+1BA0E6", "bytes", cameraLookAtEditorFunctionOriginal);
                m.WriteMemory("Cubic.exe+21BE4B", "bytes", disableMovement1Original);
                m.WriteMemory("Cubic.exe+21BE7A", "bytes", disableMovement1Original);
                m.WriteMemory("Cubic.exe+21BDE1", "bytes", disableMovement2Original);
                m.WriteMemory("Cubic.exe+21BE18", "bytes", disableMovement2Original);
                StopAnimation();
            }

        }
        private void HidePlayerToggle(bool enabled)
        {
            if (enabled)
            {
                m.WriteMemory("Cubic.exe+1A09F9", "bytes", hidePlayerAvatarFunctionEntry);
            }
            else
            {
                m.WriteMemory("Cubic.exe+1A09F9", "bytes", hidePlayerAvatarFunctionOriginal);
            }
        }

        private void HideUserInterfaceToggle(bool enabled)
        {
            if (enabled)
            {
                m.WriteMemory("Cubic.exe+1BFB65", "bytes", "84");
                m.WriteMemory("Cubic.exe+231C0F", "bytes", "C7 82 38 02 00 00 00 40 1C C6 90 90 90 90 90 90");
            }
            else
            {
                m.WriteMemory("Cubic.exe+1BFB65", "bytes", "85");
                m.WriteMemory("Cubic.exe+231C0F", "bytes", "F3 0F 11 82 38 02 00 00 F3 0F 58 8A 3C 02 00 00");
            }
        }

        private void HideNametagsToggle(bool enabled)
        {
            if (enabled)
            {
                m.WriteMemory("Cubic.exe+1A8ADD", "bytes", "E9 09 0F 00 00 90");
            }
            else
            {
                m.WriteMemory("Cubic.exe+1A8ADD", "bytes", "0F 85 08 0F 00 00");
            }   
        }

    }

}