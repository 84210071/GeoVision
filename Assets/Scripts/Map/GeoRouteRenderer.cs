using System.Collections.Generic;
using CesiumForUnity;
using GeoVision.Data;
using Unity.Mathematics;
using UnityEngine;

namespace GeoVision.Map
{
    public class GeoRouteRenderer : MonoBehaviour
    {
        private const int SamplesPerSegment = 3;
        private const double RouteHeightMeters = 400.0;

        private readonly List<double3> _llhPoints = new List<double3>();
        private Vector3[] _buffer;
        private LineRenderer _line;
        private CesiumGeoreference _georeference;

        public void Build(CesiumGeoreference georeference, TrajectoryPoint[] points)
        {
            _georeference = georeference;
            _llhPoints.Clear();
            if (georeference == null || points == null || points.Length < 2)
            {
                Debug.LogError("GeoRouteRenderer: need a georeference and at least 2 trajectory points.");
                return;
            }

            georeference.Initialize();
            Densify(points);
            EnsureLine();
            _buffer = new Vector3[_llhPoints.Count];
            Debug.Log("GeoRouteRenderer: " + points.Length + " source points, " + _llhPoints.Count + " draw samples inside Hebei.");
            RefreshPositions();
        }

        private void LateUpdate()
        {
            RefreshPositions();
        }

        private void Densify(TrajectoryPoint[] points)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                double3 from = new double3(points[i].longitude, points[i].latitude, RouteHeightMeters);
                double3 to = new double3(points[i + 1].longitude, points[i + 1].latitude, RouteHeightMeters);
                int start = i == 0 ? 0 : 1;
                for (int s = start; s <= SamplesPerSegment; s++)
                {
                    double t = s / (double)SamplesPerSegment;
                    _llhPoints.Add(GeoGlobeUtil.InterpolateLlh(from, to, t));
                }
            }
        }

        private void RefreshPositions()
        {
            if (_georeference == null || _line == null || _llhPoints.Count == 0)
            {
                return;
            }

            _georeference.Initialize();
            for (int i = 0; i < _llhPoints.Count; i++)
            {
                double3 ecef = GeoGlobeUtil.ToEcef(_georeference, _llhPoints[i].x, _llhPoints[i].y, _llhPoints[i].z);
                _buffer[i] = GeoGlobeUtil.EcefToGeoreferenceLocal(_georeference, ecef);
            }

            _line.positionCount = _buffer.Length;
            _line.SetPositions(_buffer);

            double height = 8000.0;
            Camera camera = Camera.main;
            if (camera != null)
            {
                CesiumGlobeAnchor cameraAnchor = camera.GetComponent<CesiumGlobeAnchor>();
                if (cameraAnchor != null)
                {
                    height = math.abs(cameraAnchor.longitudeLatitudeHeight.z);
                }
            }

            float width = (float)math.clamp(height * 0.010, 400.0, 18000.0);
            _line.startWidth = width;
            _line.endWidth = width;
        }

        private void EnsureLine()
        {
            if (_line != null)
            {
                return;
            }

            GameObject lineObject = new GameObject("TrajectoryRoute");
            lineObject.transform.SetParent(_georeference != null ? _georeference.transform : transform, false);
            _line = lineObject.AddComponent<LineRenderer>();
            _line.useWorldSpace = false;
            _line.loop = false;
            _line.numCapVertices = 2;
            _line.numCornerVertices = 2;
            _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _line.receiveShadows = false;
            _line.alignment = LineAlignment.View;
            _line.widthMultiplier = 1f;
            _line.material = GeoOverlayMaterial.Create(new Color(1f, 0.82f, 0.15f, 0.95f), Texture2D.whiteTexture);
        }
    }
}
