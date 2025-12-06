using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GridTool
{
    public class GridBaker : MonoBehaviour
    {
        [Title("Map Layers")]
        [ListDrawerSettings(ShowFoldout = true)]
        public List<GridLayer> layers = new();

        [Required("Output Map Data must be assigned!")]
        [AssetsOnly]
        public GridDataSO outputMapData;

#if UNITY_EDITOR
        [Button(ButtonSizes.Large), GUIColor(0.3f, 0.9f, 0.3f)]
        public void BakeGrid()
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

            // CHANGED: Use method instead of property
            outputMapData.ClearCellDatas();
    
            float cellSize = GetTilemapCellSize();
            outputMapData.SetCellSize(cellSize);

            Dictionary<Vector2Int, CellData> cellDataDict = new();
            BoundsInt overallBounds = CalculateOverallBounds();

            if (overallBounds.size.x == 0 || overallBounds.size.y == 0)
            {
                Debug.LogError("Map is empty, nothing to bake");
                return;
            }

            foreach (GridLayer layer in layers)
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
            }

            // CHANGED: Use method to add data
            foreach (var kvp in cellDataDict)
            {
                outputMapData.AddCellData(kvp.Value);
            }

            // CHANGED: Use method to sort
            outputMapData.SortCellDatas();

            // Force Unity to recognize changes
            EditorUtility.SetDirty(outputMapData);
            AssetDatabase.SaveAssetIfDirty(outputMapData); // CHANGED: Use SaveAssetIfDirty
            AssetDatabase.Refresh();

            Debug.Log($"Map baked successfully with {outputMapData.CellDatas.Count} cells");
        }

        private void ProcessLayer(GridLayer layer, Dictionary<Vector2Int, CellData> cellDataDict, BoundsInt bounds)
        {
            int tilesProcessed = 0;

            if(layer.tileMap != null && !layer.terrainType.IsRender)
            {
                TilemapRenderer tr = layer.tileMap.GetComponent<TilemapRenderer>();
                if(tr != null)
                {
                    tr.enabled = false;
                }   
            }

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
                        NodePosition = cellPos,
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

        //Create a surround bounds that contains all layers
        private BoundsInt CalculateOverallBounds()
        {
            bool firstBounds = true;
            BoundsInt overallBounds = new();

            foreach (GridLayer layer in layers)
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



        public void SetUp(List<GridLayer> layers, GridDataSO outputMapData)
        {
            Debug.Log("Setting up MapBaker... " + outputMapData);

            this.layers = layers;
            this.outputMapData = outputMapData;
        }
        
        private float GetTilemapCellSize()
        {
            foreach (GridLayer layer in layers)
            {
                if (layer.tileMap != null)
                {
                    // Get cell size from tilemap
                    return layer.tileMap.cellSize.x; // Assuming square cells
                }
            }
    
            Debug.LogWarning("No valid tilemap found, using default cell size 1.0");
            return 1f;
        }
#endif
    }
}
