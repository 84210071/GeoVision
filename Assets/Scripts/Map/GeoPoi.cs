using CesiumForUnity;
using GeoVision.Data;
using Unity.Mathematics;
using UnityEngine;

namespace GeoVision.Map
{
    public class GeoPoi : MonoBehaviour, IGeoSelectable
    {
        [SerializeField] private GeoPointData data;

        private MeshRenderer _visualRenderer;
        private Color _baseColor;
        private bool _highlighted;

        public GeoPointData Data => data;

        public string Id => data != null ? data.id : string.Empty;
        public string DisplayName => data != null ? data.name : string.Empty;
        public string Type => data != null ? data.type : string.Empty;
        public string Status => data != null ? data.status : string.Empty;
        public string Description => data != null ? data.description : string.Empty;
        public double Longitude => data != null ? data.longitude : 0.0;
        public double Latitude => data != null ? data.latitude : 0.0;
        public double Height => data != null ? data.height : 0.0;

        public void Bind(GeoPointData pointData)
        {
            data = pointData;
            name = "POI_" + pointData.id + "_" + pointData.name;

            CesiumGlobeAnchor anchor = GetComponent<CesiumGlobeAnchor>();
            if (anchor == null)
            {
                anchor = gameObject.AddComponent<CesiumGlobeAnchor>();
            }

            anchor.adjustOrientationForGlobeWhenMoving = true;
            anchor.detectTransformChanges = false;
            anchor.longitudeLatitudeHeight = new double3(
                pointData.longitude,
                pointData.latitude,
                math.max(pointData.height, 250.0));

            CreateOrUpdateVisual(pointData);
        }

        public void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible)
            {
                gameObject.SetActive(visible);
            }
        }

        public void SetHighlighted(bool highlighted)
        {
            _highlighted = highlighted;
            if (_visualRenderer == null)
            {
                return;
            }

            Color color = highlighted
                ? new Color(1f, 0.92f, 0.2f, 1f)
                : _baseColor;
            _visualRenderer.material.SetColor("_Color", color);
            _visualRenderer.material.color = color;
        }

        private void CreateOrUpdateVisual(GeoPointData pointData)
        {
            Transform visualTransform = transform.Find("Visual");
            GameObject visual = visualTransform != null
                ? visualTransform.gameObject
                : GameObject.CreatePrimitive(PrimitiveType.Quad);
            visual.name = "Visual";
            visual.transform.SetParent(transform, false);
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localPosition = Vector3.zero;
            visual.layer = gameObject.layer;

            Collider meshCollider = visual.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                DestroyCollider(meshCollider);
            }

            BoxCollider boxCollider = visual.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = visual.AddComponent<BoxCollider>();
            }

            boxCollider.isTrigger = true;

            _visualRenderer = visual.GetComponent<MeshRenderer>();
            _baseColor = PoiIconLibrary.GetTypeColor(pointData.type) * PoiIconLibrary.GetStatusTint(pointData.status);
            _visualRenderer.sharedMaterial = GeoOverlayMaterial.Create(_baseColor, PoiIconLibrary.GetIcon(pointData.type));
            _visualRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _visualRenderer.receiveShadows = false;

            PoiBillboard billboard = visual.GetComponent<PoiBillboard>();
            if (billboard == null)
            {
                billboard = visual.AddComponent<PoiBillboard>();
            }

            billboard.Configure(1400f, 22000f, 0.03f);
            SetHighlighted(_highlighted);
        }

        private static void DestroyCollider(Collider collider)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(collider);
            }
            else
            {
                Object.DestroyImmediate(collider);
            }
        }
    }
}
