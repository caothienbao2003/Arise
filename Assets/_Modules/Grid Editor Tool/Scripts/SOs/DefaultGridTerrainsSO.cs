using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace GridTool
{
    [CreateAssetMenu(menuName = "GridTool/Default Grid Terrains", fileName =  "Default Grid Terrains")]
    public class DefaultGridTerrainsSO: ScriptableObject
    {
        public List<TerrainTypeSO> DefaultTerrainTypes = new List<TerrainTypeSO>();
    }
}