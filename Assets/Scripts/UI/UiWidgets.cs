using UnityEngine;

namespace GeoVision.Map
{
    public static class UiWidgets
    {
        public static readonly Color PanelBg = new Color(5f / 255f, 25f / 255f, 45f / 255f, 210f / 255f);
        public static readonly Color PanelInner = new Color(10f / 255f, 45f / 255f, 70f / 255f, 150f / 255f);
        public static readonly Color HeaderBg = new Color(4f / 255f, 16f / 255f, 32f / 255f, 200f / 255f);
        public static readonly Color Border = new Color(0.22f, 0.78f, 0.95f, 0.28f);
        public static readonly Color Accent = new Color(0.35f, 0.86f, 0.98f, 1f);
        public static readonly Color Title = new Color(0.93f, 0.97f, 1f, 1f);
        public static readonly Color Muted = new Color(0.55f, 0.70f, 0.80f, 0.72f);
        public static readonly Color Label = new Color(0.52f, 0.66f, 0.76f, 0.88f);
        public static readonly Color Number = new Color(0.90f, 0.96f, 1f, 1f);
        public static readonly Color Danger = new Color(1f, 0.32f, 0.36f, 1f);
        public static readonly Color Warning = new Color(1f, 0.62f, 0.18f, 1f);
        public static readonly Color Ok = new Color(0.22f, 0.92f, 0.62f, 1f);
        public static readonly Color ButtonBg = new Color(0.05f, 0.16f, 0.28f, 0.92f);
        public static readonly Color BarTrack = new Color(0.08f, 0.16f, 0.24f, 0.85f);

        private static Font _font;

        public static Font DefaultFont()
        {
            if (_font != null)
            {
                return _font;
            }

            _font = Font.CreateDynamicFontFromOSFont(
                new[] { "Microsoft YaHei", "微软雅黑", "SimHei", "PingFang SC", "Arial Unicode MS", "Arial" },
                16);
            if (_font == null)
            {
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            if (_font == null)
            {
                _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return _font;
        }
    }
}
