using System.Collections.Generic;
using CesiumForUnity;
using GeoVision.Core;
using GeoVision.Data;
using UnityEngine;

namespace GeoVision.Map
{
    public class PoiSpawner : MonoBehaviour
    {
        private const string PoiResourcePath = "Data/pois";

        [SerializeField] private Transform poiRoot;

        public List<GeoPoi> SpawnedPois { get; } = new List<GeoPoi>();

        public int SpawnedCount => SpawnedPois.Count;

        public void Spawn(GeoDataService dataService)
        {
            if (poiRoot == null)
            {
                GameObject layer = new GameObject("PoiLayer");
                layer.transform.SetParent(transform, false);
                poiRoot = layer.transform;
            }

            ClearExisting();

            GeoPointCollection collection = dataService.LoadJson<GeoPointCollection>(PoiResourcePath);
            if (collection == null || collection.items == null)
            {
                Debug.LogError("PoiSpawner: failed to load mock POI JSON.");
                return;
            }

            CesiumGeoreference georeference = GetComponentInParent<CesiumGeoreference>();
            if (georeference == null)
            {
                Debug.LogError("PoiSpawner: must be nested under CesiumGeoreference.");
                return;
            }

            georeference.Initialize();

            foreach (GeoPointData point in collection.items)
            {
                GameObject poiObject = new GameObject("POI_" + point.id);
                poiObject.transform.SetParent(poiRoot, false);
                GeoPoi poi = poiObject.AddComponent<GeoPoi>();
                poi.Bind(point);
                SpawnedPois.Add(poi);
            }

            Debug.Log("PoiSpawner: created " + SpawnedCount + " POIs from mock JSON.");
        }

        private void ClearExisting()
        {
            SpawnedPois.Clear();
            if (poiRoot == null)
            {
                return;
            }

            for (int i = poiRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(poiRoot.GetChild(i).gameObject);
            }
        }
    }
}
