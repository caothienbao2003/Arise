using FreelancerNecromancer;
using System;
using UnityEngine;

namespace MapTool
{
    [Serializable]
    public class CellData
    {
        public Vector2Int CellPosition { get; set; }
        public Vector3 WorldPosition { get; set; }
        public TerrainTypeSO TerrainType { get; set; }
        public Sprite TileSprite { get; set; }
    }
}