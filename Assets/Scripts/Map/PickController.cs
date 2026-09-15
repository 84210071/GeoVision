using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GeoVision.Map
{
    public class PickController : MonoBehaviour
    {
        [SerializeField] private float maxPickDistance = 5000000f;

        private IGeoSelectable _current;
        private GeoInfoPanel _infoPanel;

        public event Action<IGeoSelectable> SelectionChanged;

        public IGeoSelectable Current => _current;

        public void Bind(GeoInfoPanel infoPanel)
        {
            if (_infoPanel != null)
            {
                _infoPanel.Closed -= OnPanelClosed;
            }

            _infoPanel = infoPanel;
            if (_infoPanel != null)
            {
                _infoPanel.Closed += OnPanelClosed;
            }
        }

        private void OnDestroy()
        {
            if (_infoPanel != null)
            {
                _infoPanel.Closed -= OnPanelClosed;
            }
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxPickDistance, ~0, QueryTriggerInteraction.Collide))
            {
                IGeoSelectable selectable = hit.collider.GetComponentInParent<IGeoSelectable>();
                if (selectable != null)
                {
                    Select(selectable);
                    return;
                }
            }

            ClearSelection();
        }

        public void Select(IGeoSelectable selectable)
        {
            if (selectable == null)
            {
                ClearSelection();
                return;
            }

            if (_current != null && !ReferenceEquals(_current, selectable))
            {
                _current.SetHighlighted(false);
            }

            _current = selectable;
            _current.SetHighlighted(true);
            Debug.Log(
                "Pick: id=" + selectable.Id +
                " name=" + selectable.DisplayName +
                " type=" + selectable.Type +
                " lon=" + selectable.Longitude.ToString("F6") +
                " lat=" + selectable.Latitude.ToString("F6") +
                " status=" + selectable.Status);

            if (_infoPanel != null)
            {
                _infoPanel.Show(selectable);
            }

            RaiseSelectionChanged();
        }

        public void ClearSelection()
        {
            if (_current != null)
            {
                _current.SetHighlighted(false);
                _current = null;
            }

            if (_infoPanel != null)
            {
                _infoPanel.Hide();
            }

            RaiseSelectionChanged();
        }

        private void OnPanelClosed()
        {
            if (_current != null)
            {
                _current.SetHighlighted(false);
                _current = null;
            }

            RaiseSelectionChanged();
        }

        private void RaiseSelectionChanged()
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(_current);
            }
        }
    }
}
