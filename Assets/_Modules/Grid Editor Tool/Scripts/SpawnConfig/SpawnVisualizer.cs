#if UNITY_EDITOR
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

namespace GridTool
{
    public class SpawnVisualizer : MonoBehaviour
    {
        [Title("Spawn Data")]
        [SerializeField, AssetsOnly, Required]
        public SpawnDataSO SpawnDataSO;
        
        [Title("Grid Data")]
        [SerializeField, AssetsOnly, Required]
        public GridDataSO GridDataSO;

        [Title("Visualization Settings")]
        [SerializeField] private bool showFixedSpawns = true;
        [SerializeField] private bool showWaveSpawns = true;
        [SerializeField] private bool showSummonZone = true;
        [SerializeField] private bool showLabels = true;

        [SerializeField] private Color summonZoneColor = new Color(0.3f, 0.5f, 1f, 0.3f);
        
        [SerializeField, Range(0.5f, 2f)] private float iconSize = 1f;
        [SerializeField] private int labelFontSize = 12;

        [Title("Wave Filter")]
        [SerializeField] private bool filterByWave = false;
        [SerializeField, ShowIf(nameof(filterByWave))] private int waveIndexToShow = 1;

        private void OnDrawGizmos()
        {
            if (SpawnDataSO == null || GridDataSO == null)
                return;

            if (showSummonZone)
                DrawSummonZone();

            if (showFixedSpawns)
                DrawFixedSpawns();

            if (showWaveSpawns)
                DrawWaveSpawns();
        }

        private void DrawSummonZone()
        {
            if (SpawnDataSO.SummonZone?.Positions == null)
                return;

            Gizmos.color = summonZoneColor;
            
            foreach (var pos in SpawnDataSO.SummonZone.Positions)
            {
                Vector3 worldPos = GetWorldPosition(pos);
                float cellSize = GridDataSO.CellSize;
                
                Gizmos.DrawCube(worldPos, new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.01f));
                
                Gizmos.color = new Color(summonZoneColor.r, summonZoneColor.g, summonZoneColor.b, 1f);
                Gizmos.DrawWireCube(worldPos, new Vector3(cellSize, cellSize, 0.01f));
                Gizmos.color = summonZoneColor;

                if (showLabels)
                {
                    DrawLabel(worldPos, "S", Color.cyan);
                }
            }
        }

        private void DrawFixedSpawns()
        {
            if (SpawnDataSO.FixedSpawns == null)
                return;

            foreach (var spawn in SpawnDataSO.FixedSpawns)
            {
                if (spawn.SpawnType == null)
                    continue;

                Vector3 worldPos = GetWorldPosition(spawn.Position);
                Color gizmoColor = spawn.SpawnType.GizmoColor;

                DrawSpawnIcon(worldPos, gizmoColor);

                if (showLabels)
                {
                    string iconText = spawn.SpawnType.IconText;
                    DrawLabel(worldPos, iconText, gizmoColor);
                }
            }
        }

        private void DrawWaveSpawns()
        {
            if (SpawnDataSO.WaveSpawns == null)
                return;

            foreach (var wave in SpawnDataSO.WaveSpawns)
            {
                if (filterByWave && wave.WaveIndex != waveIndexToShow)
                    continue;

                if (wave.SpawnEntries == null)
                    continue;

                foreach (var entry in wave.SpawnEntries)
                {
                    if (entry.SpawnType == null)
                        continue;

                    Vector3 worldPos = GetWorldPosition(entry.GridPosition);
                    Color gizmoColor = entry.SpawnType.GizmoColor;

                    worldPos += new Vector3(0, 0, -0.1f);

                    DrawSpawnIcon(worldPos, gizmoColor);

                    if (showLabels)
                    {
                        string iconText = entry.SpawnType.IconText;
                        string label = $"W{wave.WaveIndex}:{iconText}";
                        DrawLabel(worldPos, label, gizmoColor);
                    }
                }
            }
        }

        private void DrawSpawnIcon(Vector3 worldPos, Color color)
        {
            float cellSize = GridDataSO.CellSize;
            float size = cellSize * 0.5f * iconSize;

            Gizmos.color = color;
            Gizmos.DrawSphere(worldPos, size * 0.3f);
            
            Vector3 top = worldPos + Vector3.up * size;
            Vector3 bottom = worldPos + Vector3.down * size;
            Vector3 left = worldPos + Vector3.left * size;
            Vector3 right = worldPos + Vector3.right * size;

            Gizmos.DrawLine(top, right);
            Gizmos.DrawLine(right, bottom);
            Gizmos.DrawLine(bottom, left);
            Gizmos.DrawLine(left, top);
        }

        private void DrawLabel(Vector3 worldPos, string text, Color color)
        {
            float cellSize = GridDataSO.CellSize;
            Vector3 labelPos = worldPos + new Vector3(cellSize * 0.1f, cellSize * 0.4f, 0);

            GUIStyle style = new GUIStyle
            {
                normal = { textColor = color },
                fontSize = labelFontSize,
                fontStyle = FontStyle.Bold
            };

            Handles.Label(labelPos, text, style);
        }

        private Vector3 GetWorldPosition(Vector2Int gridPos)
        {
            float cellSize = GridDataSO.CellSize;
            return new Vector3(
                gridPos.x * cellSize + cellSize * 0.5f,
                gridPos.y * cellSize + cellSize * 0.5f,
                0f
            );
        }
        //
        // [Button("Focus on King", ButtonSizes.Medium)]
        // [GUIColor(0.4f, 0.8f, 1f)]
        // private void FocusOnKing()
        // {
        //     var kingPos = SpawnDataSO?.GetKingPosition();
        //     if (kingPos.HasValue)
        //     {
        //         Vector3 worldPos = GetWorldPosition(kingPos.Value);
        //         Selection.activeGameObject = gameObject;
        //         SceneView.lastActiveSceneView.Frame(new Bounds(worldPos, Vector3.one * 5f), false);
        //     }
        //     else
        //     {
        //         Debug.LogWarning("King position not found");
        //     }
        // }
        //
        // [Button("Update Summon Zone from King", ButtonSizes.Medium)]
        // [GUIColor(0.3f, 0.9f, 0.3f)]
        // private void UpdateSummonZone()
        // {
        //     if (SpawnDataSO != null)
        //     {
        //         var kingPos = SpawnDataSO.GetKingPosition();
        //         if (kingPos.HasValue)
        //         {
        //             SpawnDataSO.SummonZone.UpdateKingPosition(kingPos.Value);
        //             EditorUtility.SetDirty(SpawnDataSO);
        //         }
        //     }
        // }
    }
}
#endif