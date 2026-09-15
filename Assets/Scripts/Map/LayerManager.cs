using UnityEngine;

namespace GeoVision.Map
{
    /// <summary>
    /// Turns existing GIS overlays on/off. Does not own spawn or render logic.
    /// </summary>
    public class LayerManager : MonoBehaviour
    {
        public bool PoiEnabled { get; private set; } = true;
        public bool BoundaryEnabled { get; private set; } = true;
        public bool TrajectoryEnabled { get; private set; } = true;
        public bool AlertEnabled { get; private set; } = true;

        private PoiClusterSystem _cluster;
        private HebeiBoundaryRenderer _boundary;
        private GeoRouteRenderer _route;
        private GeoVehicle _vehicle;

        public void Bind(
            PoiClusterSystem cluster,
            HebeiBoundaryRenderer boundary,
            GeoRouteRenderer route,
            GeoVehicle vehicle)
        {
            _cluster = cluster;
            _boundary = boundary;
            _route = route;
            _vehicle = vehicle;
            Apply();
        }

        public void SetPoi(bool visible)
        {
            PoiEnabled = visible;
            Apply();
        }

        public void SetBoundary(bool visible)
        {
            BoundaryEnabled = visible;
            Apply();
        }

        public void SetTrajectory(bool visible)
        {
            TrajectoryEnabled = visible;
            Apply();
        }

        public void SetAlert(bool visible)
        {
            AlertEnabled = visible;
            Apply();
        }

        private void Apply()
        {
            if (_cluster != null)
            {
                _cluster.SetLayerVisible(PoiEnabled, AlertEnabled);
            }

            if (_boundary != null)
            {
                _boundary.SetVisible(BoundaryEnabled);
            }

            if (_route != null)
            {
                _route.SetVisible(TrajectoryEnabled);
            }

            if (_vehicle != null)
            {
                _vehicle.SetVisible(TrajectoryEnabled);
            }
        }
    }
}
