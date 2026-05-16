using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MeasureSpeedVector.Models;
using MeasureSpeedVertor.Utilities;


namespace MeasureSpeedVector
{
    partial class Program
    {
        // =========================
        // Main
        // =========================

        static void Main(string[] args)
        {


            string imageFolder = @"..\..\..\Images";

            var detections = ProcessFrames(imageFolder);

            Common.PrintSection("Before Interpolation");
            Common.PrintDetections(detections);

            InterpolateMissingFrames(detections);

            Common.PrintSection("After Interpolation");
            Common.PrintDetections(detections);

            ComputeDepth(detections);

            Common.PrintSection("After Z Computation");
            Common.PrintDetections(detections);

            var points3D = ConvertTo3D(detections);

            Common.Print3DPoints(points3D);

            ComputeVelocity(points3D);

            Cv2.DestroyAllWindows();
        }

        // =========================
        // Frame Processing
        // =========================

        static List<BallDetection> ProcessFrames(string folderPath)
        {
            var files = Directory
                .GetFiles(folderPath)
                .OrderBy(x => x)
                .ToArray();

            var results = new List<BallDetection>();

            Mat previousGray = null;

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);

                Mat frame = Cv2.ImRead(file);

                Mat gray = new Mat();
                Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

                if (previousGray == null)
                {
                    previousGray = gray.Clone();

                    results.Add(new BallDetection(fileName));

                    continue;
                }

                BallDetection detection =
                    DetectBall(frame, gray, previousGray, fileName);

                results.Add(detection);

                previousGray = gray.Clone();
            }

            return results;
        }

        // =========================
        // Ball Detection
        // =========================

        static BallDetection DetectBall(Mat frame, Mat currentGray, Mat previousGray, string frameName)
        {
            Mat diff = new Mat();

            Cv2.Absdiff(currentGray, previousGray, diff);

            Mat binary = new Mat();

            Cv2.Threshold(diff, binary, 10, 255, ThresholdTypes.Binary);

            var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
            Cv2.MorphologyEx(binary, binary, MorphTypes.Open, kernel);

            Cv2.Dilate(binary, binary, kernel, iterations: 7);

            // ROI mask
            Mat mask = CreateTriangleMask(frame.Size());

            Mat merged = new Mat();

            Cv2.BitwiseAnd(mask, binary, merged);

            Mat filtered = new Mat();

            Cv2.CopyTo(currentGray, filtered, merged);

            Cv2.GaussianBlur(filtered, filtered, new Size(9, 9), 2);

            CircleSegment[] circles = Cv2.HoughCircles(filtered, HoughModes.Gradient, dp: 1.2, minDist: 500, param1: 120, param2: 26, minRadius: 5, maxRadius: 27);

            Mat output = frame.Clone();

            if (circles.Length == 1)
            {
                var c = circles[0];

                Point center = new Point((int)c.Center.X, (int)c.Center.Y);

                int radius = (int)c.Radius;

                DrawDetection(output, center, radius);

                Console.WriteLine(
                    $"Detected: {frameName} " +
                    $"Center=({center.X},{center.Y}) " +
                    $"Radius={radius}");

                ShowPreview(output);

                return new BallDetection(frameName, center.X, center.Y, radius);
            }

            ShowPreview(output);

            return new BallDetection(frameName);
        }

        // =========================
        // Mask
        // =========================

        static Mat CreateTriangleMask(Size size)
        {
            Mat mask = Mat.Zeros(size, MatType.CV_8UC1);

            Point[] triangle =
            {
                new Point(0, size.Height * 1.1 / 3),
                new Point(0, size.Height * 1.8 / 3),
                new Point(size.Width * 1.9 / 3, size.Height * 2 / 3)
            };

            Cv2.FillPoly(mask, new[] { triangle }, Scalar.White);

            return mask;
        }

        // =========================
        // Draw Detection
        // =========================

        static void DrawDetection(Mat image, Point center, int radius)
        {
            Cv2.Circle(image, center, radius, new Scalar(0, 0, 255), 2);

            Cv2.Circle(image, center, 2, new Scalar(0, 255, 0), -1);
        }

        // =========================
        // Preview Window
        // =========================

        static void ShowPreview(Mat image)
        {
            Cv2.NamedWindow("Ball Detection", WindowFlags.KeepRatio);
            Cv2.ResizeWindow("Ball Detection", new Size(480, 680));
            Cv2.ImShow("Ball Detection", image);

            Cv2.WaitKey(1);
        }

        // =========================
        // Interpolation
        // =========================

        static void InterpolateMissingFrames(List<BallDetection> detections)
        {
            for (int i = 0; i < detections.Count; i++)
            {
                if (detections[i].IsValid)
                    continue;

                int left = FindLeftValid(detections, i);

                int right = FindRightValid(detections, i);

                if (left == -1 || right == -1)
                    continue;

                double t = (double)(i - left) / (right - left);

                detections[i].X = Linear_Interpolation(detections[left].X.Value, detections[right].X.Value, t);

                detections[i].Y = Linear_Interpolation(detections[left].Y.Value, detections[right].Y.Value, t);

                detections[i].Radius = Linear_Interpolation(detections[left].Radius.Value, detections[right].Radius.Value, t);
            }
        }

        static int FindLeftValid(List<BallDetection> list, int index)
        {
            for (int i = index - 1; i >= 0; i--)
            {
                if (list[i].IsValid)
                    return i;
            }

            return -1;
        }

        static int FindRightValid(List<BallDetection> list, int index)
        {
            for (int i = index + 1; i < list.Count; i++)
            {
                if (list[i].IsValid)
                    return i;
            }

            return -1;
        }

        static double Linear_Interpolation(double a, double b, double t)
        {
            return a + (b - a) * t;
        }

        // =========================
        // Compute Z
        // =========================

        static void ComputeDepth(List<BallDetection> detections)
        {
            foreach (var d in detections)
            {
                if (!d.IsValid)
                    continue;

                double z = (CameraConfig.FocalLength * CameraConfig.BallRadius) / (d.Radius.Value * CameraConfig.PixelSize);

                d.Z = z;
            }
        }

        // =========================
        // Pixel -> 3D
        // =========================

        static List<(double X, double Y, double Z)>
            ConvertTo3D(List<BallDetection> detections)
        {
            var points =
                new List<(double, double, double)>();

            foreach (var d in detections)
            {
                if (!d.IsValid || d.Z == null)
                    continue;

                double X = (d.X.Value - CameraConfig.Cx) * CameraConfig.PixelSize * d.Z.Value / CameraConfig.FocalLength;

                double Y = (d.Y.Value - CameraConfig.Cy) * CameraConfig.PixelSize * d.Z.Value / CameraConfig.FocalLength;

                double Z = d.Z.Value;

                points.Add((X, Y, Z));
            }

            return points;
        }

        // =========================
        // Velocity
        // =========================

        static void ComputeVelocity(List<(double X, double Y, double Z)> points)
        {
            if (points.Count < 2)
            {
                Console.WriteLine(
                    "Not enough points.");
                return;
            }

            double dt = 1.0 / CameraConfig.FPS;

            var first = points.First();
            var last = points.Last();

            double totalTime = dt * (points.Count - 1);

            double vx = (last.X - first.X) / totalTime;

            double vy = (last.Y - first.Y) / totalTime;

            double vz = (last.Z - first.Z) / totalTime;

            // pitch correction
            double theta = Common.Deg2Rad(CameraConfig.TiltDeg);

            double vxWorld = vx;

            double vyWorld = vy * Math.Cos(theta) - vz * Math.Sin(theta);

            double vzWorld = vy * Math.Sin(theta) + vz * Math.Cos(theta);
            double speed =
                Math.Sqrt(vxWorld * vxWorld + vyWorld * vyWorld + vzWorld * vzWorld);
            Console.WriteLine();
            Console.WriteLine(
                "===== SPEED VECTOR (WORLD FRAME) =====");

            Console.WriteLine(
                $"Vx = {vxWorld:F2} m/s");

            Console.WriteLine(
                $"Vy = {vyWorld:F2} m/s");

            Console.WriteLine(
                $"Vz = {vzWorld:F2} m/s");

            Console.WriteLine(
                $"Speed Magnitude = {speed:F2} m/s");
        }


    }
}