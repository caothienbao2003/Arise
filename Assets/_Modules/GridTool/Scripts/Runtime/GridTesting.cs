using GridTool;
using GridUtilities;
using UnityEngine;

public class GridTesting : MonoBehaviour
{
    private Grid<CellData> grid;

    private void Start()
    {
        grid = new Grid<CellData>();
    }
}
