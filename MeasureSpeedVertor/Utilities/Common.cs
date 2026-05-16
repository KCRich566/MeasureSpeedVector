using MeasureSpeedVector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureSpeedVertor.Utilities
{
    public static class Common
    {
        public static double Deg2Rad(double deg)
        {
            return deg * Math.PI / 180.0;
        }

        public static void PrintSection(string title)
        {
            Console.WriteLine();
            Console.WriteLine(
                "=================================");
            Console.WriteLine(title);
            Console.WriteLine(
                "=================================");
        }

        public static void PrintDetections(
            List<BallDetection> detections)
        {
            foreach (var d in detections)
            {
                Console.WriteLine(d);
            }
        }

        public static void Print3DPoints(
            List<(double X, double Y, double Z)> points)
        {
            PrintSection("3D Coordinates");

            int index = 0;

            foreach (var p in points)
            {
                Console.WriteLine(
                    $"[{index}] " +
                    $"X={p.X:F3}, " +
                    $"Y={p.Y:F3}, " +
                    $"Z={p.Z:F3}");

                index++;
            }
        }

        
    }
}
