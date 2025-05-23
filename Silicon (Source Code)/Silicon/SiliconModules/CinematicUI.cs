﻿using System;
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
        // Play button state, alternates between play and stop when clicked
        private enum PlayButtonState {Play, Stop}
        private PlayButtonState playButtonState = PlayButtonState.Play;
        // Animation stopping mechanism for when stop button is pressed or freecam is disabled
        private CancellationTokenSource animationCancellationTokenSource;
        List<List<double>> animationFrames = new List<List<double>>();

        // Saved JSON Format
        public class AnimationData
        {
            public int CameraDistance { get; set; }
            public List<List<double>> Frames { get; set; }
        }

        private void AddAnimationFrameButton_Click(object sender, EventArgs e)
        {
            List<double> frame = new List<double>();
            frame.Add(currentCameraLookAtX);
            frame.Add(currentCameraLookAtY);
            frame.Add(currentCameraLookAtZ);
            frame.Add(currentCameraPitch);
            frame.Add(currentCameraYaw);
            frame.Add(currentCameraRoll);
            frame.Add(currentCameraFOV);
            frame.Add(currentCameraSightRange);
            frame.Add(double.TryParse(CinematicSpeedTextBox.Text, out double speed) ? speed : 10.0);


            // Insert frame after selected row
            int insertIndex;
            if (listViewFrames.SelectedItems.Count == 0)
            {
                // No selection, append to the end
                insertIndex = animationFrames.Count;
            }
            else
            {
                // Insert after the bottommost row if multiple are selected
                insertIndex = listViewFrames.SelectedItems.Cast<ListViewItem>().Max(item => item.Index) + 1;
            }

            animationFrames.Insert(insertIndex, frame);
            UpdateListView();

            // Auto-select the newly added frame
            listViewFrames.SelectedItems.Clear();
            if (insertIndex < listViewFrames.Items.Count)
            {
                listViewFrames.Items[insertIndex].Selected = true;
                listViewFrames.Items[insertIndex].Focused = true;
                listViewFrames.EnsureVisible(insertIndex);
            }
        }

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

            if (playButtonState == PlayButtonState.Stop)
            {
                StopAnimation();
                playButtonState = PlayButtonState.Play;
                PlayAnimationButton.Text = " ►";
                PlayAnimationButton.Refresh();
                return;
            }

            FreecamSwitch.Switched = true;
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

            //CameraDistanceSlider.Value = cinematicLoadedCameraDistance;
            // Teleport immediately to start frame
            List<double> firstFrame = animationFrames[startIndex];
            currentCameraLookAtX = firstFrame[0];
            currentCameraLookAtY = firstFrame[1];
            currentCameraLookAtZ = firstFrame[2];
            currentCameraPitch = firstFrame[3];
            currentCameraYaw = firstFrame[4];
            currentCameraRoll = firstFrame[5];
            currentCameraFOV = firstFrame[6];
            currentCameraSightRange = firstFrame[7];

            targetCameraLookAtX = firstFrame[0];
            targetCameraLookAtY = firstFrame[1];
            targetCameraLookAtZ = firstFrame[2];
            targetCameraPitch = firstFrame[3];
            targetCameraYaw = firstFrame[4];
            targetCameraRoll = firstFrame[5];
            targetCameraFOV = firstFrame[6];
            targetCameraSightRange = firstFrame[7];

            //
            startCameraLookAtX = firstFrame[0];
            startCameraLookAtY = firstFrame[1];
            startCameraLookAtZ = firstFrame[2];
            startCameraPitch = firstFrame[3];
            startCameraYaw = firstFrame[4];
            startCameraRoll = firstFrame[5];
            startCameraFOV = firstFrame[6];
            startCameraSightRange = firstFrame[7];

            // Update FOV and Sight Range Sliders
            CameraFOVSlider.Value = (int)firstFrame[6];
            cameraFOVSliderValue = (int)firstFrame[6];
            GameFogSlider.Value = (int)firstFrame[7];
            gameFogSliderValue = (int)firstFrame[7];

            await Task.Delay(20);

            for (int i = startIndex; i < animationFrames.Count - 1; i++)
            {
                if (token.IsCancellationRequested)
                    break;

                    

                List<double> startFrame = animationFrames[i];
                List<double> endFrame = animationFrames[i + 1];

                //
                startCameraLookAtX = startFrame[0];
                startCameraLookAtY = startFrame[1];
                startCameraLookAtZ = startFrame[2];
                startCameraPitch = startFrame[3];
                startCameraYaw = startFrame[4];
                startCameraRoll = startFrame[5];
                startCameraFOV = startFrame[6];
                startCameraSightRange = startFrame[7];

                double startX = startFrame[0], startY = startFrame[1], startZ = startFrame[2];
                double startPitch = startFrame[3], startYaw = startFrame[4], startRoll = startFrame[5];
                double startFOV = startFrame[6], startSightRange = startFrame[7];
                double endX = endFrame[0], endY = endFrame[1], endZ = endFrame[2];
                double endPitch = endFrame[3], endYaw = endFrame[4], endRoll = endFrame[5];
                double endFOV = endFrame[6], endSightRange = endFrame[7];
                double moveSpeed = endFrame[8];
                
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

                    double elapsedTime = (Environment.TickCount - startTime) / 10000.0;
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
                        targetCameraRoll = CatmullRom(p0[5], p1[5], p2[5], p3[5], alpha);
                        targetCameraFOV = CatmullRom(p0[6], p1[6], p2[6], p3[6], alpha);
                        targetCameraSightRange = CatmullRom(p0[7], p1[7], p2[7], p3[7], alpha);
                    }
                    else
                    {
                        targetCameraLookAtX = frameInterpolation(startX, endX, alpha);
                        targetCameraLookAtY = frameInterpolation(startY, endY, alpha);
                        targetCameraLookAtZ = frameInterpolation(startZ, endZ, alpha);
                        targetCameraPitch = frameInterpolation(startPitch, endPitch, alpha);
                        targetCameraYaw = frameInterpolation(startYaw, endYaw, alpha);
                        targetCameraRoll = frameInterpolation(startRoll, endRoll, alpha);
                        targetCameraFOV = frameInterpolation(startFOV, endFOV, alpha);
                        targetCameraSightRange = frameInterpolation(startSightRange, endSightRange, alpha);
                    }
                    upVector = ComputeUpVectorFromRoll((float)currentCameraRoll);

                    // Update FOV and Sight Range sliders after each keyframe pass
                    CameraFOVSlider.Value = (int)endFrame[6];
                    cameraFOVSliderValue = (int)endFrame[6];
                    GameFogSlider.Value = (int)endFrame[7];
                    gameFogSliderValue = (int)endFrame[7];

                    if (alpha >= 1.0)
                        break;

                    await Task.Delay(5);
                }
            }

            playButtonState = PlayButtonState.Play;
            PlayAnimationButton.Text = " ►";
            PlayAnimationButton.Refresh();
            
            
        }

        // Interpolation Helper Method
        private double CatmullRom(double p0, double p1, double p2, double p3, double t)
        {
            return 0.5 * (
                2 * p1 +
                (-p0 + p2) * t +
                (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t +
                (-p0 + 3 * p1 - 3 * p2 + p3) * t * t * t
            );
        }
        private void StopAnimation()
        {
            animationCancellationTokenSource?.Cancel();
            targetCameraLookAtX = currentCameraLookAtX;
            targetCameraLookAtY = currentCameraLookAtY;
            targetCameraLookAtZ = currentCameraLookAtZ;
            targetCameraPitch = currentCameraPitch;
            targetCameraYaw = currentCameraYaw;
            targetCameraFOV = currentCameraFOV;
        }

        private void UpdateListView()
        {
            // Update the ListView with the current animationFrames
            listViewFrames.Items.Clear();
            int i = 1;
            foreach (var frame in animationFrames)
            {
                ListViewItem item = new ListViewItem(i.ToString()); // LookAtX
                item.SubItems.Add(frame[0].ToString("F1")); // LookAtX
                item.SubItems.Add(frame[1].ToString("F1")); // LookAtY
                item.SubItems.Add(frame[2].ToString("F1")); // LookAtZ
                item.SubItems.Add(frame[3].ToString("F1")); // Pitch
                item.SubItems.Add(frame[4].ToString("F1")); // Yaw
                item.SubItems.Add(frame[5].ToString("F1")); // Roll
                item.SubItems.Add(frame[6].ToString("F0")); // FOV
                item.SubItems.Add(frame[7].ToString("F0")); // Sight Range
                item.SubItems.Add(frame[8].ToString("F1")); // Speed
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
            // Cancel if too many selected
            // Show warning if trying to go to frame while animation is playing
            if (selectedIndex >= animationFrames.Count) return;
            if (playButtonState == PlayButtonState.Stop)
            {
                MessageBox.Show("Cannot go to frame while animation is in progress", "Go To Frame", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
 
            FreecamSwitch.Switched = true;
            await Task.Delay(20);

            List<double> goToFrame = animationFrames[selectedIndex];
            // Set new target
            targetCameraLookAtX = goToFrame[0];
            targetCameraLookAtY = goToFrame[1];
            targetCameraLookAtZ = goToFrame[2];
            targetCameraPitch = goToFrame[3];
            targetCameraYaw = goToFrame[4];
            targetCameraRoll = goToFrame[5];
            targetCameraFOV = goToFrame[6];
            targetCameraSightRange = goToFrame[7];

            startCameraLookAtX = currentCameraLookAtX;
            startCameraLookAtY = currentCameraLookAtY;
            startCameraLookAtZ = currentCameraLookAtZ;
            startCameraPitch = currentCameraPitch;
            startCameraYaw = currentCameraYaw;
            startCameraRoll = currentCameraRoll;
            startCameraFOV = currentCameraFOV;
            startCameraSightRange = currentCameraSightRange;

            // Update FOV and Sight Range sliders
            CameraFOVSlider.Value = (int)goToFrame[6];
            cameraFOVSliderValue = (int)goToFrame[6];
            GameFogSlider.Value = (int)goToFrame[7];
            gameFogSliderValue = (int)goToFrame[7];

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
            animationDuration = distance / (speed);
            animationStartTime = (Environment.TickCount / 10000.0);
        }

        private void GoToNextFrame()
        {
            if (listViewFrames.Items.Count == 0)return;

            int targetIndex;

            if (listViewFrames.SelectedItems.Count == 0)
            {
                targetIndex = 0; // Go to first frame
            }
            else
            {
                int maxIndex = listViewFrames.SelectedItems.Cast<ListViewItem>().Max(item => item.Index);
                targetIndex = (maxIndex + 1) % listViewFrames.Items.Count; // wrap around to 0
            }

            SelectAndGoToFrame(targetIndex);
        }

        private void GoToPreviousFrame()
        {
            if (listViewFrames.Items.Count == 0) return;

            int targetIndex;
            if (listViewFrames.SelectedItems.Count == 0)
            {
                targetIndex = listViewFrames.Items.Count - 1; // Go to last frame
            }
            else
            {
                int minIndex = listViewFrames.SelectedItems.Cast<ListViewItem>().Min(item => item.Index);
                targetIndex = (minIndex - 1 + listViewFrames.Items.Count) % listViewFrames.Items.Count; // wrap around to last
            }

            SelectAndGoToFrame(targetIndex);
        }

        private void SelectAndGoToFrame(int index)
        {
            listViewFrames.SelectedItems.Clear();
            listViewFrames.Items[index].Selected = true;
            listViewFrames.Items[index].Focused = true;
            listViewFrames.EnsureVisible(index);
            ActivateGoToFrame(index);
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
                    // Prepare data to save: frames + metadata
                    var dataToSave = new
                    {
                        CameraDistance = CameraDistanceSlider.Value,
                        Frames = animationFrames
                    };

                    // Serialize with indentation
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    };

                    string json = System.Text.Json.JsonSerializer.Serialize(dataToSave, options);

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
                    string json = File.ReadAllText(openFileDialog.FileName);
                    List<List<double>> frames = null;

                    // Try to deserialize as new format
                    try
                    {
                        var data = System.Text.Json.JsonSerializer.Deserialize<AnimationData>(json);
                        if (data?.Frames != null)
                        {
                            frames = data.Frames;
                            CameraDistanceSlider.Value = data.CameraDistance;
                        }
                    }
                    catch // Ignore and try old format
                    {  }

                    // If new format failed, try old format
                    if (frames == null)
                    {
                        frames = System.Text.Json.JsonSerializer.Deserialize<List<List<double>>>(json);
                        if (frames == null) throw new Exception("Invalid or empty JSON format.");
                    }

                    animationFrames.Clear();
                    listViewFrames.Items.Clear();

                    int i = 1;
                    foreach (var rawFrame in frames)
                    {
                        List<double> frame = new List<double>
                        {
                            rawFrame.Count > 0 ? rawFrame[0] : 50.0,
                            rawFrame.Count > 1 ? rawFrame[1] : 50.0,
                            rawFrame.Count > 2 ? rawFrame[2] : 50.0,
                            rawFrame.Count > 3 ? rawFrame[3] : 0.0,
                            rawFrame.Count > 4 ? rawFrame[4] : 0.0,
                            rawFrame.Count > 5 ? rawFrame[5] : 0.0,
                            rawFrame.Count > 6 ? rawFrame[6] : 70.0,
                            rawFrame.Count > 7 ? rawFrame[7] : 100.0,
                            rawFrame.Count > 7 ? rawFrame[8] : 60.0
                        };

                        animationFrames.Add(frame);

                        ListViewItem item = new ListViewItem(i.ToString());
                        item.SubItems.Add(frame[0].ToString("F1"));
                        item.SubItems.Add(frame[1].ToString("F1"));
                        item.SubItems.Add(frame[2].ToString("F1"));
                        item.SubItems.Add(frame[3].ToString("F1"));
                        item.SubItems.Add(frame[4].ToString("F1"));
                        item.SubItems.Add(frame[5].ToString("F1"));
                        item.SubItems.Add(frame[6].ToString("F0"));
                        item.SubItems.Add(frame[7].ToString("F0"));
                        item.SubItems.Add(frame[8].ToString("F1"));
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