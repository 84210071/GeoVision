using CesiumForUnity;
using Unity.Mathematics;
using UnityEngine;

namespace GeoVision.Map
{
    public class GeoVehicle : MonoBehaviour, IGeoSelectable
    {
        private CesiumGlobeAnchor _anchor;
        private MeshRenderer[] _renderers;
        private Color _baseColor = new Color(1f, 0.55f, 0.08f, 1f);
        private string _id = "vehicle-patrol-01";
        private string _displayName = "巡检车-01";
        private string _status = "normal";
        private string _description = "模拟工程巡检车，沿河北省石家庄-保定-沧州-衡水-邢台回放。";

        public CesiumGlobeAnchor Anchor => _anchor;
        public string Id => _id;
        public string DisplayName => _displayName;
        public string Type => "vehicle";
        public string Status => _status;
        public string Description => _description;
        public double Longitude => _anchor != null ? _anchor.longitudeLatitudeHeight.x : 0.0;
        public double Latitude => _anchor != null ? _anchor.longitudeLatitudeHeight.y : 0.0;
        public double Height => _anchor != null ? _anchor.longitudeLatitudeHeight.z : 0.0;

        public void Initialize(string id, string displayName, double3 startLlh)
        {
            _id = id;
            _displayName = displayName;
            name = "Vehicle_" + id;

            _anchor = GetComponent<CesiumGlobeAnchor>();
            if (_anchor == null)
            {
                _anchor = gameObject.AddComponent<CesiumGlobeAnchor>();
            }

            _anchor.adjustOrientationForGlobeWhenMoving = true;
            _anchor.detectTransformChanges = false;
            _anchor.longitudeLatitudeHeight = startLlh;
            CreateVisual();
        }

        public void SetGlobePose(double3 longitudeLatitudeHeight, float yawDegrees)
        {
            if (_anchor == null)
            {
                return;
            }

            _anchor.longitudeLatitudeHeight = longitudeLatitudeHeight;
            _anchor.rotationEastUpNorth = Quaternion.Euler(0f, yawDegrees, 0f);
        }

        public void SetVisible(bool visible)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = visible;
            }

            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = visible;
            }
        }

        public void SetHighlighted(bool highlighted)
        {
            if (_renderers == null)
            {
                return;
            }

            Color color = highlighted ? new Color(1f, 0.92f, 0.2f, 1f) : _baseColor;
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null)
                {
                    continue;
                }

                _renderers[i].material.SetColor("_Color", color);
                _renderers[i].material.color = color;
            }
        }

        private void CreateVisual()
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(transform, false);
            body.transform.localPosition = new Vector3(0f, 4f, 0f);
            body.transform.localScale = new Vector3(18f, 8f, 42f);
            StylePrimitive(body);

            GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabin";
            cabin.transform.SetParent(transform, false);
            cabin.transform.localPosition = new Vector3(0f, 10f, 8f);
            cabin.transform.localScale = new Vector3(14f, 6f, 16f);
            StylePrimitive(cabin);

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.name = "Marker";
            marker.transform.SetParent(transform, false);
            Collider markerCollider = marker.GetComponent<MeshCollider>();
            if (markerCollider != null)
            {
                Destroy(markerCollider);
            }

            StylePrimitive(marker);
            PoiBillboard billboard = marker.AddComponent<PoiBillboard>();
            billboard.Configure(800f, 32000f, 0.025f);

            _renderers = GetComponentsInChildren<MeshRenderer>();
            SetHighlighted(false);
        }

        private void StylePrimitive(GameObject primitive)
        {
            Collider meshCollider = primitive.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                Destroy(meshCollider);
            }

            BoxCollider box = primitive.GetComponent<BoxCollider>();
            if (box == null)
            {
                box = primitive.AddComponent<BoxCollider>();
            }

            box.isTrigger = true;
            MeshRenderer renderer = primitive.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = GeoOverlayMaterial.Create(_baseColor, Texture2D.whiteTexture);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }
}
