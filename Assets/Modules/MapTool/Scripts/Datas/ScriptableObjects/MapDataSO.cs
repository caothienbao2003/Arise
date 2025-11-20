using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapTool
{
    [CreateAssetMenu(fileName = "NewGridData", menuName = "MapTool/Grid Data")]
    public class MapDataSO : ScriptableObject
    {
        [SerializeField, HideInInspector]
        public List<CellData> CellDatas = new();

        [SerializeField, HideInInspector]
        public List<SpawnPoint> SpawnPoints = new();


        // Spawn point limits
        [Title("Spawn Point Limits")]
        [InfoBox("Set the maximum number of spawn points for each type. Use 0 for unlimited.")]

        [HorizontalGroup("Limits/Row1")]
        [LabelText("King Spawn Limit")]
        [SerializeField] private int maxKingSpawns = 1;

        [HorizontalGroup("Limits/Row1")]
        [LabelText("Enemy Spawn Limit")]
        [SerializeField] private int maxEnemySpawns = 0;

        [HorizontalGroup("Limits/Row2")]
        [LabelText("Item Spawn Limit")]
        [SerializeField] private int maxItemSpawns = 0;

        [HorizontalGroup("Limits/Row2")]
        [LabelText("Monster Spawn Limit")]
        [SerializeField] private int maxMonsterSpawns = 0;


        // Spawn point tools
        [Title("Spawn Point Editor")]
        [EnumToggleButtons]
        [SerializeField] private SpawnType selectedSpawnType = SpawnType.KingSpawn;

        [ShowInInspector, ReadOnly]
        [PropertyOrder(1)]
        private string SpawnCountInfo
        {
            get
            {
                int count = SpawnPoints.Count(sp => sp.spawnType == selectedSpawnType);
                int max = GetMaxSpawnsForType(selectedSpawnType);
                string maxText = max == 0 ? "Unlimited" : max.ToString();
                return $"{selectedSpawnType}: {count} / {maxText}";
            }
        }

        [InfoBox("Click a cell in the grid below to set it as a spawn point.")]
        [Button("Clear All Spawn Points"), GUIColor(1f, 0.5f, 0.5f)]
        private void ClearSpawnPoints()
        {
            SpawnPoints.Clear();
            UnityEditor.EditorUtility.SetDirty(this);
        }


        // Grid visualization
        [Title("Grid Visualization")]
        [ShowInInspector]
        [TableMatrix(DrawElementMethod = "DrawCell", SquareCells = true)]
        private CellVisualData[,] GridPreview
        {
            get
            {
                if (CellDatas == null || CellDatas.Count == 0)
                    return new CellVisualData[1, 1];

                return GenerateGridPreview();
            }
            set { }
        }


        // Debug list
        [Title("Spawn Points List")]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "position")]
        [SerializeField]
        private List<SpawnPoint> debugSpawnPoints
        {
            get => SpawnPoints;
            set => SpawnPoints = value;
        }


        private CellVisualData[,] GenerateGridPreview()
        {
            int minX = CellDatas.Min(c => c.CellPosition.x);
            int maxX = CellDatas.Max(c => c.CellPosition.x);
            int minY = CellDatas.Min(c => c.CellPosition.y);
            int maxY = CellDatas.Max(c => c.CellPosition.y);

            int width = maxX - minX + 1;
            int height = maxY - minY + 1;

            var grid = new CellVisualData[width, height];

            // Fill initial empty cells
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = new CellVisualData
                    {
                        color = new Color(0.2f, 0.2f, 0.2f, 0.5f),
                        position = new Vector2Int(x + minX, (height - 1 - y) + minY),
                        name = "",
                        mapData = this
                    };
                }
            }

            // Place terrain cells
            foreach (var cell in CellDatas)
            {
                int arrayX = cell.CellPosition.x - minX;
                int arrayY = maxY - cell.CellPosition.y;

                if (arrayX >= 0 && arrayX < width && arrayY >= 0 && arrayY < height)
                {
                    grid[arrayX, arrayY] = new CellVisualData
                    {
                        color = cell.TerrainType?.CellColor ?? Color.gray,
                        name = cell.TerrainType?.DisplayName ?? "Empty",
                        position = cell.CellPosition,
                        mapData = this
                    };
                }
            }

            // Overlay spawn points
            foreach (var sp in SpawnPoints)
            {
                int arrayX = sp.position.x - minX;
                int arrayY = maxY - sp.position.y;

                if (arrayX >= 0 && arrayX < width && arrayY >= 0 && arrayY < height)
                {
                    grid[arrayX, arrayY].spawnType = sp.spawnType;
                }
            }

            return grid;
        }


#if UNITY_EDITOR
        private CellVisualData DrawCell(Rect rect, CellVisualData value)
        {
            if (value == null)
                return value;

            // Click interaction
            if (GUI.Button(rect, GUIContent.none))
                OnCellClicked(value.position);

            // Background
            UnityEditor.EditorGUI.DrawRect(rect, value.color);

            // Cell name
            if (!string.IsNullOrEmpty(value.name) && rect.width > 30)
            {
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.Clamp((int)(rect.width / 6), 6, 10),
                    fontStyle = FontStyle.Bold,
                    wordWrap = true
                };

                Rect shadowRect = new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height);
                labelStyle.normal.textColor = Color.black;
                UnityEditor.EditorGUI.LabelField(shadowRect, value.name, labelStyle);

                labelStyle.normal.textColor = Color.white;
                UnityEditor.EditorGUI.LabelField(rect, value.name, labelStyle);
            }

            // Spawn type icon
            if (value.spawnType != SpawnType.None)
            {
                string icon = GetSpawnTypeIcon(value.spawnType);

                GUIStyle iconStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.UpperRight,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold
                };

                Rect shadow = new Rect(rect.x + 1, rect.y + 1, rect.width - 4, rect.height - 4);
                iconStyle.normal.textColor = Color.black;
                UnityEditor.EditorGUI.LabelField(shadow, icon, iconStyle);

                Rect iconRect = new Rect(rect.x, rect.y, rect.width - 4, rect.height - 4);
                iconStyle.normal.textColor = Color.yellow;
                UnityEditor.EditorGUI.LabelField(iconRect, icon, iconStyle);
            }

            // Hover effect
            if (rect.Contains(Event.current.mousePosition))
            {
                Rect hover = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);
                UnityEditor.EditorGUI.DrawRect(hover, new Color(1f, 1f, 1f, 0.2f));
            }

            return value;
        }


        private void OnCellClicked(Vector2Int position)
        {
            SpawnPoint existing = SpawnPoints.Find(sp => sp.position == position);

            if (existing != null)
            {
                if (existing.spawnType == selectedSpawnType)
                {
                    SpawnPoints.Remove(existing);
                }
                else
                {
                    existing.spawnType = selectedSpawnType;
                }
            }
            else
            {
                int count = SpawnPoints.Count(sp => sp.spawnType == selectedSpawnType);
                int max = GetMaxSpawnsForType(selectedSpawnType);

                if (max > 0 && count >= max)
                {
                    UnityEditor.EditorUtility.DisplayDialog(
                        "Spawn Limit Reached",
                        $"Maximum allowed: {max} {selectedSpawnType} spawn points.",
                        "OK"
                    );
                    return;
                }

                SpawnPoints.Add(new SpawnPoint
                {
                    position = position,
                    spawnType = selectedSpawnType
                });
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }


        private int GetMaxSpawnsForType(SpawnType type)
        {
            return type switch
            {
                SpawnType.KingSpawn => maxKingSpawns,
                SpawnType.EnemySpawn => maxEnemySpawns,
                SpawnType.ItemSpawn => maxItemSpawns,
                SpawnType.MonsterSpawn => maxMonsterSpawns,
                _ => 0
            };
        }


        private string GetSpawnTypeIcon(SpawnType type)
        {
            return type switch
            {
                SpawnType.KingSpawn => "K",
                SpawnType.EnemySpawn => "E",
                SpawnType.ItemSpawn => "I",
                SpawnType.MonsterSpawn => "M",
                _ => ""
            };
        }
#endif


        // Runtime functions
        public CellData GetCellAt(Vector2Int position)
        {
            return CellDatas?.Find(c => c.CellPosition == position);
        }

        public List<Vector2Int> GetSpawnPositions(SpawnType type)
        {
            return SpawnPoints
                .Where(sp => sp.spawnType == type)
                .Select(sp => sp.position)
                .ToList();
        }

        public Vector2Int GetKingSpawnPosition()
        {
            var king = SpawnPoints.Find(sp => sp.spawnType == SpawnType.KingSpawn);
            return king != null ? king.position : Vector2Int.zero;
        }
    }


    [System.Serializable]
    public class CellVisualData
    {
        public Color color;
        public string name;
        public Vector2Int position;
        public SpawnType spawnType = SpawnType.None;

        [System.NonSerialized]
        public MapDataSO mapData;
    }


    [System.Serializable]
    public class SpawnPoint
    {
        public Vector2Int position;
        public SpawnType spawnType;

        public override string ToString()
        {
            return $"{spawnType} at ({position.x}, {position.y})";
        }
    }


    public enum SpawnType
    {
        None,
        KingSpawn,
        EnemySpawn,
        ItemSpawn,
        MonsterSpawn
    }
}
