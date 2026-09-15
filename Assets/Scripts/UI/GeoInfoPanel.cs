using System;
using UnityEngine;
using UnityEngine.UI;

namespace GeoVision.Map
{
    public class GeoInfoPanel : MonoBehaviour
    {
        private const double LocateHeightMeters = 2200.0;

        public event Action Closed;

        [SerializeField] private GameObject root;
        [SerializeField] private Text nameText;
        [SerializeField] private Text typeText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text coordText;
        [SerializeField] private Text descText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button locateButton;

        private IGeoSelectable _current;
        private GeoFlyToController _flyTo;
        private bool _wired;

        private void Awake()
        {
            WireButtons();
            Hide();
        }

        public void Bind(GeoFlyToController flyTo)
        {
            _flyTo = flyTo;
            WireButtons();
        }

        public void Show(IGeoSelectable selectable)
        {
            _current = selectable;
            if (root != null)
            {
                root.SetActive(true);
            }

            if (selectable == null)
            {
                return;
            }

            Set(nameText, selectable.DisplayName);
            Set(typeText, FormatType(selectable.Type));
            Set(statusText, FormatStatus(selectable.Status));
            Set(coordText, selectable.Longitude.ToString("F6") + "  ,  " + selectable.Latitude.ToString("F6"));
            Set(descText, string.IsNullOrEmpty(selectable.Description) ? "暂无描述" : selectable.Description);
        }

        public void Hide()
        {
            _current = null;
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void WireButtons()
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (locateButton != null)
            {
                locateButton.onClick.AddListener(OnLocateClicked);
            }
        }

        private void OnCloseClicked()
        {
            Hide();
            if (Closed != null)
            {
                Closed();
            }
        }

        private void OnLocateClicked()
        {
            if (_flyTo == null || _current == null)
            {
                return;
            }

            _flyTo.FlyToSelectable(_current, LocateHeightMeters);
        }

        private static void Set(Text label, string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }

        private static string FormatType(string type)
        {
            switch (type)
            {
                case "project":
                    return "项目";
                case "camera":
                    return "摄像头";
                case "vehicle":
                    return "车辆";
                case "alert":
                    return "告警点";
                case "monitor":
                    return "监测点";
                default:
                    return type;
            }
        }

        private static string FormatStatus(string status)
        {
            switch (status)
            {
                case "normal":
                    return "正常";
                case "warning":
                    return "提醒";
                case "alarm":
                    return "告警";
                case "offline":
                    return "离线";
                default:
                    return status;
            }
        }
    }
}
