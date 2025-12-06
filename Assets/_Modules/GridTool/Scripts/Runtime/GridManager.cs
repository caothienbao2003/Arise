using UnityEngine;

namespace GridTool
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [SerializeField] private GridDataSO gridDataSO;

    }
}