using UnityEngine;
using UnityEngine.UI;

namespace GeoVision.Map
{
    public class GeoInfoPanel : MonoBehaviour
    {
        private const double LocateHeightMeters = 2200.0;

        private GameObject _root;
        private Text _body;
        private IGeoSelectable _current;
        private GeoFlyToController _flyTo;

        public void Bind(GeoFlyToController flyTo)
        {
            _flyTo = flyTo;
        }

        public void Build(Transform canvas)
        {
            _root = UiWidgets.CreatePanel(canvas, "PoiInfoPanel", new Vector2(-24f, -24f), new Vector2(360f, 280f), TextAnchor.UpperRight);
            UiWidgets.CreateLabel(_root.transform, "POI 信息", 18, FontStyle.Bold, new Vector2(16f, -12f), new Vector2(240f, 28f));
            _body = UiWidgets.CreateLabel(_root.transform, "", 15, FontStyle.Normal, new Vector2(16f, -44f), new Vector2(328f, 170f));
            _body.alignment = TextAnchor.UpperLeft;
            Button locate = UiWidgets.CreateButton(_root.transform, "定位", new Vector2(16f, 16f), new Vector2(120f, 36f), new Color(0.15f, 0.55f, 0.95f, 1f));
            locate.onClick.AddListener(OnLocateClicked);
            Hide();
        }

        public void Show(IGeoSelectable selectable)
        {
            _current = selectable;
            if (_root != null)
            {
                _root.SetActive(true);
            }

            if (_body == null || selectable == null)
            {
                return;
            }

            _body.text =
                "id: " + selectable.Id + "\n" +
                "name: " + selectable.DisplayName + "\n" +
                "type: " + selectable.Type + "\n" +
                "status: " + selectable.Status + "\n" +
                "lon: " + selectable.Longitude.ToString("F6") + "\n" +
                "lat: " + selectable.Latitude.ToString("F6") + "\n" +
                "height: " + selectable.Height.ToString("F1") + " m\n" +
                selectable.Description;
        }

        public void Hide()
        {
            _current = null;
            if (_root != null)
            {
                _root.SetActive(false);
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
    }
}
