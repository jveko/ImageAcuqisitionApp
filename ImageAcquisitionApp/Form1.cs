using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace ImageAcquisitionApp
{
    public partial class Form1 : Form
    {
        private VideoCapture capture;
        private Stopwatch stopwatch;
        private Mat previousFrame;
        private int frameCounter = 0;
        private const int FRAME_THRESHOLD = 60;
        private const double MSE_THRESHOLD = 500.0;
        private const string bathPath = "..\\..\\";
        private const double DIFF_THRESHOLD = 1000000;
        private bool IsAlreadyCaptured = false;

        public Form1()
        {
            InitializeComponent();

            capture = new VideoCapture(1);
            capture.FrameWidth = 640;
            capture.FrameHeight = 480;

            timer1.Start();
            stopwatch = Stopwatch.StartNew();
            previousFrame = new Mat();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            using var frame = new Mat();
            var isReading = capture.Read(frame);
            if (isReading)
            {
                pictureBox1.Image = frame.ToBitmap();
            }

            cameraLabel.Text = isReading ? "Camera Open" : "Camera Closed";

            // Calculate FPS
            var fps = 1000.0 / stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            framePerSecondLabel.Text = $@"FPS: {fps:F2}";

            // Compare frames to detect changes
            if (!previousFrame.Empty())
            {
                // Calculate absolute difference between frames
                var diff = new Mat();
                Cv2.Absdiff(previousFrame, frame, diff);
                var diffSum = Cv2.Sum(diff).Val0;
                // Check if frames are similar
                if (diffSum <= DIFF_THRESHOLD)
                {
                    frameCounter++;
                    if (frameCounter >= FRAME_THRESHOLD)
                    {
                        if (IsAlreadyCaptured)
                        {
                            captureStatusLabel.Text = @$"Status Capture : Captured ({frameCounter})";
                        }
                        else
                        {
                            captureStatusLabel.Text = @$"Status Capture : Capturing ({frameCounter})";
                            IsAlreadyCaptured = true;
                            Capture();
                        }
                    }
                    else
                    {
                        captureStatusLabel.Text = @$"Status Capture : Active ({frameCounter})";
                        IsAlreadyCaptured = false;
                    }
                }
                else
                {
                    captureStatusLabel.Text = @$"Status Capture : Active ({frameCounter})";
                    frameCounter = 0;
                    IsAlreadyCaptured = false;
                }
            }

            // Save current frame as previous frame
            previousFrame = frame.Clone();
        }

        private void forceCaptureButton_Click(object sender, EventArgs e)
        {
            Capture();
        }

        private void Capture()
        {
            // Set camera focus to 0 (minimum value)
            capture.Focus = 0;

            // Wait for camera to adjust focus
            Thread.Sleep(1000);

            // Capture current frame
            using var frame = new Mat();
            var isReading = capture.Read(frame);
            if (!isReading)
            {
                MessageBox.Show("Failed to capture frame!");
                return;
            }

            // Generate filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var filename = $"frame_{timestamp}.jpg";

            // Save frame as image
            Cv2.ImWrite(Path.Combine(bathPath, @$"Images/{filename}"), frame);

            // Show message box with filename
            MessageBox.Show($"Frame saved as {filename}");
        }
    }
}