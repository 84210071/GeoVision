using UnityEngine;
using UnityEngine.UI;

namespace GeoVision.Map
{
    public class AlarmItemView : MonoBehaviour
    {
        [SerializeField] private Text timeText;
        [SerializeField] private Text typeText;
        [SerializeField] private Text stateText;
        [SerializeField] private Image statusMark;

        public void Set(string time, string type, string state, Color color)
        {
            if (timeText != null)
            {
                timeText.text = time;
            }

            if (typeText != null)
            {
                typeText.text = type;
            }

            if (stateText != null)
            {
                stateText.text = state;
                stateText.color = color;
            }

            if (statusMark != null)
            {
                statusMark.color = color;
            }
        }
    }
}
