using CesiumForUnity;
using Unity.Mathematics;
using UnityEngine;

namespace GeoVision.Map
{
    /// <summary>
    /// When enabled, drives DynamicCamera via CesiumGlobeAnchor. When disabled, CesiumCameraController is restored.
    /// </summary>
    public class CameraFollowController : MonoBehaviour
    {
        [SerializeField] private double followHeightMeters = 900.0;
        [SerializeField] private float followPitch = -62f;

        private CesiumGlobeAnchor _cameraAnchor;
        private CesiumCameraController _cameraController;
        private GeoVehicle _vehicle;
        private bool _following;
        private bool _savedMovement = true;
        private bool _savedRotation = true;

        public event System.Action<bool> FollowingChanged;

        public bool IsFollowing => _following;

        public void Bind(CesiumGlobeAnchor cameraAnchor, CesiumCameraController cameraController, GeoVehicle vehicle)
        {
            _cameraAnchor = cameraAnchor;
            _cameraController = cameraController;
            _vehicle = vehicle;
        }

        public void SetFollowing(bool follow)
        {
            if (_following == follow)
            {
                return;
            }

            _following = follow;
            if (_cameraController == null)
            {
                return;
            }

            if (follow)
            {
                _savedMovement = _cameraController.enableMovement;
                _savedRotation = _cameraController.enableRotation;
                _cameraController.enableMovement = false;
                _cameraController.enableRotation = false;
                ApplyFollowPose();
            }
            else
            {
                _cameraController.enableMovement = _savedMovement;
                _cameraController.enableRotation = _savedRotation;
            }

            Debug.Log("CameraFollow: " + (follow ? "ON" : "OFF"));
            if (FollowingChanged != null)
            {
                FollowingChanged(_following);
            }
        }

        private void LateUpdate()
        {
            if (_following)
            {
                ApplyFollowPose();
            }
        }

        private void ApplyFollowPose()
        {
            if (_cameraAnchor == null || _vehicle == null || _vehicle.Anchor == null)
            {
                return;
            }

            double3 vehicleLlh = _vehicle.Anchor.longitudeLatitudeHeight;
            quaternion vehicleRotation = _vehicle.Anchor.rotationEastUpNorth;
            float yaw = ((Quaternion)vehicleRotation).eulerAngles.y;
            _cameraAnchor.longitudeLatitudeHeight = new double3(
                vehicleLlh.x,
                vehicleLlh.y,
                vehicleLlh.z + followHeightMeters);
            _cameraAnchor.rotationEastUpNorth = Quaternion.Euler(followPitch, yaw, 0f);
        }
    }
}
