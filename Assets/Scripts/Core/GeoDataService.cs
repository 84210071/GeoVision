using UnityEngine;

namespace GeoVision.Core
{
    /// <summary>
    /// Loads Mock JSON from Resources. All GIS demo data is local and fictional.
    /// </summary>
    public class GeoDataService
    {
        public T LoadJson<T>(string resourcesPath)
        {
            TextAsset asset = Resources.Load<TextAsset>(resourcesPath);
            if (asset == null)
            {
                Debug.LogError("GeoDataService: missing Resources/" + resourcesPath);
                return default;
            }

            return JsonUtility.FromJson<T>(asset.text);
        }
    }
}
