using UnityEngine;
using UnityEngine.UI;

namespace GeoVision.Map
{
    public class GisHudFontApplier : MonoBehaviour
    {
        private void Awake()
        {
            Font font = UiWidgets.DefaultFont();
            Text[] labels = GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] != null)
                {
                    labels[i].font = font;
                }
            }
        }
    }
}
