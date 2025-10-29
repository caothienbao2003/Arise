using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MapTool
{
    public class MapBaker : MonoBehaviour
    {
        [Title("Map Layers")]
        [ListDrawerSettings(ShowFoldout = true)]
        public List<MapLayer> layers = new();

        [Required("Output Map Data must be assigned!")]
        [AssetsOnly]
        public MapDataSO outputMapData;

#if UNITY_EDITOR
        [Button(ButtonSizes.Large), GUIColor(0.3f, 0.9f, 0.3f)]
        public void BakeMap()
        {
            Debug.Log(outputMapData);

            if (outputMapData == null)
            {
                Debug.LogError("Output Asset is not assigned.");
                return;
            }

            if (layers.Count == 0)
            {
                Debug.LogError("No layers assigned to bake.");
                return;
            }

            Debug.Log("Baking map...");

            outputMapData.cellDatas.Clear();

            Dictionary<Vector2Int, CellData> cellDataDict = new();
            BoundsInt overallBounds = CalculateOverallBounds();

            if (overallBounds.size.x == 0 || overallBounds.size.y == 0)
            {
                Debug.LogError("Map is empty, nothing to bake");
                return;
            }

            foreach (MapLayer layer in layers)
            {
                if (layer.tileMap == null)
                {
                    Debug.LogWarning($"Layer {layer.layerName} has no TileMap assigned. Skipping.");
                    continue;
                }

                if (layer.terrainType == null)
                {
                    Debug.LogWarning($"Layer {layer.layerName} has no TerrainType assigned. Skipping.");
                    continue;
                }

                ProcessLayer(layer, cellDataDict, overallBounds);

                foreach(var kvp in cellDataDict)
                {
                    outputMapData.cellDatas.Add(kvp.Value);
                }

                outputMapData.cellDatas.Sort((a, b) =>
                {
                    int compareY = b.CellPosition.y.CompareTo(a.CellPosition.y);
                    return compareY != 0 ? compareY : a.CellPosition.x.CompareTo(b.CellPosition.x);
                });

                EditorUtility.SetDirty(outputMapData);
                AssetDatabase.SaveAssets();

                Debug.Log("Map baked successfully");
            }
        }

        private void ProcessLayer(MapLayer layer, Dictionary<Vector2Int, CellData> cellDataDict, BoundsInt bounds)
        {
            int tilesProcessed = 0;

            foreach (Vector3Int cellPosition in bounds.allPositionsWithin)
            {
                TileBase tile = layer.tileMap.GetTile(cellPosition);

                if (tile == null)
                {
                    continue;
                }

                Vector2Int cellPos = new Vector2Int(cellPosition.x, cellPosition.y);
                if (!cellDataDict.ContainsKey(cellPos))
                {
                    //If not exist, create new CellData
                    cellDataDict[cellPos] = new CellData
                    {
                        CellPosition = cellPos,
                        WorldPosition = layer.tileMap.CellToWorld(cellPosition),
                        TerrainType = layer.terrainType
                    };
                }
                else
                {
                    //If exist, update its data
                    cellDataDict[cellPos].TerrainType = layer.terrainType;
                }

                tilesProcessed++;
            }
        }

        private BoundsInt CalculateOverallBounds()
        {
            bool firstBounds = true;
            BoundsInt overallBounds = new();

            foreach (MapLayer layer in layers)
            {
                if (layer.tileMap == null)
                {
                    continue;
                }

                BoundsInt layerBounds = layer.tileMap.cellBounds;
                if (layerBounds.size.x == 0 || layerBounds.size.y == 0)
                {
                    continue;
                }

                if (firstBounds)
                {
                    overallBounds = layerBounds;
                    firstBounds = false;
                }
                else
                {
                    overallBounds.xMin = Math.Min(overallBounds.xMin, layerBounds.xMin);
                    overallBounds.yMin = Math.Min(overallBounds.yMin, layerBounds.yMin);
                    overallBounds.xMax = Math.Max(overallBounds.xMax, layerBounds.xMax);
                    overallBounds.yMax = Math.Max(overallBounds.yMax, layerBounds.yMax);
                }
            }

            return overallBounds;
        }



        public void SetUp(List<MapLayer> layers, MapDataSO outputMapData)
        {
            Debug.Log("Setting up MapBaker... " + outputMapData);

            this.layers = layers;
            this.outputMapData = outputMapData;
        }
#endif
    }
}
