using Sirenix.OdinInspector;
using UnityEngine.Tilemaps;

namespace MapTool
{
    [System.Serializable]
    public class MapLayer
    {
        public string layerName;
        public Tilemap tileMap;
        public TerrainTypeSO terrainType;
    }
}