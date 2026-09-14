using System.Collections.Generic;
using CesiumForUnity;
using GeoVision.Data;
using Unity.Mathematics;
using UnityEngine;

namespace GeoVision.Map
{
    public class HebeiBoundaryRenderer : MonoBehaviour
    {
        private const string ResourcePath = "Data/hebei-boundary";
        private const double OutlineHeightMeters = 400.0;
        private const int MaxPointsPerRing = 900;

        private readonly List<double2[]> _rings = new List<double2[]>();
        private readonly List<LineRenderer> _lines = new List<LineRenderer>();
        private readonly List<Vector3[]> _buffers = new List<Vector3[]>();
        private CesiumGeoreference _georeference;

        public int RingCount => _rings.Count;

        public void Build(CesiumGeoreference georeference)
        {
            _georeference = georeference;
            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset == null)
            {
                Debug.LogError("HebeiBoundaryRenderer: missing Resources/Data/hebei-boundary.json");
                return;
            }

            List<List<double2>> parsed = GeoJsonRingParser.ExtractRings(asset.text);
            ClearLines();
            _rings.Clear();
            _buffers.Clear();

            for (int i = 0; i < parsed.Count; i++)
            {
                double2[] ring = Downsample(parsed[i]);
                if (ring.Length < 2)
                {
                    continue;
                }

                _rings.Add(ring);
                _buffers.Add(new Vector3[ring.Length]);
                _lines.Add(CreateLine(ring.Length, i == 0));
            }

            Debug.Log("HebeiBoundaryRenderer: drew " + _rings.Count + " outline rings from " + parsed.Count + " GeoJSON rings.");
            RefreshPositions();
        }

        private void LateUpdate()
        {
            RefreshPositions();
        }

        private void RefreshPositions()
        {
            if (_georeference == null || _rings.Count == 0)
            {
                return;
            }

            _georeference.Initialize();
            float width = ResolveLineWidth();

            for (int r = 0; r < _rings.Count; r++)
            {
                double2[] ring = _rings[r];
                LineRenderer line = _lines[r];
                if (line == null)
                {
                    continue;
                }

                Vector3[] points = _buffers[r];
                for (int i = 0; i < ring.Length; i++)
                {
                    double3 ecef = _georeference.ellipsoid.LongitudeLatitudeHeightToCenteredFixed(
                        new double3(ring[i].x, ring[i].y, OutlineHeightMeters));
                    double3 local = _georeference.TransformEarthCenteredEarthFixedPositionToUnity(ecef);
                    points[i] = new Vector3((float)local.x, (float)local.y, (float)local.z);
                }

                line.positionCount = points.Length;
                line.SetPositions(points);
                float ringWidth = r == 0 ? width : width * 0.55f;
                line.startWidth = ringWidth;
                line.endWidth = ringWidth;
            }
        }

        private float ResolveLineWidth()
        {
            Camera camera = Camera.main;
            double height = 380000.0;
            if (camera != null)
            {
                CesiumGlobeAnchor cameraAnchor = camera.GetComponent<CesiumGlobeAnchor>();
                if (cameraAnchor != null)
                {
                    height = math.abs(cameraAnchor.longitudeLatitudeHeight.z);
                }
            }

            return (float)math.clamp(height * 0.012, 2500.0, 40000.0);
        }

        private LineRenderer CreateLine(int pointCount, bool primary)
        {
            GameObject lineObject = new GameObject(primary ? "HebeiOutline" : "HebeiOutlineHole");
            lineObject.transform.SetParent(_georeference != null ? _georeference.transform : transform, false);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = pointCount;
            line.numCapVertices = 2;
            line.numCornerVertices = 2;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.alignment = LineAlignment.View;
            line.widthMultiplier = 1f;
            Color outline = primary
                ? new Color(0.15f, 0.95f, 1f, 1f)
                : new Color(0.45f, 0.85f, 1f, 0.85f);
            line.material = GeoOverlayMaterial.Create(outline, Texture2D.whiteTexture);
            return line;
        }

        private static double2[] Downsample(List<double2> ring)
        {
            if (ring.Count <= MaxPointsPerRing)
            {
                return ring.ToArray();
            }

            int step = Mathf.CeilToInt(ring.Count / (float)MaxPointsPerRing);
            List<double2> sampled = new List<double2>();
            for (int i = 0; i < ring.Count; i += step)
            {
                sampled.Add(ring[i]);
            }

            double2 first = sampled[0];
            double2 last = sampled[sampled.Count - 1];
            if (math.abs(first.x - last.x) > 0.000001 || math.abs(first.y - last.y) > 0.000001)
            {
                sampled.Add(first);
            }

            return sampled.ToArray();
        }

        private void ClearLines()
        {
            for (int i = _lines.Count - 1; i >= 0; i--)
            {
                if (_lines[i] != null)
                {
                    Destroy(_lines[i].gameObject);
                }
            }

            _lines.Clear();
        }
    }
}
