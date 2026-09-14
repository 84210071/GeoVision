using UnityEngine;

namespace GeoVision.Map
{
    /// <summary>
    /// Faces the camera and keeps a stable screen size. Overlay shader draws on top of the globe.
    /// </summary>
    public class PoiBillboard : MonoBehaviour
    {
        [SerializeField] private float minWorldSize = 1200f;
        [SerializeField] private float maxWorldSize = 28000f;
        [SerializeField] private float distanceScale = 0.028f;

        public void Configure(float minSize, float maxSize, float scale)
        {
            minWorldSize = minSize;
            maxWorldSize = maxSize;
            distanceScale = scale;
        }

        private void LateUpdate()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            Vector3 up = transform.parent != null ? transform.parent.up : Vector3.up;
            Vector3 toCamera = camera.transform.position - transform.position;
            if (toCamera.sqrMagnitude < 0.001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(toCamera.normalized, up);

            float worldSize = Mathf.Clamp(toCamera.magnitude * distanceScale, minWorldSize, maxWorldSize);
            transform.localScale = new Vector3(worldSize, worldSize, worldSize);
            transform.localPosition = new Vector3(0f, worldSize * 0.35f, 0f);
        }
    }
}
