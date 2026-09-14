using UnityEngine;
using UnityEngine.UI;

namespace GeoVision.Map
{
    public class TrajectoryPanel : MonoBehaviour
    {
        private TrajectoryPlayer _player;
        private CameraFollowController _follow;
        private Slider _slider;
        private Text _timeLabel;
        private Text _speedLabel;
        private Toggle _followToggle;
        private bool _suppressSlider;

        public void Bind(TrajectoryPlayer player, CameraFollowController follow)
        {
            _player = player;
            _follow = follow;
            if (_player != null)
            {
                _player.ProgressChanged += Refresh;
            }
        }

        public void Build(Transform canvas)
        {
            GameObject root = UiWidgets.CreatePanel(canvas, "TrajectoryPanel", new Vector2(0f, 18f), new Vector2(920f, 92f), TextAnchor.LowerCenter);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 18f);

            Button play = UiWidgets.CreateButton(root.transform, "Play", new Vector2(16f, 44f), new Vector2(72f, 32f), new Color(0.18f, 0.7f, 0.32f, 1f));
            Button pause = UiWidgets.CreateButton(root.transform, "Pause", new Vector2(96f, 44f), new Vector2(72f, 32f), new Color(0.75f, 0.55f, 0.12f, 1f));
            Button stop = UiWidgets.CreateButton(root.transform, "Stop", new Vector2(176f, 44f), new Vector2(72f, 32f), new Color(0.75f, 0.22f, 0.18f, 1f));
            play.onClick.AddListener(() => { if (_player != null) { _player.Play(); } });
            pause.onClick.AddListener(() => { if (_player != null) { _player.Pause(); } });
            stop.onClick.AddListener(() => { if (_player != null) { _player.Stop(); } });

            Button x1 = UiWidgets.CreateButton(root.transform, "1x", new Vector2(264f, 44f), new Vector2(48f, 32f), new Color(0.25f, 0.35f, 0.5f, 1f));
            Button x2 = UiWidgets.CreateButton(root.transform, "2x", new Vector2(318f, 44f), new Vector2(48f, 32f), new Color(0.25f, 0.35f, 0.5f, 1f));
            Button x4 = UiWidgets.CreateButton(root.transform, "4x", new Vector2(372f, 44f), new Vector2(48f, 32f), new Color(0.25f, 0.35f, 0.5f, 1f));
            x1.onClick.AddListener(() => { if (_player != null) { _player.SetPlaybackSpeed(1f); Refresh(); } });
            x2.onClick.AddListener(() => { if (_player != null) { _player.SetPlaybackSpeed(2f); Refresh(); } });
            x4.onClick.AddListener(() => { if (_player != null) { _player.SetPlaybackSpeed(4f); Refresh(); } });

            _followToggle = UiWidgets.CreateToggle(root.transform, "Follow Vehicle", new Vector2(440f, 44f), new Vector2(200f, 32f));
            _followToggle.onValueChanged.AddListener(OnFollowChanged);
            if (_follow != null)
            {
                _follow.FollowingChanged += OnFollowExternalChanged;
            }

            _timeLabel = UiWidgets.CreateLabel(root.transform, "Time --:--:--", 14, FontStyle.Bold, new Vector2(650f, -12f), new Vector2(140f, 24f));
            _speedLabel = UiWidgets.CreateLabel(root.transform, "Speed -- km/h", 14, FontStyle.Bold, new Vector2(790f, -12f), new Vector2(120f, 24f));

            _slider = UiWidgets.CreateSlider(root.transform, new Vector2(16f, 10f), new Vector2(888f, 24f));
            _slider.onValueChanged.AddListener(OnSliderChanged);
            Refresh();
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.ProgressChanged -= Refresh;
            }

            if (_follow != null)
            {
                _follow.FollowingChanged -= OnFollowExternalChanged;
            }
        }

        private void OnFollowExternalChanged(bool on)
        {
            if (_followToggle != null)
            {
                _followToggle.SetIsOnWithoutNotify(on);
            }
        }

        private void OnFollowChanged(bool on)
        {
            if (_follow != null)
            {
                _follow.SetFollowing(on);
            }
        }

        private void OnSliderChanged(float value)
        {
            if (_suppressSlider || _player == null)
            {
                return;
            }

            _player.SetProgress(value);
        }

        private void Refresh()
        {
            if (_player == null)
            {
                return;
            }

            _suppressSlider = true;
            if (_slider != null)
            {
                _slider.value = _player.NormalizedProgress;
            }

            _suppressSlider = false;
            if (_timeLabel != null)
            {
                _timeLabel.text = "Time " + _player.FormatCurrentTimestamp();
            }

            if (_speedLabel != null)
            {
                _speedLabel.text = "Speed " + (_player.CurrentSpeed * 3.6).ToString("F1") + " km/h  " + _player.PlaybackSpeed.ToString("0") + "x";
            }
        }
    }
}
