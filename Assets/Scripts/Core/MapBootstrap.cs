using CesiumForUnity;
using GeoVision.Data;
using GeoVision.Map;
using Unity.Mathematics;
using UnityEngine;

namespace GeoVision.Core
{
    /// <summary>
    /// Creates the GIS runtime root under the existing CesiumGeoreference.
    /// Does not modify globe tileset setup.
    /// </summary>
    public class MapBootstrap : MonoBehaviour
    {
        private const double HebeiLongitude = 114.5149;
        private const double HebeiLatitude = 38.0428;
        private const double HebeiViewHeightMeters = 380000.0;
        private const string TrajectoryResource = "Data/trajectory";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            if (FindObjectOfType<MapBootstrap>() != null)
            {
                return;
            }

            CesiumGeoreference georeference = FindObjectOfType<CesiumGeoreference>();
            if (georeference == null)
            {
                Debug.LogError("MapBootstrap: no CesiumGeoreference in scene.");
                return;
            }

            GameObject root = new GameObject("GisRoot");
            root.transform.SetParent(georeference.transform, false);
            root.AddComponent<MapBootstrap>();
        }

        private void Start()
        {
            CesiumGeoreference georeference = GetComponentInParent<CesiumGeoreference>();
            if (georeference == null)
            {
                Debug.LogError("MapBootstrap: GisRoot is not under CesiumGeoreference.");
                return;
            }

            georeference.Initialize();
            FocusHebeiCamera();

            HebeiBoundaryRenderer boundary = GetComponent<HebeiBoundaryRenderer>();
            if (boundary == null)
            {
                boundary = gameObject.AddComponent<HebeiBoundaryRenderer>();
            }

            boundary.Build(georeference);

            PoiSpawner spawner = GetComponent<PoiSpawner>();
            if (spawner == null)
            {
                spawner = gameObject.AddComponent<PoiSpawner>();
            }

            spawner.Spawn(new GeoDataService());
            LogPoiVisibility(spawner);

            PoiClusterSystem cluster = GetComponent<PoiClusterSystem>();
            if (cluster == null)
            {
                cluster = gameObject.AddComponent<PoiClusterSystem>();
            }

            cluster.SetPois(spawner.SpawnedPois);
            BootstrapQueryAndTrajectory(georeference);
        }

        private void BootstrapQueryAndTrajectory(CesiumGeoreference georeference)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("MapBootstrap: Main Camera not found for query/trajectory.");
                return;
            }

            CesiumFlyToController cesiumFlyTo = camera.GetComponent<CesiumFlyToController>();
            CesiumCameraController cameraController = camera.GetComponent<CesiumCameraController>();
            CesiumGlobeAnchor cameraAnchor = camera.GetComponent<CesiumGlobeAnchor>();
            if (cesiumFlyTo == null || cameraController == null || cameraAnchor == null)
            {
                Debug.LogError("MapBootstrap: DynamicCamera is missing Cesium FlyTo/Camera/GlobeAnchor.");
                return;
            }

            GisDashboard dashboard = FindObjectOfType<GisDashboard>();
            GeoInfoPanel infoPanel = FindObjectOfType<GeoInfoPanel>();
            TrajectoryPanel trajectoryPanel = FindObjectOfType<TrajectoryPanel>();
            if (dashboard == null || infoPanel == null || trajectoryPanel == null)
            {
                Debug.LogError("MapBootstrap: GisHud is missing from the scene. Place Assets/Prefabs/UI/GisHud.prefab in SampleScene.");
                return;
            }

            GeoDataService dataService = new GeoDataService();
            TrajectoryData trajectory = dataService.LoadJson<TrajectoryData>(TrajectoryResource);
            if (trajectory == null || trajectory.points == null || trajectory.points.Length < 2)
            {
                Debug.LogError("MapBootstrap: failed to load mock trajectory JSON.");
                return;
            }

            GameObject vehicleObject = new GameObject("PatrolVehicle");
            vehicleObject.transform.SetParent(transform, false);
            GeoVehicle vehicle = vehicleObject.AddComponent<GeoVehicle>();
            TrajectoryPoint first = trajectory.points[0];
            vehicle.Initialize(
                trajectory.vehicleId,
                trajectory.name,
                new double3(first.longitude, first.latitude, first.height));

            GeoRouteRenderer route = GetComponent<GeoRouteRenderer>();
            if (route == null)
            {
                route = gameObject.AddComponent<GeoRouteRenderer>();
            }

            route.Build(georeference, trajectory.points);

            TrajectoryPlayer player = GetComponent<TrajectoryPlayer>();
            if (player == null)
            {
                player = gameObject.AddComponent<TrajectoryPlayer>();
            }

            player.Bind(georeference, vehicle, trajectory);

            CameraFollowController follow = GetComponent<CameraFollowController>();
            if (follow == null)
            {
                follow = gameObject.AddComponent<CameraFollowController>();
            }

            follow.Bind(cameraAnchor, cameraController, vehicle);

            GeoFlyToController flyTo = GetComponent<GeoFlyToController>();
            if (flyTo == null)
            {
                flyTo = gameObject.AddComponent<GeoFlyToController>();
            }

            flyTo.Bind(cesiumFlyTo, follow);

            PoiSpawner spawner = GetComponent<PoiSpawner>();
            PoiClusterSystem cluster = GetComponent<PoiClusterSystem>();
            HebeiBoundaryRenderer boundary = GetComponent<HebeiBoundaryRenderer>();

            LayerManager layers = GetComponent<LayerManager>();
            if (layers == null)
            {
                layers = gameObject.AddComponent<LayerManager>();
            }

            layers.Bind(cluster, boundary, route, vehicle);

            infoPanel.Bind(flyTo);

            PickController pick = GetComponent<PickController>();
            if (pick == null)
            {
                pick = gameObject.AddComponent<PickController>();
            }

            pick.Bind(infoPanel);
            dashboard.Bind(spawner, layers, player, pick);
            trajectoryPanel.Bind(player, follow);
        }

        private static void FocusHebeiCamera()
        {
            Camera main = Camera.main;
            if (main == null)
            {
                Debug.LogError("MapBootstrap: Main Camera not found.");
                return;
            }

            CesiumGlobeAnchor cameraAnchor = main.GetComponent<CesiumGlobeAnchor>();
            if (cameraAnchor == null)
            {
                Debug.LogError("MapBootstrap: DynamicCamera is missing CesiumGlobeAnchor.");
                return;
            }

            cameraAnchor.longitudeLatitudeHeight = new double3(
                HebeiLongitude,
                HebeiLatitude,
                HebeiViewHeightMeters);
            cameraAnchor.rotationEastUpNorth = Quaternion.Euler(-89.0f, 0.0f, 0.0f);
            Debug.Log("MapBootstrap: camera at Hebei lon=" + HebeiLongitude + " lat=" + HebeiLatitude + " h=" + HebeiViewHeightMeters);
        }

        private static void LogPoiVisibility(PoiSpawner spawner)
        {
            if (spawner == null || spawner.SpawnedPois.Count == 0 || Camera.main == null)
            {
                return;
            }

            GeoPoi first = spawner.SpawnedPois[0];
            float distance = Vector3.Distance(Camera.main.transform.position, first.transform.position);
            Debug.Log(
                "MapBootstrap: first POI world=" + first.transform.position +
                " camera=" + Camera.main.transform.position +
                " distance=" + distance.ToString("F0") + "m");
        }
    }
}
