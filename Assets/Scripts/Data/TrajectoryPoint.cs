using System;

namespace GeoVision.Data
{
    [Serializable]
    public class TrajectoryPoint
    {
        public double longitude;
        public double latitude;
        public double height;
        public string timestamp;
        public double speed;
    }

    [Serializable]
    public class TrajectoryData
    {
        public string vehicleId;
        public string name;
        public TrajectoryPoint[] points;
    }
}
