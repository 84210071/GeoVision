using UnityEngine;
using UnityEngine.UI;

namespace GeoVision.Map
{
    public class TrajectoryPanel : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Button speed1Button;
        [SerializeField] private Button speed2Button;
        [SerializeField] private Button speed4Button;
        [SerializeField] private Toggle followToggle;
        [SerializeField] private Text timeText;
        [SerializeField] private Text speedText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Color speedIdleColor = new Color(0.05f, 0.16f, 0.28f, 0.92f);
        [SerializeField] private Color speedActiveColor = new Color(0.12f, 0.42f, 0.58f, 0.95f);

        private TrajectoryPlayer _player;
        private CameraFollowController _follow;
        private bool _wired;
        private bool _suppressSlider;

        public void Bind(TrajectoryPlayer player, CameraFollowController follow)
        {
            _player = player;
            _follow = follow;
            if (_player != null)
            {
                _player.ProgressChanged -= Refresh;
                _player.ProgressChanged += Refresh;
            }

            if (_follow != null)
            {
                _follow.FollowingChanged -= OnFollowExternalChanged;
                _follow.FollowingChanged += OnFollowExternalChanged;
            }

            WireEvents();
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

        private void WireEvents()
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
            if (playButton != null)
            {
                playButton.onClick.AddListener(() => { if (_player != null) { _player.Play(); } });
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(() => { if (_player != null) { _player.Pause(); } });
            }

            if (stopButton != null)
            {
                stopButton.onClick.AddListener(() => { if (_player != null) { _player.Stop(); } });
            }

            if (speed1Button != null)
            {
                speed1Button.onClick.AddListener(() => SetSpeed(1f));
            }

            if (speed2Button != null)
            {
                speed2Button.onClick.AddListener(() => SetSpeed(2f));
            }

            if (speed4Button != null)
            {
                speed4Button.onClick.AddListener(() => SetSpeed(4f));
            }

            if (followToggle != null)
            {
                followToggle.onValueChanged.AddListener(OnFollowChanged);
            }

            if (progressBar != null)
            {
                progressBar.onValueChanged.AddListener(OnSliderChanged);
            }
        }

        private void SetSpeed(float speed)
        {
            if (_player != null)
            {
                _player.SetPlaybackSpeed(speed);
                Refresh();
            }
        }

        private void OnFollowExternalChanged(bool on)
        {
            if (followToggle != null)
            {
                followToggle.SetIsOnWithoutNotify(on);
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
            if (progressBar != null)
            {
                progressBar.value = _player.NormalizedProgress;
            }

            _suppressSlider = false;
            if (timeText != null)
            {
                timeText.text = "时间  " + _player.FormatCurrentTimestamp();
            }

            if (speedText != null)
            {
                speedText.text = "速度  " + (_player.CurrentSpeed * 3.6).ToString("F1") + " km/h";
            }

            Highlight(speed1Button, Mathf.Abs(_player.PlaybackSpeed - 1f) < 0.01f);
            Highlight(speed2Button, Mathf.Abs(_player.PlaybackSpeed - 2f) < 0.01f);
            Highlight(speed4Button, Mathf.Abs(_player.PlaybackSpeed - 4f) < 0.01f);
        }

        private void Highlight(Button button, bool active)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = active ? speedActiveColor : speedIdleColor;
            }
        }
    }
}
