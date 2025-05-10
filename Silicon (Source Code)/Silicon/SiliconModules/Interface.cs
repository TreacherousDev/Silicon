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

            cameraFOVSliderValue = 22;
            CameraFOVSlider.Value = 22;
            m.WriteMemory("Cubic.exe+E20E1D", "float", cameraFOVSliderValue.ToString());

            cameraDistanceSliderValue = 33;
            CameraDistanceSlider.Value = 33;
            m.WriteMemory("Cubic.exe+E20FAC", "float", cameraDistanceSliderValue.ToString());

            gameFogSliderValue = 110;
            GameFogSlider.Value = 110;
            m.WriteMemory("Cubic.exe+2FFEC8", "float", GameFogSlider.Value.ToString());

            HidePlayerModelSwitch.Switched = false;
            HideUserInterfaceSwitch.Switched = false;
            HideNametagsSwitch.Switched = false;
            FreecamSwitch.Switched = false;
        }

        private void Preset2Button_Click(object sender, EventArgs e)
        {
            cameraFOVSliderValue = 45;
            CameraFOVSlider.Value = 45;
            m.WriteMemory("Cubic.exe+E20E1D", "float", cameraFOVSliderValue.ToString());

            cameraDistanceSliderValue = 33;
            CameraDistanceSlider.Value = 33;
            m.WriteMemory("Cubic.exe+E20FAC", "float", cameraDistanceSliderValue.ToString());

            gameFogSliderValue = 110;
            GameFogSlider.Value = 110;
            m.WriteMemory("Cubic.exe+2FFEC8", "float", GameFogSlider.Value.ToString());

            HidePlayerModelSwitch.Switched = false;
            HideUserInterfaceSwitch.Switched = false;
            HideNametagsSwitch.Switched = false;
            FreecamSwitch.Switched = false;
        }

        private void Preset3Button_Click(object sender, EventArgs e)
        {
            cameraFOVSliderValue = 70;
            CameraFOVSlider.Value = 70;
            m.WriteMemory("Cubic.exe+E20E1D", "float", cameraFOVSliderValue.ToString());

            cameraDistanceSliderValue = 1;
            CameraDistanceSlider.Value = 1;
            m.WriteMemory("Cubic.exe+E20FAC", "float", cameraDistanceSliderValue.ToString());

            gameFogSliderValue = 110;
            GameFogSlider.Value = 110;
            m.WriteMemory("Cubic.exe+2FFEC8", "float", GameFogSlider.Value.ToString());

            HidePlayerModelSwitch.Switched = true;
            HideUserInterfaceSwitch.Switched = false;
            HideNametagsSwitch.Switched = true;
            FreecamSwitch.Switched = false;
        }

        private double CatmullRom(double p0, double p1, double p2, double p3, double t)
        {
            return 0.5 * (
                2 * p1 +
                (-p0 + p2) * t +
                (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t +
                (-p0 + 3 * p1 - 3 * p2 + p3) * t * t * t
            );
        }

        // Play button state, alternates between play and stop when clicked
        private enum PlayButtonState { Play, Stop }
        private PlayButtonState playButtonState = PlayButtonState.Play;
        // Animation stopping mechanism for when stop button is pressed or freecam is disabled
        private CancellationTokenSource animationCancellationTokenSource;
        List<List<double>> animationFrames = new List<List<double>>();
        private async void PlayAnimationButton_Click(object sender, EventArgs e)
        {
            if (animationFrames.Count == 0)
            {
                MessageBox.Show("No keyframes found. Please add at least one keyframe before playing the animation.",
                                "Animation Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            if (playButtonState == PlayButtonState.Play)
            {
                playButtonState = PlayButtonState.Stop;
                PlayAnimationButton.Text = "◼";
                animationCancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = animationCancellationTokenSource.Token;

                // Play from selected frame (from first if there are multiple)
                int startIndex = 0;
                if (listViewFrames.SelectedItems.Count > 0)
                {
                    startIndex = listViewFrames.SelectedItems
                        .Cast<ListViewItem>()
                        .Min(item => item.Index);
                }

                // Teleport immediately to start frame
                List<double> firstFrame = animationFrames[startIndex];
                currentCameraLookAtX = firstFrame[0];
                currentCameraLookAtY = firstFrame[1];
                currentCameraLookAtZ = firstFrame[2];
                currentCameraPitch = firstFrame[3];
                currentCameraYaw = firstFrame[4];
                targetCameraLookAtX = firstFrame[0];
                targetCameraLookAtY = firstFrame[1];
                targetCameraLookAtZ = firstFrame[2];
                targetCameraPitch = firstFrame[3];
                targetCameraYaw = firstFrame[4];
                await Task.Delay(20);

                for (int i = startIndex; i < animationFrames.Count - 1; i++)
                {
                    if (token.IsCancellationRequested)
                        break;

                    List<double> startFrame = animationFrames[i];
                    List<double> endFrame = animationFrames[i + 1];

                    double startX = startFrame[0], startY = startFrame[1], startZ = startFrame[2];
                    double startPitch = startFrame[3], startYaw = startFrame[4];
                    double moveSpeed = endFrame[5];

                    double endX = endFrame[0], endY = endFrame[1], endZ = endFrame[2];
                    double endPitch = endFrame[3], endYaw = endFrame[4];
                    Interpolator.MethodDelegate frameInterpolation = _interpolator;

                    double distance = Math.Sqrt(
                        Math.Pow(endX - startX, 2) +
                        Math.Pow(endY - startY, 2) +
                        Math.Pow(endZ - startZ, 2));

                    double duration = distance / moveSpeed;
                    double startTime = Environment.TickCount;

                    while (true)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        double elapsedTime = (Environment.TickCount - startTime) / 1000.0;
                        double alpha = elapsedTime / duration;
                        alpha = Clamp(alpha, 0.0, 1.0);

                        if (InterpolationComboBox.Text == "Catmull-Rom")
                        {
                            int lastIndex = animationFrames.Count - 1;
                            List<double> p1 = animationFrames[i];
                            List<double> p2 = animationFrames[i + 1];
                            List<double> p0 = (i - 1) >= 0 ? animationFrames[i - 1] : p1;
                            List<double> p3 = (i + 2) <= lastIndex ? animationFrames[i + 2] : p2;

                            targetCameraLookAtX = CatmullRom(p0[0], p1[0], p2[0], p3[0], alpha);
                            targetCameraLookAtY = CatmullRom(p0[1], p1[1], p2[1], p3[1], alpha);
                            targetCameraLookAtZ = CatmullRom(p0[2], p1[2], p2[2], p3[2], alpha);
                            targetCameraPitch = CatmullRom(p0[3], p1[3], p2[3], p3[3], alpha);
                            targetCameraYaw = CatmullRom(p0[4], p1[4], p2[4], p3[4], alpha);
                        }
                        else
                        {
                            targetCameraLookAtX = frameInterpolation(startX, endX, alpha);
                            targetCameraLookAtY = frameInterpolation(startY, endY, alpha);
                            targetCameraLookAtZ = frameInterpolation(startZ, endZ, alpha);
                            targetCameraPitch = frameInterpolation(startPitch, endPitch, alpha);
                            targetCameraYaw = frameInterpolation(startYaw, endYaw, alpha);
                        }

                        if (alpha >= 1.0)
                            break;

                        await Task.Delay(5);
                    }
                }

                playButtonState = PlayButtonState.Play;
                PlayAnimationButton.Text = " ►";
                PlayAnimationButton.Refresh();
            }
            else if (playButtonState == PlayButtonState.Stop)
            {
                StopAnimation();
                playButtonState = PlayButtonState.Play;
                PlayAnimationButton.Text = " ►";
                PlayAnimationButton.Refresh();
            }
        }

        private void StopAnimation()
        {
            animationCancellationTokenSource?.Cancel();
            targetCameraLookAtX = currentCameraLookAtX;
            targetCameraLookAtY = currentCameraLookAtY;
            targetCameraLookAtZ = currentCameraLookAtZ;
            targetCameraPitch = currentCameraPitch;
            targetCameraYaw = currentCameraYaw;
        }


        private void AddAnimationFrameButton_Click(object sender, EventArgs e)
        {
            List<double> frame = new List<double>();
            frame.Add(currentCameraLookAtX);
            frame.Add(currentCameraLookAtY);
            frame.Add(currentCameraLookAtZ);
            frame.Add(currentCameraPitch);
            frame.Add(currentCameraYaw);
            double speed;
            if (!double.TryParse(CinematicSpeedTextBox.Text, out speed))
            {
                speed = 10.0; // default value if parsing fails
            }
            frame.Add(speed);
            frame.Add(InterpolationComboBox.SelectedIndex);

            animationFrames.Add(frame);
            UpdateListView();
        }
    

        private void UpdateListView()
        {
            // Update the ListView with the current animationFrames
            listViewFrames.Items.Clear();
            int i = 1;
            foreach (var frame in animationFrames)
            {
                ListViewItem item = new ListViewItem(i.ToString()); // LookAtX
                item.SubItems.Add(frame[0].ToString("F1"));                   // LookAtX
                item.SubItems.Add(frame[1].ToString("F1"));                   // LookAtY
                item.SubItems.Add(frame[2].ToString("F1"));                   // LookAtZ
                item.SubItems.Add(frame[3].ToString("F1"));                   // Pitch
                item.SubItems.Add(frame[4].ToString("F1"));                   // Yaw
                item.SubItems.Add(frame[5].ToString("F1"));                   // Speed
                //item.SubItems.Add(frame[6].ToString("F0"));                   // Interpolation
                listViewFrames.Items.Add(item);
                i++;
            }
        }

        private void ListViewFrames_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Start dragging the selected item
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void ListViewFrames_DragEnter(object sender, DragEventArgs e)
        {
            // Allow dragging into the ListView
            e.Effect = DragDropEffects.Move;
        }

        private void ListViewFrames_DragDrop(object sender, DragEventArgs e)
        {
            // Handle reordering of frames
            Point cp = listViewFrames.PointToClient(new Point(e.X, e.Y));
            ListViewItem dragToItem = listViewFrames.GetItemAt(cp.X, cp.Y);

            if (dragToItem != null)
            {
                int dragToIndex = dragToItem.Index;
                ListViewItem dragItem = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
                int dragFromIndex = dragItem.Index;

                // Reorder animationFrames
                var frame = animationFrames[dragFromIndex];
                animationFrames.RemoveAt(dragFromIndex);
                animationFrames.Insert(dragToIndex, frame);

                UpdateListView();
            }
        }

        private void DeleteAnimationFrameButton_Click(object sender, EventArgs e)
        {
            if (playButtonState == PlayButtonState.Stop)
            {
                MessageBox.Show("Cannot delete while animation is in progress", "Delete Frame", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (listViewFrames.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a frame to delete.", "Delete Frame", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (ListViewItem frame in listViewFrames.SelectedItems.Cast<ListViewItem>().ToList())
            {
                int selectedIndex = frame.Index;
                animationFrames.RemoveAt(selectedIndex);
                listViewFrames.Items.RemoveAt(selectedIndex);
            }

            UpdateListView();
        }


        private async void ActivateGoToFrame(int selectedIndex)
        {
            if (selectedIndex >= animationFrames.Count) return;

            FreecamSwitch.Switched = true;


            await Task.Delay(20);

            List<double> goToFrame = animationFrames[selectedIndex];

            // Set new target
            targetCameraLookAtX = goToFrame[0];
            targetCameraLookAtY = goToFrame[1];
            targetCameraLookAtZ = goToFrame[2];
            targetCameraPitch = goToFrame[3];
            targetCameraYaw = goToFrame[4];

            // Compute animation duration based on distance
            double distance = Math.Sqrt(
                Math.Pow(targetCameraLookAtX - currentCameraLookAtX, 2) +
                Math.Pow(targetCameraLookAtY - currentCameraLookAtY, 2) +
                Math.Pow(targetCameraLookAtZ - currentCameraLookAtZ, 2)
            );

            if (distance < equalityTolerance)
            {
                currentCameraLookAtX = targetCameraLookAtX;
                currentCameraLookAtY = targetCameraLookAtY;
                currentCameraLookAtZ = targetCameraLookAtZ;
            }

            double speed = double.TryParse(CinematicSpeedTextBox.Text, out var s) ? s : 10.0;
            animationDuration = distance / (speed / 100);
            animationStartTime = (Environment.TickCount / 100.0);
        }

        private void GoToAnnimationFrameButton_Click(object sender, EventArgs e)
        {
            if (listViewFrames.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a frame to view.", "Delete Frame", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (listViewFrames.SelectedItems.Count > 1)
            {

                MessageBox.Show("multiple frames selected. Please select only one.", "Delete Frame", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int selectedIndex = listViewFrames.SelectedItems[0].Index;
            ActivateGoToFrame(selectedIndex);
        }

        private void SaveAnimationButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                DefaultExt = "json",
                AddExtension = true,
                Title = "Save Animation Frames"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Serialize the animationFrames list to JSON
                    string json = System.Text.Json.JsonSerializer.Serialize(animationFrames);

                    // Write to the selected file
                    File.WriteAllText(saveFileDialog.FileName, json);

                    MessageBox.Show("Animation frames saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadAnimationButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Load Animation Frames"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Read the file content
                    string json = File.ReadAllText(openFileDialog.FileName);

                    // Deserialize JSON to List<List<double>>
                    animationFrames = System.Text.Json.JsonSerializer.Deserialize<List<List<double>>>(json);

                    // Refresh the ListView to reflect loaded data
                    listViewFrames.Items.Clear();
                    int i = 1;
                    foreach (var frame in animationFrames)
                    {
                        ListViewItem item = new ListViewItem(i.ToString());
                        item.SubItems.Add(frame[0].ToString("F1"));                   // LookAtX
                        item.SubItems.Add(frame[1].ToString("F1"));                   // LookAtY
                        item.SubItems.Add(frame[2].ToString("F1"));                   // LookAtZ
                        item.SubItems.Add(frame[3].ToString("F1"));                   // Pitch
                        item.SubItems.Add(frame[4].ToString("F1"));                   // Yaw
                        item.SubItems.Add(frame[5].ToString("F1"));                   // Speed
                        //item.SubItems.Add(frame[6].ToString("F0"));                   // Interpolation                                                              
                        listViewFrames.Items.Add(item);
                        i++;
                    }

                    MessageBox.Show("Animation frames loaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void InterpolationComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _interpolator = Interpolator.GetMethodWithIndex(InterpolationComboBox.SelectedIndex);
        }

        private void CinematicSpeedTextBox_TextChanged(object sender, EventArgs e)
        {
            var metroBox = sender as MetroSet_UI.Controls.MetroSetTextBox;
            var innerTextBox = metroBox.Controls[0] as TextBox;

            if (innerTextBox != null)
            {
                int caret = innerTextBox.SelectionStart;

                if (!Regex.IsMatch(metroBox.Text, @"^-?\d*\.?\d*$"))
                {
                    metroBox.Text = Regex.Replace(metroBox.Text, @"[^0-9.-]", "");
                    innerTextBox.SelectionStart = Math.Min(caret, metroBox.Text.Length);
                }
            }
        }

    }
}
