using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GeoVision.Map
{
    public static class UiWidgets
    {
        public static Font DefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return font;
        }

        public static Canvas CreateHudCanvas()
        {
            GameObject canvasObject = new GameObject("GisHud");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.AddComponent<GraphicRaycaster>();

            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventObject = new GameObject("EventSystem");
                eventObject.AddComponent<EventSystem>();
                eventObject.AddComponent<StandaloneInputModule>();
            }

            return canvas;
        }

        public static GameObject CreatePanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, TextAnchor corner)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.05f, 0.08f, 0.12f, 0.82f);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            ApplyCorner(rect, corner, anchoredPosition);
            return panel;
        }

        public static Text CreateLabel(Transform parent, string text, int fontSize, FontStyle style, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(parent, false);
            Text label = labelObject.AddComponent<Text>();
            label.font = DefaultFont();
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.color = Color.white;
            label.text = text;
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return label;
        }

        public static Button CreateButton(Transform parent, string text, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject buttonObject = new GameObject(text + "Button");
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = color;
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            GameObject labelObject = new GameObject("Text");
            labelObject.transform.SetParent(buttonObject.transform, false);
            Text label = labelObject.AddComponent<Text>();
            label.font = DefaultFont();
            label.fontSize = 16;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.text = text;
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            return button;
        }

        public static Slider CreateSlider(Transform parent, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject sliderObject = new GameObject("Slider");
            sliderObject.transform.SetParent(parent, false);
            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            RectTransform rect = sliderObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            GameObject background = new GameObject("Background");
            background.transform.SetParent(sliderObject.transform, false);
            Image bg = background.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.25f, 0.3f, 1f);
            Stretch(background.GetComponent<RectTransform>());

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObject.transform, false);
            Stretch(fillArea.GetComponent<RectTransform>());
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.7f, 1f, 1f);
            Stretch(fill.GetComponent<RectTransform>());

            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObject.transform, false);
            Stretch(handleArea.GetComponent<RectTransform>());
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(16f, 0f);

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        public static Toggle CreateToggle(Transform parent, string text, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject toggleObject = new GameObject(text + "Toggle");
            toggleObject.transform.SetParent(parent, false);
            Toggle toggle = toggleObject.AddComponent<Toggle>();
            RectTransform rect = toggleObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            GameObject box = new GameObject("Background");
            box.transform.SetParent(toggleObject.transform, false);
            Image boxImage = box.AddComponent<Image>();
            boxImage.color = new Color(0.2f, 0.25f, 0.32f, 1f);
            RectTransform boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0f, 0.5f);
            boxRect.anchorMax = new Vector2(0f, 0.5f);
            boxRect.pivot = new Vector2(0f, 0.5f);
            boxRect.anchoredPosition = new Vector2(4f, 0f);
            boxRect.sizeDelta = new Vector2(22f, 22f);

            GameObject check = new GameObject("Checkmark");
            check.transform.SetParent(box.transform, false);
            Image checkImage = check.AddComponent<Image>();
            checkImage.color = new Color(0.2f, 0.85f, 0.45f, 1f);
            Stretch(check.GetComponent<RectTransform>());

            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(toggleObject.transform, false);
            Text label = labelObject.AddComponent<Text>();
            label.font = DefaultFont();
            label.fontSize = 15;
            label.color = Color.white;
            label.text = text;
            label.alignment = TextAnchor.MiddleLeft;
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(32f, 0f);
            labelRect.offsetMax = Vector2.zero;

            toggle.graphic = checkImage;
            toggle.targetGraphic = boxImage;
            toggle.isOn = false;
            return toggle;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void ApplyCorner(RectTransform rect, TextAnchor corner, Vector2 anchoredPosition)
        {
            if (corner == TextAnchor.UpperRight)
            {
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
            }
            else if (corner == TextAnchor.LowerCenter)
            {
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
            }
            else
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
            }

            rect.anchoredPosition = anchoredPosition;
        }
    }
}
