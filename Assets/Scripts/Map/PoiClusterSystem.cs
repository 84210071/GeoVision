using System.Collections.Generic;
using CesiumForUnity;
using Unity.Mathematics;
using UnityEngine;

namespace GeoVision.Map
{
    /// <summary>
    /// Province-scale zoom shows one cluster; closer zoom shows individual POIs.
    /// </summary>
    public class PoiClusterSystem : MonoBehaviour
    {
        [SerializeField] private float clusterHeightMeters = 90000f;

        private readonly List<GeoPoi> _pois = new List<GeoPoi>();
        private GameObject _clusterRoot;
        private CesiumGlobeAnchor _clusterAnchor;
        private CesiumGlobeAnchor _cameraAnchor;
        private bool _clusterMode;
        private GUIStyle _labelStyle;

        public void SetPois(List<GeoPoi> pois)
        {
            _pois.Clear();
            if (pois != null)
            {
                _pois.AddRange(pois);
            }

            EnsureClusterMarker();
            Debug.Log("PoiClusterSystem: tracking " + _pois.Count + " POIs. Cluster when camera height > " + clusterHeightMeters + " m.");
        }

        private void LateUpdate()
        {
            if (_pois.Count == 0)
            {
                return;
            }

            if (_cameraAnchor == null && Camera.main != null)
            {
                _cameraAnchor = Camera.main.GetComponent<CesiumGlobeAnchor>();
            }

            double cameraHeight = _cameraAnchor != null ? _cameraAnchor.longitudeLatitudeHeight.z : 0.0;
            bool clusterMode = cameraHeight > clusterHeightMeters;
            if (clusterMode != _clusterMode)
            {
                _clusterMode = clusterMode;
                Debug.Log("PoiClusterSystem: " + (clusterMode ? "cluster ON" : "cluster OFF") + " cameraHeight=" + cameraHeight.ToString("F0"));
            }

            for (int i = 0; i < _pois.Count; i++)
            {
                if (_pois[i] != null)
                {
                    _pois[i].SetVisible(!clusterMode);
                }
            }

            if (_clusterRoot != null)
            {
                _clusterRoot.SetActive(clusterMode);
            }

            if (clusterMode)
            {
                UpdateClusterPose();
            }
        }

        private void OnGUI()
        {
            if (!_clusterMode || _clusterRoot == null || !_clusterRoot.activeInHierarchy || Camera.main == null)
            {
                return;
            }

            Vector3 screen = Camera.main.WorldToScreenPoint(_clusterRoot.transform.position);
            if (screen.z <= 0f)
            {
                return;
            }

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                _labelStyle.normal.textColor = Color.white;
            }

            float x = screen.x;
            float y = Screen.height - screen.y;
            GUI.Box(new Rect(x - 36f, y - 22f, 72f, 44f), _pois.Count.ToString(), _labelStyle);
        }

        private void EnsureClusterMarker()
        {
            if (_clusterRoot != null)
            {
                return;
            }

            _clusterRoot = new GameObject("PoiCluster");
            _clusterRoot.transform.SetParent(transform, false);

            _clusterAnchor = _clusterRoot.AddComponent<CesiumGlobeAnchor>();
            _clusterAnchor.adjustOrientationForGlobeWhenMoving = true;
            _clusterAnchor.detectTransformChanges = false;
            _clusterAnchor.longitudeLatitudeHeight = ComputeCenter();

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
            visual.name = "Visual";
            visual.transform.SetParent(_clusterRoot.transform, false);
            Collider collider = visual.GetComponent<MeshCollider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            MeshRenderer renderer = visual.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = GeoOverlayMaterial.Create(
                new Color(1f, 0.5f, 0.08f, 1f),
                PoiIconLibrary.GetIcon("project"));
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            PoiBillboard billboard = visual.AddComponent<PoiBillboard>();
            billboard.Configure(6000f, 48000f, 0.055f);
        }

        private void UpdateClusterPose()
        {
            if (_clusterAnchor == null)
            {
                return;
            }

            _clusterAnchor.longitudeLatitudeHeight = ComputeCenter();
        }

        private double3 ComputeCenter()
        {
            double lon = 0;
            double lat = 0;
            double height = 0;
            int count = 0;
            for (int i = 0; i < _pois.Count; i++)
            {
                if (_pois[i] == null || _pois[i].Data == null)
                {
                    continue;
                }

                lon += _pois[i].Data.longitude;
                lat += _pois[i].Data.latitude;
                height += math.max(_pois[i].Data.height, 500.0);
                count++;
            }

            if (count == 0)
            {
                return new double3(114.5149, 38.0428, 500.0);
            }

            return new double3(lon / count, lat / count, height / count);
        }
    }
}
