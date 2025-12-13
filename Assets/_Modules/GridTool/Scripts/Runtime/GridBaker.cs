using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GridTool
{
    // Note: This code assumes the existence of:
    // 1. GridLayer (from your first prompt)
    // 2. TerrainTypeSO (must contain a 'Priority' and 'IsRender' field)
    // 3. GridDataSO (must contain 'ClearCellDatas', 'SetCellSize', 'AddCellData', 'SortCellDatas' and a 'CellDatas' list)
    // 4. CellData (must contain 'GridPosition', 'TerrainType', 'IsWalkable', and the NEW 'LayerPriority' field)

    public class GridBaker : MonoBehaviour
    {
        // --- PUBLIC INSPECTOR FIELDS ---
        
        [Title("Map Layers")]
        [ListDrawerSettings(ShowFoldout = true, CustomAddFunction = nameof(CreateNewLayer))]
        public List<GridLayer> layers = new();

        [Required("Output Map Data must be assigned!")]
        [AssetsOnly]
        public GridDataSO outputMapData;
        
        private GridLayer CreateNewLayer()
        {
            return new GridLayer { LayerName = "New Layer" };
        }

        // --- PUBLIC EDITOR METHODS (Buttons) ---
#if UNITY_EDITOR
        
        [Button(ButtonSizes.Medium)]
        [GUIColor(0.8f, 0.8f, 0.3f)] // Utility color
        public void ResetAttributes()
        {
            foreach (GridLayer layer in layers)
            {
                if (layer.TerrainType != null)
                {
                    layer.Priority = layer.TerrainType.Priority;
                    layer.IsWalkable = layer.TerrainType.IsWalkable;
                }
            }
            Debug.Log("Layer priorities reset to their respective TerrainType priorities.");
        }
        
        [Button(ButtonSizes.Large)]
        [GUIColor(0.3f, 0.9f, 0.3f)] // Primary action color
        public void BakeGrid()
        {
            if (!ValidateBakeConditions())
            {
                return;
            }

            Debug.Log("Starting map bake process...");
            
            Dictionary<Vector2Int, CellData> cellDataDict = new();
            BoundsInt overallBounds = InitializeBake(cellDataDict);

            if (overallBounds.size.x == 0 || overallBounds.size.y == 0)
            {
                Debug.LogError("Bake Failed: Map is empty (overall bounds size is zero).");
                return;
            }
            
            ProcessAllLayers(cellDataDict, overallBounds);
            
            FinalizeBake(cellDataDict);
            
            Debug.Log($"Map baked successfully with {outputMapData.CellDatas.Count} cells.");
        }
        
        private bool ValidateBakeConditions()
        {
            if (outputMapData == null)
            {
                Debug.LogError("Bake Failed: Output Asset is not assigned.");
                return false;
            }

            if (layers.Count == 0)
            {
                Debug.LogError("Bake Failed: No layers assigned to bake.");
                return false;
            }
            return true;
        }

        private BoundsInt InitializeBake(Dictionary<Vector2Int, CellData> cellDataDict)
        {
            outputMapData.ClearCellDatas();
            outputMapData.SetCellSize(GetTilemapCellSize());
            
            // cellDataDict is already new, so no need to clear it.
            return CalculateOverallBounds();
        }

        private void ProcessAllLayers(Dictionary<Vector2Int, CellData> cellDataDict, BoundsInt overallBounds)
        {
            // Sort layers: 'b.Priority.CompareTo(a.Priority)' sorts in DESCENDING order.
            // This means layers with a HIGHER Priority value are processed first.
            layers.Sort((a, b) => b.Priority.CompareTo(a.Priority)); 

            foreach (GridLayer layer in layers)
            {
                if (!IsLayerValid(layer))
                {
                    continue;
                }
                
                ProcessLayer(layer, cellDataDict, overallBounds);
            }
            
            Debug.Log($"Non-walkable Cells found: {cellDataDict.Count(x => !x.Value.IsWalkable)}");
        }

        private void FinalizeBake(Dictionary<Vector2Int, CellData> cellDataDict)
        {
            // Batch add the generated cell data
            foreach (var kvp in cellDataDict)
            {
                outputMapData.AddCellData(kvp.Value);
            }

            outputMapData.SortCellDatas(); // Sort the final list
            
            SaveGridAsset();
        }

        // --- CORE PROCESSING & UTILITIES ---

        private bool IsLayerValid(GridLayer layer)
        {
            if (layer.TileMap == null)
            {
                Debug.LogWarning($"Skipping layer '{layer.LayerName}': TileMap is null.");
                return false;
            }
            if (layer.TerrainType == null)
            {
                Debug.LogWarning($"Skipping layer '{layer.LayerName}': TerrainType is null.");
                return false;
            }
            return true;
        }

        private void ProcessLayer(GridLayer layer, Dictionary<Vector2Int, CellData> cellDataDict, BoundsInt bounds)
        {
            // Optimization: Disable rendering for non-visual layers
            if (!layer.TerrainType.IsRender)
            {
                if (layer.TileMap.TryGetComponent<TilemapRenderer>(out var tr))
                {
                    tr.enabled = false;
                }
            }

            foreach (Vector3Int cellPosition in bounds.allPositionsWithin)
            {
                TileBase tile = layer.TileMap.GetTile(cellPosition);

                if (tile == null)
                {
                    continue;
                }

                Vector2Int cellPos = new Vector2Int(cellPosition.x, cellPosition.y);
                
                if (!cellDataDict.ContainsKey(cellPos))
                {
                    // Case 1: If cell does not exist, create it with current layer data.
                    cellDataDict[cellPos] = new CellData
                    {
                        GridPosition = cellPos,
                        TerrainType = layer.TerrainType,
                        IsWalkable = layer.IsWalkable,
                    };
                }
                else
                {
                    
                }
            }
        }

        private BoundsInt CalculateOverallBounds()
        {
            BoundsInt overallBounds = new();
            bool firstBounds = true;

            foreach (GridLayer layer in layers)
            {
                if (layer.TileMap == null) continue;

                BoundsInt layerBounds = layer.TileMap.cellBounds;
                if (layerBounds.size.x == 0 && layerBounds.size.y == 0) continue;
                
                if (firstBounds)
                {
                    overallBounds = layerBounds;
                    firstBounds = false;
                }
                else
                {
                    // Union the current overallBounds with the new layerBounds
                    overallBounds = UnionBoundsInt(overallBounds, layerBounds); 
                }
            }

            return overallBounds;
        }

        /// <summary>
        /// Combines two BoundsInt structures into a single encompassing BoundsInt.
        /// </summary>
        private BoundsInt UnionBoundsInt(BoundsInt a, BoundsInt b)
        {
            a.xMin = Math.Min(a.xMin, b.xMin);
            a.yMin = Math.Min(a.yMin, b.yMin);
            a.xMax = Math.Max(a.xMax, b.xMax);
            a.yMax = Math.Max(a.yMax, b.yMax);
            return a;
        }

        public void SetUp(List<GridLayer> layers, GridDataSO outputMapData)
        {
            Debug.Log("Setting up MapBaker...");
            this.layers = layers;
            this.outputMapData = outputMapData;
        }

        private void SaveGridAsset()
        {
            EditorUtility.SetDirty(outputMapData);
            AssetDatabase.SaveAssetIfDirty(outputMapData);
            AssetDatabase.Refresh();
        }
        
        private float GetTilemapCellSize()
        {
            foreach (GridLayer layer in layers)
            {
                if (layer.TileMap != null)
                {
                    return layer.TileMap.cellSize.x; 
                }
            }

            Debug.LogWarning("No valid tilemap found, using default cell size 1.0.");
            return 1f;
        }
#endif
    }
}