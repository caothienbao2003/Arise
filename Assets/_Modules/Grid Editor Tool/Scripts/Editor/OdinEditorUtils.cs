using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace GridTool
{
    public static class OdinEditorUtils
    {
        public static void HandleCreateNewScriptableObjectWindow<T>(OdinMenuTree tree, string menuName,
            string folderPath, bool allowCreateNew = true)
            where T : ScriptableObject, IDisplayNameable
        {
            if (allowCreateNew)
            {
                var createNewWindow = new CreateNewScriptableObjectWindow<T>(folderPath);
                tree.Add(menuName, createNewWindow);
            }

            IEnumerable<OdinMenuItem> items = tree.AddAllAssetsAtPath(
                menuName,
                folderPath,
                typeof(T),
                includeSubDirectories: true,
                flattenSubDirectories: true
            );

            foreach (var item in items)
            {
                if (item.Value is IDisplayNameable data)
                {
                    item.Name = data.DisplayName;
                }
            }
        }
    }
}