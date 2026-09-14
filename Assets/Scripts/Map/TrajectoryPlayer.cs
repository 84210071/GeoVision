using System;
using System.Globalization;
using CesiumForUnity;
using GeoVision.Data;
using Unity.Mathematics;
using UnityEngine;

namespace GeoVision.Map
{
    public class TrajectoryPlayer : MonoBehaviour
    {
        public event Action ProgressChanged;

        private TrajectoryPoint[] _points;
        private double[] _times;
        private CesiumGeoreference _georeference;
        private GeoVehicle _vehicle;
        private bool _playing;
        private double _currentTime;
        private float _playbackSpeed = 1f;
        private float _lastHeading;

        public bool IsPlaying => _playing;
        public float PlaybackSpeed => _playbackSpeed;
        public double CurrentTime => _currentTime;
        public double Duration { get; private set; }
        public double CurrentSpeed { get; private set; }
        public DateTime StartUtc { get; private set; }
        public GeoVehicle Vehicle => _vehicle;

        public float NormalizedProgress
        {
            get { return Duration <= 0.0 ? 0f : (float)math.clamp(_currentTime / Duration, 0.0, 1.0); }
        }

        public void Bind(CesiumGeoreference georeference, GeoVehicle vehicle, TrajectoryData data)
        {
            _georeference = georeference;
            _vehicle = vehicle;
            Load(data);
            Stop();
        }

        public void Play()
        {
            if (_points == null || _points.Length < 2)
            {
                return;
            }

            if (_currentTime >= Duration)
            {
                _currentTime = 0.0;
            }

            _playing = true;
        }

        public void Pause()
        {
            _playing = false;
        }

        public void Stop()
        {
            _playing = false;
            _currentTime = 0.0;
            ApplyPose();
            RaiseProgress();
        }

        public void SetProgress(float normalizedProgress)
        {
            if (Duration <= 0.0)
            {
                return;
            }

            _currentTime = math.clamp(normalizedProgress, 0.0, 1.0) * Duration;
            ApplyPose();
            RaiseProgress();
        }

        public void SetPlaybackSpeed(float speed)
        {
            _playbackSpeed = Mathf.Max(speed, 0.01f);
        }

        public string FormatCurrentTimestamp()
        {
            DateTime utc = StartUtc.AddSeconds(_currentTime);
            return utc.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private void Update()
        {
            if (!_playing || Duration <= 0.0)
            {
                return;
            }

            _currentTime += Time.deltaTime * _playbackSpeed;
            if (_currentTime >= Duration)
            {
                _currentTime = Duration;
                _playing = false;
            }

            ApplyPose();
            RaiseProgress();
        }

        private void Load(TrajectoryData data)
        {
            if (data == null || data.points == null || data.points.Length < 2)
            {
                Debug.LogError("TrajectoryPlayer: trajectory JSON needs at least 2 points.");
                _points = Array.Empty<TrajectoryPoint>();
                _times = Array.Empty<double>();
                Duration = 0.0;
                return;
            }

            _points = data.points;
            _times = new double[_points.Length];
            StartUtc = ParseTimestamp(_points[0].timestamp);
            for (int i = 0; i < _points.Length; i++)
            {
                DateTime utc = ParseTimestamp(_points[i].timestamp);
                _times[i] = (utc - StartUtc).TotalSeconds;
            }

            Duration = _times[_times.Length - 1];
            Debug.Log("TrajectoryPlayer: loaded " + _points.Length + " points, duration=" + Duration.ToString("F1") + "s");
        }

        private void ApplyPose()
        {
            if (_vehicle == null || _georeference == null || _points == null || _points.Length == 0)
            {
                return;
            }

            _georeference.Initialize();
            int index = FindSegment(_currentTime);
            TrajectoryPoint from = _points[index];
            TrajectoryPoint to = _points[math.min(index + 1, _points.Length - 1)];
            double span = _times[math.min(index + 1, _times.Length - 1)] - _times[index];
            double t = span > 0.0001 ? (_currentTime - _times[index]) / span : 0.0;
            t = math.clamp(t, 0.0, 1.0);

            double3 source = new double3(from.longitude, from.latitude, from.height);
            double3 destination = new double3(to.longitude, to.latitude, to.height);
            double3 llh = GeoGlobeUtil.InterpolateLlh(source, destination, t);

            double3 lookSource = source;
            double3 lookDest = destination;
            if (index + 1 >= _points.Length)
            {
                lookSource = new double3(_points[index - 1].longitude, _points[index - 1].latitude, _points[index - 1].height);
                lookDest = source;
            }

            float heading = GeoGlobeUtil.HeadingYawDegrees(lookSource.x, lookSource.y, lookDest.x, lookDest.y);
            if (math.abs(lookDest.x - lookSource.x) < 1e-8 && math.abs(lookDest.y - lookSource.y) < 1e-8)
            {
                heading = _lastHeading;
            }

            _lastHeading = heading;
            CurrentSpeed = math.lerp(from.speed, to.speed, t);
            _vehicle.SetGlobePose(llh, heading);
        }

        private int FindSegment(double time)
        {
            if (time <= _times[0])
            {
                return 0;
            }

            for (int i = 0; i < _times.Length - 1; i++)
            {
                if (time <= _times[i + 1])
                {
                    return i;
                }
            }

            return _points.Length - 1;
        }

        private void RaiseProgress()
        {
            if (ProgressChanged != null)
            {
                ProgressChanged();
            }
        }

        private static DateTime ParseTimestamp(string timestamp)
        {
            DateTime parsed;
            if (DateTime.TryParse(
                    timestamp,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out parsed))
            {
                return parsed;
            }

            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
    }
}
