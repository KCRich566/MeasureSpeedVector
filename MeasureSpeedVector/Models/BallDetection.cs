namespace MeasureSpeedVector.Models
{
    // =========================
    // Data Model
    // =========================

    public class BallDetection
    {
        public string FrameName { get; set; }

        public double? X { get; set; }

        public double? Y { get; set; }

        public double? Radius { get; set; }

        public double? Z { get; set; }

        public bool IsValid =>
            X != null &&
            Y != null &&
            Radius != null;

        public BallDetection(
            string frameName,
            double? x = null,
            double? y = null,
            double? radius = null,
            double? z = null)
        {
            FrameName = frameName;
            X = x;
            Y = y;
            Radius = radius;
            Z = z;
        }

        public override string ToString()
        {
            return
                $"{FrameName} | " +
                $"X={X?.ToString("F1") ?? "null"}, " +
                $"Y={Y?.ToString("F1") ?? "null"}, " +
                $"R={Radius?.ToString("F1") ?? "null"}, " +
                $"Z={Z?.ToString("F3") ?? "null"}";
        }
    }
}
