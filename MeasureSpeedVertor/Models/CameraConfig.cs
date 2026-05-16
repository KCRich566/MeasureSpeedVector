using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureSpeedVector.Models
{
    internal class CameraConfig
    {
        static public double PixelSize { get; } = 0.0048e-3;
        static public double FocalLength { get; } = 8e-3;
        static public int FPS { get; } = 240;
        static public double BallRadius { get; } = 0.0373;

        static public int Width { get; } = 1024;
        static public int Height { get; } = 1280;

        static public double TiltDeg { get; } = 10;

        static public double Cx => Width / 2.0;
        static public double Cy => Height / 2.0;

      
    }
}
