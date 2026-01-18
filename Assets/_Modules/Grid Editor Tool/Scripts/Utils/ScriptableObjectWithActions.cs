using Sirenix.OdinInspector;
using UnityEngine;

namespace _Modules.Grid_Editor_Tool.Scripts.Utils
{
    public abstract class ScriptableObjectWithActions: ScriptableObject
    {
#if UNITY_EDITOR
        protected bool IsSavedAsset =>
            ScriptableAssetEditorUtils.IsSavedAsset(this);

        [BoxGroup("Actions")]
        [HorizontalGroup("Actions/Buttons")]
        [Button("Ping", ButtonSizes.Large)]
        [GUIColor(0.4f, 0.8f, 1f)]
        [ShowIf(nameof(IsSavedAsset))]
        [PropertyOrder(998)]
        protected virtual void PingAsset()
        {
            ScriptableAssetEditorUtils.Open(this);
        }

        [HorizontalGroup("Actions/Buttons")]
        [Button("Delete", ButtonSizes.Large)]
        [GUIColor(1f, 0.4f, 0.4f)]
        [ShowIf(nameof(IsSavedAsset))]
        [PropertyOrder(999)]
        protected virtual void DeleteAsset()
        {
            if (!ScriptableAssetEditorUtils.ConfirmDelete(this))
                return;

            ScriptableAssetEditorUtils.Delete(this);
        }
#endif
    }
}