using UnityEngine;

public sealed class GridOccupant : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridMap gridMap;

    [Header("Settings")]
    [SerializeField] private bool snapToCellOnStart = true;

    public Vector2Int Cell { get; private set; }

    private void Awake()
    {
        if (this.gridMap == null)
        {
            this.gridMap = FindFirstObjectByType<GridMap>();
        }

        if (this.gridMap == null)
        {
            Debug.LogError("GridOccupant could not find GridMap.");
        }
    }

    private void Start()
    {
        if (this.gridMap == null)
        {
            return;
        }

        if (this.snapToCellOnStart)
        {
            Vector3 snapped = this.gridMap.SnapToCellCenter(this.transform.position);
            this.transform.position = snapped;
        }

        this.Cell = this.gridMap.WorldToCell(this.transform.position);
    }

    public void RefreshCellFromTransform()
    {
        if (this.gridMap == null)
        {
            return;
        }

        this.Cell = this.gridMap.WorldToCell(this.transform.position);
    }

    public void SetCell(Vector2Int cell)
    {
        if (this.gridMap == null)
        {
            return;
        }

        this.Cell = cell;
        this.transform.position = this.gridMap.CellToWorldCenter(cell);
    }
}
