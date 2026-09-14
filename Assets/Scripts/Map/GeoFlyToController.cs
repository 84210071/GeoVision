using System.Reflection;
using CesiumForUnity;
using Unity.Mathematics;
using UnityEngine;

namespace GeoVision.Map
{
    /// <summary>
    /// Thin wrapper over CesiumFlyToController. Does not reimplement globe math.
    /// </summary>
    public class GeoFlyToController : MonoBehaviour
    {
        [SerializeField] private float destinationPitch = -48f;
        [SerializeField] private float destinationYaw = 0f;
        [SerializeField] private bool canInterruptByMoving = true;

        private CesiumFlyToController _cesiumFlyTo;
        private CameraFollowController _follow;

        public bool IsReady => _cesiumFlyTo != null;

        public void Bind(CesiumFlyToController flyTo, CameraFollowController follow)
        {
            _cesiumFlyTo = flyTo;
            _follow = follow;
        }

        public void FlyTo(double longitude, double latitude, double height)
        {
            FlyTo(longitude, latitude, height, destinationYaw, destinationPitch);
        }

        public void FlyTo(double longitude, double latitude, double height, float yaw, float pitch)
        {
            if (_cesiumFlyTo == null)
            {
                Debug.LogError("GeoFlyToController: CesiumFlyToController is missing.");
                return;
            }

            if (_follow != null && _follow.IsFollowing)
            {
                _follow.SetFollowing(false);
            }

            InterruptCurrentFlight();
            _cesiumFlyTo.FlyToLocationLongitudeLatitudeHeight(
                new double3(longitude, latitude, height),
                yaw,
                pitch,
                canInterruptByMoving);
            Debug.Log("GeoFlyTo: lon=" + longitude.ToString("F5") + " lat=" + latitude.ToString("F5") + " h=" + height.ToString("F0"));
        }

        public void FlyToSelectable(IGeoSelectable selectable, double cameraHeightMeters)
        {
            if (selectable == null)
            {
                return;
            }

            FlyTo(selectable.Longitude, selectable.Latitude, cameraHeightMeters);
        }

        private void InterruptCurrentFlight()
        {
            if (_cesiumFlyTo == null)
            {
                return;
            }

            MethodInfo interrupt = typeof(CesiumFlyToController).GetMethod(
                "InterruptFlight",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (interrupt != null)
            {
                interrupt.Invoke(_cesiumFlyTo, null);
            }
        }
    }
}
