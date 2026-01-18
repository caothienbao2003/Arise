using System.Collections.Generic;
using _Modules.Grid_Editor_Tool.Scripts.Utils;
using NUnit.Framework;
using UnityEngine;

namespace GridTool
{
    [CreateAssetMenu(menuName = "GridTool/Default Grid Terrains", fileName =  "Default Grid Terrains")]
    public class DefaultGridTerrainsSO: ScriptableObjectWithActions
    {
        public List<TerrainTypeSO> DefaultTerrainTypes = new List<TerrainTypeSO>();
    }
}