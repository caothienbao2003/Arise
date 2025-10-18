using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIUtils
{
    public static TextMeshProUGUI CreateWorldText(
        string text,
        Vector2 worldPosition,
        Vector2 size,
        float fontSize = 40,
        TextAlignmentOptions textAlignment = TextAlignmentOptions.Center,
        Color? color = null,
        Transform parent = null,
        Canvas canvas = null,
        int sortingOrder = 100
        )
    {
        GameObject textObject = new GameObject("World_Text", typeof(RectTransform));

        if (parent != null)
        {
            textObject.transform.SetParent(parent, false);
        }

        textObject.transform.position = worldPosition;

        if (canvas == null)
        {
            canvas = textObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler canvasScaler = textObject.AddComponent<CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 10f;
        }

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;

        TextMeshProUGUI textMesh = textObject.AddComponent<TextMeshProUGUI>();
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.alignment = textAlignment;
        textMesh.textWrappingMode = TextWrappingModes.NoWrap;

        return textMesh;
    }
}
