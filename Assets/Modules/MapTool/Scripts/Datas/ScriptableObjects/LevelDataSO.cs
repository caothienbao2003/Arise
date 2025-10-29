#if UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace MapTool
{
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "MapTool/Level Data")]
    public class LevelDataSO : ScriptableObject
    {
        [BoxGroup("Level")]
        [VerticalGroup("Level/Left"), LabelWidth(100)]
        public string levelName = "";

        [BoxGroup("Level")]
        [VerticalGroup("Level/Right"), LabelWidth(100)]
        public SceneAsset levelScene;

        [BoxGroup("Grid Data"), LabelWidth(100)]
        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        [SerializeField]
        public MapDataSO gridData;
    }
}
#endif