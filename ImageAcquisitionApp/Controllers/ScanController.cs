using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;

namespace ImageAcquisitionApp.Controllers;

public class ScanController
{
    private const string ImageBasePath = "..\\..\\Assets\\Images";

    private static bool IsValidPath(string filePath)
    {
        // Check if file exists
        return System.IO.File.Exists(filePath);
    }

    private static Mat GrayScaleConvert(Mat image)
    {
        // Convert the image to grayscale
        var result = new Mat();
        Cv2.CvtColor(image, result, ColorConversionCodes.BGR2GRAY);
        return result;
    }

    private static (Mat, double) Resize(Mat image, double? rescaleFactor = null)
    {
        const int hMax = 600;
        var h = image.Height;
        var w = image.Width;

        rescaleFactor ??= (double) hMax / h;

        var newW = (int) (w * rescaleFactor);
        var newH = (int) (h * rescaleFactor);

        var dimension = new Size(newW, newH);

        var resizedImage = new Mat();
        Cv2.Resize(image, resizedImage, dimension);

        var factor = 1 / rescaleFactor.Value;

        return (resizedImage, factor);
    }

    private static Mat RemoveNoise(Mat image)
    {
        var result = new Mat();
        Cv2.MedianBlur(image, result, 5);
        return result;
    }

    private static Mat EdgeDetection(Mat image)
    {
        // Apply Canny edge detection
        var result = new Mat();
        Cv2.Canny(image, result, 100, 200);
        return result;
    }

    public static Mat Thresholding(Mat image)
    {
        var result = new Mat();
        Cv2.Threshold(image, result, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
        return result;
    }

    public static Mat Dilate(Mat image)
    {
        var result = new Mat();
        var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
        Cv2.Dilate(image, result, kernel, new Point(-1, -1), 1, BorderTypes.Default, Scalar.All(0));
        return result;
    }

    public static Mat Erode(Mat image)
    {
        var result = new Mat();
        var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
        Cv2.Erode(image, result, kernel, new Point(-1, -1), 1, BorderTypes.Default, Scalar.All(0));
        return result;
    }

    public static Mat Opening(Mat image)
    {
        var result = new Mat();
        var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
        Cv2.MorphologyEx(image, result, MorphTypes.Open, kernel, new Point(-1, -1), 1, BorderTypes.Default,
            Scalar.All(0));
        return result;
    }


    public static IEnumerable<Point[]> Contours(Mat image)
    {
        Cv2.FindContours(image, out var contours, out var hierarchy, RetrievalModes.List,
            ContourApproximationModes.ApproxSimple);
        contours = contours.OrderByDescending(c => Cv2.ContourArea(c)).Take(5).ToArray();
        return contours;
    }


    public (Mat image, Point[] points) ScanOCR(string filePath)
    {
        try
        {
            if (!IsValidPath(filePath)) throw new Exception("Path is not valid");
            var image = Cv2.ImRead(filePath);

            var h = image.Height;
            var w = image.Width;

            var (imageResized, factor) = Resize(image);

            var gray = GrayScaleConvert(imageResized);
            var blur = RemoveNoise(gray);
            var edges = EdgeDetection(blur);
            var dilate = Dilate(blur);
            var contours = Contours(dilate);
            var pointsEnumerable = contours as Point[][] ?? contours.ToArray();
            if (pointsEnumerable.Any())
            {
                // Initialize source detected points
                Point[][] sourcePoints = null;
                foreach (var curve in pointsEnumerable)
                {
                    // Approximate the contour
                    var peri = Cv2.ArcLength(curve, true);
                    Point[] approx = Cv2.ApproxPolyDP(curve, 0.02 * peri, true);

                    // If contour has 4 sides (for documents)
                    if (approx.Length != 4) continue;
                    sourcePoints = new[] {approx};
                    break;
                }

                if (sourcePoints != null)
                {
                    // Draw contour in resized image and in original image
                    Cv2.DrawContours(imageResized, new[] {sourcePoints[0]}, -1, new Scalar(0, 255, 0), 5);
                    Point[] sourcePointsScaled = new Point[4];
                    for (int i = 0; i < 4; i++)
                    {
                        sourcePointsScaled[i] = new Point((int) Math.Round(sourcePoints[0][i].X * factor),
                            (int) Math.Round(sourcePoints[0][i].Y * factor));
                    }

                    Cv2.DrawContours(image, new[] {sourcePointsScaled}, -1, new Scalar(0, 255, 0), 5);
                    if (sourcePoints.Rank == 3 && sourcePoints.GetLength(1) == 1)
                    {
                        var points = sourcePoints.Cast<Point>().ToArray();
                        for (var i = 0; i < 4; i++)
                        {
                            points[i].X = (int) Math.Round(points[i].X * factor);
                            points[i].Y = (int) Math.Round(points[i].Y * factor);
                        }

                        return (image, points);
                    }
                }
            }


            return (new Mat(), new Point[] { });
        }
        catch (Exception ex)
        {
            // Handle the case where an exception is thrown (e.g., file not found)
            Console.WriteLine("Error: " + ex.Message);
        }

        return (null, new Point[] { });
    }
    
}