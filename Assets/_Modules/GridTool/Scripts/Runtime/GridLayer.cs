using Sirenix.OdinInspector;
using UnityEngine.Tilemaps;

namespace GridTool
{
    [System.Serializable]
    public class GridLayer
    {
        public string layerName;
        public Tilemap tileMap;
        public TerrainTypeSO terrainType;
    }
}