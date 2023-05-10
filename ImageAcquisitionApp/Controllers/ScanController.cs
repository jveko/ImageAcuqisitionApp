using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using OpenCvSharp;

namespace ImageAcquisitionApp.Controllers;

public class ScanController
{
    private const string ImageBasePath = "..\\..\\Assets\\Images";

    public static Point2f[] ConvertPoints(Point[] points)
    {
        return points.Select(p => new Point2f(p.X, p.Y)).ToArray();
    }

    public static bool SaveFile(Mat frame, string fileName)
    {
        return Cv2.ImWrite(Path.Combine(ImageBasePath, fileName), frame);
    }

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
        SaveFile(result, "grayscale.jpg");
        return result;
    }

    private static (Mat, double) Resize(Mat image, double? rescaleFactor = null)
    {
        const int hMax = 600;
        var h = image.Height;
        var w = image.Width;

        rescaleFactor ??= (double) hMax / h;

        var newW = (int) (w * rescaleFactor.Value);
        var newH = (int) (h * rescaleFactor.Value);

        var dimension = new Size(newW, newH);

        var resizedImage = new Mat();
        Cv2.Resize(image, resizedImage, dimension);

        var factor = 1 / rescaleFactor.Value;
        SaveFile(resizedImage, "resized.jpg");

        return (resizedImage, factor);
    }

    private static Mat RemoveNoise(Mat image)
    {
        var result = new Mat();
        Cv2.MedianBlur(image, result, 5);
        SaveFile(result, "RemoveNoise.jpg");
        return result;
    }

    private static Mat EdgeDetection(Mat image)
    {
        // Apply Canny edge detection
        var result = new Mat();
        Cv2.Canny(image, result, 50, 200);
        SaveFile(result, "EdgeDetection.jpg");
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
        Cv2.Dilate(image, result, new Mat(2, 2, MatType.CV_8U, Scalar.All(1)), iterations: 1);
        SaveFile(result, "Dilate.jpg");
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


    public static (Point[][], HierarchyIndex[]) Contours(Mat image)
    {
        Cv2.FindContours(image, out var contours, out var hierarchy, RetrievalModes.List,
            ContourApproximationModes.ApproxSimple);
        return (contours.OrderByDescending(c => Cv2.ContourArea(c)).Take(5).ToArray(), hierarchy);
    }

    public (Mat image, Point2f[] points) ScanImage(string filePath)
    {
        try
        {
            if (!IsValidPath(filePath)) throw new Exception("Path is not valid");
            var frame = Cv2.ImRead(filePath);
            return ScanImage(frame);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public (Mat image, Point2f[] points) ScanImage(Mat image)
    {
        try
        {
            var (imageResized, factor) = Resize(image);
            var gray = GrayScaleConvert(imageResized);
            var blur = RemoveNoise(gray);
            var edges = EdgeDetection(blur);
            var dilate = Dilate(edges);
            var (contours, hierarchyIndices) = Contours(dilate);
            if (!contours.Any())
            {
                throw new NotSupportedException("Couldn't find any object in the image.");
            }

            // Initialize source detected points
            Point2f[] sourcePoints = null;
            foreach (var curve in contours)
            {
                // Approximate the contour
                var peri = Cv2.ArcLength(curve, true);
                var approx = Cv2.ApproxPolyDP(curve, 0.02 * peri, true);

                // If contour has 4 sides (for documents)
                if (approx.Length != 4) continue;
                sourcePoints = approx.Select(p => new Point2f(p.X, p.Y)).ToArray();
                ;
                break;
            }

            if (sourcePoints == null)
            {
                throw new EvaluateException("The Object is not 4 sides");
            }

            Cv2.DrawContours(imageResized,
                new List<Point[]>() {sourcePoints.Select(p => new Point((int) p.X, (int) p.Y)).ToArray()}, -1,
                Scalar.FromRgb(0, 255, 0), 5);
            Cv2.DrawContours(image,
                new List<Point[]>()
                    {sourcePoints.Select(p => new Point((int) (p.X * factor), (int) (p.Y * factor))).ToArray()}, -1,
                Scalar.FromRgb(0, 255, 0), 5);
            for (var i = 0; i < sourcePoints.Length; i++)
            {
                sourcePoints[i] *= factor;
            }

            return (image, sourcePoints);
        }
        catch (Exception ex)
        {
            // Handle the case where an exception is thrown (e.g., file not found)
            Console.WriteLine("Error: " + ex.Message);
            throw;
        }
    }

    public (Mat, Point2f[]) IsImage(string filePath)
    {
        // Read image
        Mat img = Cv2.ImRead(filePath, ImreadModes.Color);

        // Convert to grayscale
        Mat gray = new Mat();
        Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
        SaveFile(gray, "1 - gray.jpg");

        // Apply Gaussian blur
        Mat blur = new Mat();
        Cv2.GaussianBlur(gray, blur, new Size(5, 5), 0);
        SaveFile(blur, "2 - blur.jpg");

        // Apply adaptive thresholding
        Mat thresh = new Mat();
        Cv2.AdaptiveThreshold(blur, thresh, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 2);
        SaveFile(thresh, "3 - thresh.jpg");

        // Find contours
        Point[][] contours;
        HierarchyIndex[] hierarchy;
        Cv2.FindContours(thresh, out contours, out hierarchy, RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        // Find the largest contour (document's boundary)
        var largestContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).FirstOrDefault();

        // Find the quadrilateral representing the document
        double peri = Cv2.ArcLength(largestContour, true);
        Point[] approx = Cv2.ApproxPolyDP(largestContour, 0.02 * peri, true);

        // Ensure the quadrilateral has four points
        if (approx.Length != 4)
        {
            return (null, null);
        }

        Point2f[] srcPts = approx.Select(p => new Point2f(p.X, p.Y)).ToArray();

        // Apply perspective transformation
        Point2f[] dstPts = new Point2f[]
        {
            new Point2f(0, 0),
            new Point2f(0, img.Rows),
            new Point2f(img.Cols, img.Rows),
            new Point2f(img.Cols, 0)
        };

        Mat M = Cv2.GetPerspectiveTransform(srcPts, dstPts);
        Mat warped = new Mat();
        Cv2.WarpPerspective(img, warped, M, new Size(img.Cols, img.Rows));

        return (warped, srcPts);
    }

    public  (Mat, Point2f[]) IsImage2(string filePath)
    {
        // Read image
        Mat img = Cv2.ImRead(filePath, ImreadModes.Color);

        // Denoise the image
        Mat denoised = new Mat();
        Cv2.FastNlMeansDenoisingColored(img, denoised, 10, 10, 7, 21);

        // Convert to grayscale
        Mat gray = new Mat();
        Cv2.CvtColor(denoised, gray, ColorConversionCodes.BGR2GRAY);

        // Apply Gaussian blur
        Mat blur = new Mat();
        Cv2.GaussianBlur(gray, blur, new Size(5, 5), 0);

        // Apply adaptive thresholding
        Mat thresh = new Mat();
        Cv2.AdaptiveThreshold(blur, thresh, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 2);

        // Apply morphological operations (e.g., closing)
        Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
        Mat closed = new Mat();
        Cv2.MorphologyEx(thresh, closed, MorphTypes.Close, kernel);

        // Find contours
        Point[][] contours;
        HierarchyIndex[] hierarchy;
        Cv2.FindContours(closed, out contours, out hierarchy, RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        // Find the largest contour (document's boundary)
        var largestContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).FirstOrDefault();

        // Find the quadrilateral representing the document
        double peri = Cv2.ArcLength(largestContour, true);
        Point[] approx = Cv2.ApproxPolyDP(largestContour, 0.02 * peri, true);

        // Ensure the quadrilateral has four points
        if (approx.Length != 4)
        {
            return (null, null);
        }

        Point2f[] srcPts = approx.Select(p => new Point2f(p.X, p.Y)).ToArray();

        // Apply perspective transformation
        Point2f[] dstPts = new Point2f[]
        {
            new Point2f(0, 0),
            new Point2f(0, img.Rows),
            new Point2f(img.Cols, img.Rows),
            new Point2f(img.Cols, 0)
        };

        Mat M = Cv2.GetPerspectiveTransform(srcPts, dstPts);
        Mat warped = new Mat();
        Cv2.WarpPerspective(img, warped, M, new Size(img.Cols, img.Rows));

        return (warped, srcPts);
    }

    public static Mat ConvertPointToMat(Point point)
    {
        var mat = new Mat(1, 1, MatType.CV_32FC2);
        mat.Set(0, 0, point.X);
        mat.Set(0, 1, point.Y);
        return mat;
    }

    public static Mat ConvertPointToMat(Point2f point)
    {
        var mat = new Mat(1, 1, MatType.CV_32FC2);
        mat.Set(0, 0, point.X);
        mat.Set(0, 1, point.Y);
        return mat;
    }

    public static float[] CalculateDistance(Point2f[] points1, Point2f[] points2)
    {
        var numPoints = points1.Length;
        var distance = new float[numPoints];
        for (var i = 0; i < numPoints; i++)
        {
            var point1 = points1[i];
            var point2 = points2[i];
            var diffX = point1.X - point2.X;
            var diffY = point1.Y - point2.Y;
            distance[i] = (float) Math.Sqrt(diffX * diffX + diffY * diffY);
        }

        return distance;
    }

    public Point2f[] ArrangePts(Point2f[] pts, Size dim)
    {
        var w = dim.Width;
        var h = dim.Height;
        Point2f[] imgPts = {new(0, 0), new(0, h), new(w, h), new(w, 0)};

        var sortedIdx = new List<int>();

        foreach (var pos in imgPts)
        {
            var minIndex = -1;
            var minDist = double.MaxValue;

            for (var i = 0; i < pts.Length; i++)
            {
                double distX = pts[i].X - pos.X;
                double distY = pts[i].Y - pos.Y;
                var dist = Math.Sqrt(distX * distX + distY * distY);

                if (dist < minDist)
                {
                    minDist = dist;
                    minIndex = i;
                }
            }

            sortedIdx.Add(minIndex);
        }

        var arrangedPts = sortedIdx.Select(index => pts[index]).ToArray();
        return arrangedPts;
    }

    public double ScannedRatio(Point2f[] pts)
    {
        // Norm of height and width of found corners
        double wDiffX = pts[2].X - pts[1].X;
        double wDiffY = pts[2].Y - pts[1].Y;
        var wNorm = Math.Sqrt(wDiffX * wDiffX + wDiffY * wDiffY);

        double hDiffX = pts[2].X - pts[3].X;
        double hDiffY = pts[2].Y - pts[3].Y;
        var hNorm = Math.Sqrt(hDiffX * hDiffX + hDiffY * hDiffY);

        // Ratio between two norms
        var ratio = hNorm / wNorm;
        return ratio;
    }


    public (Point2f[], Size) SetDestinationPts(double ratio, Size dim)
    {
        var w = dim.Width;
        var h = dim.Height;

        if (ratio >= 1)
        {
            w = (int) (h / ratio);
        }
        else
        {
            h = (int) (w * ratio);
        }

        var dst = new Point2f[]
        {
            new(0, 0),
            new(0, h),
            new(w, h),
            new(w, 0)
        };

        return (dst, new Size(w, h));
    }


    public Mat WarpImg(Mat img, Point2f[] corners, Point2f[] pts, Size dim)
    {
        var w = dim.Width;
        var h = dim.Height;

        // Matrix transform
        var M = Cv2.GetPerspectiveTransform(corners, pts);

        // Warped image from actual size
        var warp = new Mat();
        Cv2.WarpPerspective(img, warp, M, new Size(w, h));

        return warp;
    }

    public bool SaveImage(Mat frame, string fileName)
    {
        return Cv2.ImWrite(Path.Combine(ImageBasePath, fileName), frame);
    }
}