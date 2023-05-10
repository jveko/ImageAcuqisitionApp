using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageAcquisitionApp.Controllers;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace ImageAcquisitionApp.Views.WebCam;

public partial class WebCamForm : Form
{
    private const int FrameThreshold = 60;
    private const double DiffThreshold = 1000000;
    private const string ImageBasePath = "..\\..\\Assets\\Images";
    private readonly VideoCapture _capture;
    private readonly ScanController _scanController;
    private readonly Stopwatch _stopwatch;
    private int _frameCounter;
    private bool _isAlreadyCaptured;
    private Mat _previousFrame;

    public WebCamForm(ScanController scanController)
    {
        _scanController = scanController;
        InitializeComponent();

        _capture = new VideoCapture(2)
        {
            FrameWidth = 640,
            FrameHeight = 480
        };

        timer1.Start();
        _stopwatch = Stopwatch.StartNew();
        _previousFrame = new Mat();
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        using var frame = new Mat();
        var isReading = _capture.Read(frame);
        if (isReading) pictureBox1.Image = frame.ToBitmap();

        cameraLabel.Text = isReading ? "Camera Open" : "Camera Closed";

        // Calculate FPS
        var fps = 1000.0 / _stopwatch.ElapsedMilliseconds;
        _stopwatch.Restart();
        framePerSecondLabel.Text = $@"FPS: {fps:F2}";

        // Compare frames to detect changes
        CompareFrame(frame);

        // Save current frame as previous frame
        _previousFrame = frame.Clone();
    }

    private async void forceCaptureButton_Click(object sender, EventArgs e)
    {
        var frame = await CaptureFrame();
        ScanDocument(frame);
    }

    private void ScanDocument(Mat frame)
    {
        try
        {
            var (image, corners) = _scanController.ScanImage(frame);
            var newCorners = _scanController.ArrangePts(corners, image.Size());
            var ratio = _scanController.ScannedRatio(newCorners);
            var (dst, dim) = _scanController.SetDestinationPts(ratio, image.Size());
            var warp = _scanController.WarpImg(image, newCorners, dst, image.Size());
            var (warped, asd) = _scanController.IsImage(Path.Combine(ImageBasePath, "example.jpeg"));
            var (warped2, asds) = _scanController.IsImage2(Path.Combine(ImageBasePath, "example.jpeg"));
            _scanController.SaveImage(warp, "final.jpg");
            _scanController.SaveImage(warped, "final1.jpg");
            _scanController.SaveImage(warped2, "final2.jpg");
        }
        catch (Exception e)
        {
            MessageBox.Show($"Error: {e.Message}");
            Console.WriteLine(e);
        }
    }

    private async void CompareFrame(Mat frame)
    {
        if (_previousFrame.Empty()) return;
        // Calculate absolute difference between frames
        var diff = new Mat();
        Cv2.Absdiff(_previousFrame, frame, diff);
        var diffSum = Cv2.Sum(diff).Val0;
        // Check if frames are similar
        if (diffSum <= DiffThreshold)
        {
            _frameCounter++;
            if (_frameCounter >= FrameThreshold)
            {
                if (_isAlreadyCaptured)
                {
                    captureStatusLabel.Text = @$"Status Capture : Captured ({_frameCounter})";
                }
                else
                {
                    captureStatusLabel.Text = @$"Status Capture : Capturing ({_frameCounter})";
                    _isAlreadyCaptured = true;
                    ScanDocument(await CaptureFrame());
                }
            }
            else
            {
                captureStatusLabel.Text = @$"Status Capture : Active ({_frameCounter})";
                _isAlreadyCaptured = false;
            }
        }
        else
        {
            captureStatusLabel.Text = @$"Status Capture : Active ({_frameCounter})";
            _frameCounter = 0;
            _isAlreadyCaptured = false;
        }
    }

    private async Task<Mat> CaptureFrame()
    {
        // Set camera focus to 0 (minimum value)
        _capture.Focus = 0;

        // Wait for camera to adjust focus
        Thread.Sleep(1000);

        // Capture current frame
        var frame = new Mat();
        var isReading = _capture.Read(frame);
        if (!isReading)
        {
            MessageBox.Show("Failed to capture frame!");
            return null;
        }

        // Generate filename with timestamp
        // var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        // var filename = $"frame_{timestamp}.jpg";

        // Save frame as image
        // Cv2.ImWrite(Path.Combine(ImageBasePath, filename), frame);

        // Show message box with filename
        // MessageBox.Show($"Frame saved as {filename} {AppDomain.CurrentDomain.BaseDirectory}");
        return frame;
    }
}