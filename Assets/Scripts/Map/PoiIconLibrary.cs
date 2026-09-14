using System.Collections.Generic;
using UnityEngine;

namespace GeoVision.Map
{
    public static class PoiIconLibrary
    {
        private static readonly Dictionary<string, Texture2D> GeneratedPins = new Dictionary<string, Texture2D>();

        public static Texture2D GetIcon(string type)
        {
            string key = string.IsNullOrEmpty(type) ? "project" : type.ToLowerInvariant();
            Texture2D texture = Resources.Load<Texture2D>("PoiIcons/" + key);
            if (texture != null)
            {
                return texture;
            }

            return GetOrCreatePin(key, GetTypeColor(key));
        }

        public static Color GetTypeColor(string type)
        {
            switch (type)
            {
                case "camera":
                    return new Color(0.15f, 0.85f, 0.95f, 1f);
                case "alert":
                    return new Color(1f, 0.18f, 0.18f, 1f);
                case "monitor":
                    return new Color(0.2f, 0.9f, 0.35f, 1f);
                case "vehicle":
                    return new Color(0.1f, 0.45f, 1f, 1f);
                default:
                    return new Color(0.2f, 0.55f, 1f, 1f);
            }
        }

        public static Color GetStatusTint(string status)
        {
            switch (status)
            {
                case "alarm":
                    return new Color(1f, 0.55f, 0.55f, 1f);
                case "warning":
                    return new Color(1f, 0.85f, 0.4f, 1f);
                case "offline":
                    return new Color(0.72f, 0.74f, 0.78f, 1f);
                default:
                    return Color.white;
            }
        }

        private static Texture2D GetOrCreatePin(string key, Color color)
        {
            Texture2D cached;
            if (GeneratedPins.TryGetValue(key, out cached) && cached != null)
            {
                return cached;
            }

            const int size = 64;
            Texture2D pin = new Texture2D(size, size, TextureFormat.RGBA32, false);
            pin.name = "RuntimePin_" + key;
            pin.filterMode = FilterMode.Bilinear;
            pin.wrapMode = TextureWrapMode.Clamp;

            Color clear = new Color(0f, 0f, 0f, 0f);
            Vector2 head = new Vector2(31.5f, 40f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y);
                    float headDist = Vector2.Distance(p, head);
                    bool inHead = headDist <= 16.5f;
                    bool inHole = headDist <= 6.5f;
                    bool inTip = y < 28 && Mathf.Abs(x - 31.5f) < (28 - y) * 0.42f && y > 6;
                    if (inHole)
                    {
                        pin.SetPixel(x, y, Color.white);
                    }
                    else if (inHead || inTip)
                    {
                        pin.SetPixel(x, y, color);
                    }
                    else
                    {
                        pin.SetPixel(x, y, clear);
                    }
                }
            }

            pin.Apply(false, true);
            GeneratedPins[key] = pin;
            return pin;
        }
    }
}
