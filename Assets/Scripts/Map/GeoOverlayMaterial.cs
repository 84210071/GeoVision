using UnityEngine;

namespace GeoVision.Map
{
    public static class GeoOverlayMaterial
    {
        private static Shader _overlayShader;

        public static Material Create(Color color, Texture texture)
        {
            Shader shader = GetOverlayShader();
            Material material = new Material(shader);
            Texture main = texture != null ? texture : Texture2D.whiteTexture;
            material.SetTexture("_MainTex", main);
            material.SetColor("_Color", color);
            material.mainTexture = main;
            material.color = color;
            material.renderQueue = 5000;
            material.enableInstancing = false;
            return material;
        }

        private static Shader GetOverlayShader()
        {
            if (_overlayShader != null)
            {
                return _overlayShader;
            }

            _overlayShader = Shader.Find("GeoVision/OverlayUnlit");
            if (_overlayShader == null)
            {
                _overlayShader = Shader.Find("Universal Render Pipeline/Unlit");
                Debug.LogWarning("GeoOverlayMaterial: OverlayUnlit missing, falling back to URP Unlit.");
            }

            if (_overlayShader == null)
            {
                _overlayShader = Shader.Find("Sprites/Default");
            }

            return _overlayShader;
        }
    }
}
