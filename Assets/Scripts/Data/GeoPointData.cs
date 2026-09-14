using System;

namespace GeoVision.Data
{
    [Serializable]
    public class GeoPointData
    {
        public string id;
        public string name;
        public double longitude;
        public double latitude;
        public double height;
        public string type;
        public string status;
        public string description;
    }

    [Serializable]
    public class GeoPointCollection
    {
        public GeoPointData[] items;
    }
}
