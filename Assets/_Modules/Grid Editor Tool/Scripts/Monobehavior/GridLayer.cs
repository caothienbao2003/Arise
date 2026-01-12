using Sirenix.OdinInspector;
using UnityEngine.Tilemaps;

namespace GridTool
{
    [System.Serializable]
    public class GridLayer
    {
        public string LayerName;
        public Tilemap TileMap;

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Foldout)]
        public TerrainTypeSO TerrainType;

        public int Priority;
        public bool IsWalkable;
    }
}