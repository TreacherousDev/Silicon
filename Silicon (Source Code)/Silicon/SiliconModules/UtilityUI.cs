using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using Sunny.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;

namespace Silicon
{
    public partial class SiliconForm
    {
        private void CameraMoveSpeedSlider_Scroll(object sender)
        {
            cameraMoveSpeed = (double)CameraMoveSpeedSlider.Value / 500;
        }

        private void CameraRotateSpeedSlider_Scroll(object sender)
        {
            cameraRotateSpeed = (double)CameraRotateSpeedSlider.Value / 100;
        }

        private void Preset1Button_Click(object sender, EventArgs e)
        {
            CameraFOVSlider.Value = 33;
            CameraDistanceSlider.Value = 45;
            GameFogSlider.Value = 110;
            targetCameraFOV = 33;

            HidePlayerModelSwitch.Switched = false;
            HideUserInterfaceSwitch.Switched = false;
            HideNametagsSwitch.Switched = false;
            FreecamSwitch.Switched = false;

            ResetCameraRoll();
        }

        private void Preset2Button_Click(object sender, EventArgs e)
        {
            CameraFOVSlider.Value = 33;
            CameraDistanceSlider.Value = 90;
            GameFogSlider.Value = 110;
            targetCameraFOV = 33;

            HidePlayerModelSwitch.Switched = false;
            HideUserInterfaceSwitch.Switched = false;
            HideNametagsSwitch.Switched = false;
            FreecamSwitch.Switched = false;

            ResetCameraRoll();
        }

        private void Preset3Button_Click(object sender, EventArgs e)
        {
            CameraFOVSlider.Value = 70;
            CameraDistanceSlider.Value = 2;
            GameFogSlider.Value = 110;
            targetCameraFOV = 70;

            HidePlayerModelSwitch.Switched = true;
            HideUserInterfaceSwitch.Switched = false;
            HideNametagsSwitch.Switched = true;
            FreecamSwitch.Switched = false;

            ResetCameraRoll();
        }

        private void Preset4Button_Click(object sender, EventArgs e)
        {
            CameraFOVSlider.Value = 60;
            CameraDistanceSlider.Value = 220;
            GameFogSlider.Value = 200;
            targetCameraFOV = 45;

            HidePlayerModelSwitch.Switched = false;
            HideUserInterfaceSwitch.Switched = true;
            HideNametagsSwitch.Switched = true;
            FreecamSwitch.Switched = false;

            ResetCameraRoll();
        }

        private void UpdateUtilityUI()
        {
            if (FreecamSwitch.Switched != isFreecamEnabled)
            {
                isFreecamEnabled = FreecamSwitch.Switched;
                FreecamToggle(isFreecamEnabled);
            }

            if (HidePlayerModelSwitch.Switched != isHidePlayerModelEnabled)
            {
                isHidePlayerModelEnabled = HidePlayerModelSwitch.Switched;
                HidePlayerToggle(isHidePlayerModelEnabled);
            }

            if (HideUserInterfaceSwitch.Switched != isHideUserInterfaceEnabled)
            {
                isHideUserInterfaceEnabled = HideUserInterfaceSwitch.Switched;
                HideUserInterfaceToggle(isHideUserInterfaceEnabled);
            }


            if (HideNametagsSwitch.Switched != isHideNametagsEnabled)
            {
                isHideNametagsEnabled = HideNametagsSwitch.Switched;
                HideNametagsToggle(isHideNametagsEnabled);
            }

            if (CameraFOVSlider.Value != cameraFOVSliderValue)
            {
                cameraFOVSliderValue = CameraFOVSlider.Value;
                targetCameraFOV = CameraFOVSlider.Value;
            }

            if (CameraDistanceSlider.Value != cameraDistanceSliderValue)
            {
                cameraDistanceSliderValue = CameraDistanceSlider.Value;
                m.WriteMemory("Cubic.exe+30E9FA", "float", ((float)CameraDistanceSlider.Value / 2).ToString());
            }

            if (GameFogSlider.Value != gameFogSliderValue)
            {
                gameFogSliderValue = GameFogSlider.Value;
                targetCameraSightRange = GameFogSlider.Value;
            }
        }
    }
}