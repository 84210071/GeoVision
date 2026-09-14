using CesiumForUnity;
using Unity.Mathematics;
using UnityEngine;

namespace GeoVision.Map
{
    /// <summary>
    /// Double-precision globe helpers. All ECEF conversions go through Cesium APIs.
    /// </summary>
    public static class GeoGlobeUtil
    {
        public static double3 ToEcef(CesiumGeoreference georeference, double longitude, double latitude, double height)
        {
            return georeference.ellipsoid.LongitudeLatitudeHeightToCenteredFixed(
                new double3(longitude, latitude, height));
        }

        public static double3 EcefToLlh(CesiumGeoreference georeference, double3 ecef)
        {
            return georeference.ellipsoid.CenteredFixedToLongitudeLatitudeHeight(ecef);
        }

        public static Vector3 EcefToGeoreferenceLocal(CesiumGeoreference georeference, double3 ecef)
        {
            double3 local = georeference.TransformEarthCenteredEarthFixedPositionToUnity(ecef);
            return new Vector3((float)local.x, (float)local.y, (float)local.z);
        }

        public static double3 InterpolateLlh(double3 sourceLlh, double3 destinationLlh, double t)
        {
            t = math.clamp(t, 0.0, 1.0);
            double dLon = destinationLlh.x - sourceLlh.x;
            if (dLon > 180.0)
            {
                dLon -= 360.0;
            }
            else if (dLon < -180.0)
            {
                dLon += 360.0;
            }

            return new double3(
                sourceLlh.x + dLon * t,
                math.lerp(sourceLlh.y, destinationLlh.y, t),
                math.lerp(sourceLlh.z, destinationLlh.z, t));
        }

        public static float HeadingYawDegrees(double fromLon, double fromLat, double toLon, double toLat)
        {
            double lat1 = fromLat * math.PI_DBL / 180.0;
            double lat2 = toLat * math.PI_DBL / 180.0;
            double dLon = (toLon - fromLon) * math.PI_DBL / 180.0;
            double east = math.sin(dLon) * math.cos(lat2);
            double north = math.cos(lat1) * math.sin(lat2) - math.sin(lat1) * math.cos(lat2) * math.cos(dLon);
            if (math.abs(east) < 1e-12 && math.abs(north) < 1e-12)
            {
                return 0f;
            }

            double bearing = math.atan2(east, north) * 180.0 / math.PI_DBL;
            if (bearing < 0.0)
            {
                bearing += 360.0;
            }

            return (float)bearing;
        }
    }
}
