using UnityEngine;

public sealed class GridMap : MonoBehaviour
{
    public readonly struct BlockingVisualInfo
    {
        public BlockingVisualInfo(Vector2 normalizedSize)
        {
            this.NormalizedSize = normalizedSize;
        }

        public Vector2 NormalizedSize { get; }
    }

    [Header("Grid")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float cellCenterY = 0f;

    [Header("Blocking Collision (Walls / Props)")]
    [SerializeField] private LayerMask blockingMask;

    [Header("Floor Validation")]
    [SerializeField] private LayerMask floorMask;
    [SerializeField] private float floorRayStartHeight = 1.5f;
    [SerializeField] private float floorRayDistance = 3.0f;

    [Header("Cast Shape (matches player-ish)")]
    [SerializeField] private float castRadius = 0.35f;
    [SerializeField] private float castHeight = 1.6f;

    [Header("Debug")]
    [SerializeField] private bool drawDebug = true;

    private static readonly Collider[] BlockingOverlapBuffer = new Collider[32];

    public float CellSize
    {
        get { return this.cellSize; }
    }

    public Vector3 SnapToCellCenter(Vector3 worldPos)
    {
        float x = Mathf.Round(worldPos.x / this.cellSize) * this.cellSize;
        float z = Mathf.Round(worldPos.z / this.cellSize) * this.cellSize;
        return new Vector3(x, this.cellCenterY + 0.5f, z);
    }

    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        int cx = Mathf.RoundToInt(worldPos.x / this.cellSize);
        int cz = Mathf.RoundToInt(worldPos.z / this.cellSize);
        return new Vector2Int(cx, cz);
    }

    public Vector3 CellToWorldCenter(Vector2Int cell)
    {
        float x = cell.x * this.cellSize;
        float z = cell.y * this.cellSize;
        return new Vector3(x, this.cellCenterY, z);
    }

    public bool HasFloor(Vector2Int cell)
    {
        Vector3 center = this.CellToWorldCenter(cell);

        Vector3 rayStart = center + new Vector3(0f, this.floorRayStartHeight, 0f);
        Ray ray = new Ray(rayStart, Vector3.down);

        bool hit = Physics.Raycast(ray, this.floorRayDistance, this.floorMask, QueryTriggerInteraction.Ignore);

        if (this.drawDebug)
        {
            Debug.DrawLine(rayStart, rayStart + Vector3.down * this.floorRayDistance, hit ? Color.green : Color.red, 0.05f, false);
        }

        return hit;
    }

    public bool IsPathBlocked(Vector3 fromWorldCenter, Vector3 toWorldCenter)
    {
        Vector3 from = this.SnapToCellCenter(fromWorldCenter);
        Vector3 to = this.SnapToCellCenter(toWorldCenter);

        Vector3 p1 = from + new Vector3(0f, 0.1f, 0f);
        Vector3 p2 = from + new Vector3(0f, Mathf.Max(0.2f, this.castHeight), 0f);

        Vector3 delta = to - from;
        float dist = delta.magnitude;

        if (dist <= 0.0001f)
        {
            return false;
        }

        Vector3 dir = delta / dist;

        bool blocked = Physics.CapsuleCast(
            p1,
            p2,
            this.castRadius,
            dir,
            dist,
            this.blockingMask,
            QueryTriggerInteraction.Ignore
        );

        if (this.drawDebug)
        {
            Debug.DrawLine(from, to, blocked ? Color.red : Color.cyan, 0.05f, false);
        }

        return blocked;
    }

    public bool CanEnterCell(Vector3 currentWorldCenter, Vector2Int targetCell)
    {
        if (!this.HasFloor(targetCell))
        {
            return false;
        }

        Vector3 targetCenter = this.CellToWorldCenter(targetCell);

        if (this.IsPathBlocked(currentWorldCenter, targetCenter))
        {
            return false;
        }

        return true;
    }

    public bool TryGetBlockingVisual(Vector2Int cell, out BlockingVisualInfo info)
    {
        Vector3 center = this.CellToWorldCenter(cell) + new Vector3(0f, Mathf.Max(0.5f, this.castHeight * 0.5f), 0f);
        Vector3 halfExtents = new Vector3(this.cellSize * 0.5f, Mathf.Max(0.5f, this.castHeight * 0.5f), this.cellSize * 0.5f);

        int hitCount = Physics.OverlapBoxNonAlloc(
            center,
            halfExtents,
            BlockingOverlapBuffer,
            Quaternion.identity,
            this.blockingMask,
            QueryTriggerInteraction.Ignore
        );

        if (hitCount <= 0)
        {
            info = default;
            return false;
        }

        Collider selected = null;
        float bestScore = float.MaxValue;
        Vector3 cellCenter = this.CellToWorldCenter(cell);

        for (int i = 0; i < hitCount; i++)
        {
            Collider candidate = BlockingOverlapBuffer[i];
            if (candidate == null)
            {
                continue;
            }

            Vector3 closest = candidate.ClosestPoint(cellCenter);
            float score = (closest - cellCenter).sqrMagnitude;
            if (score < bestScore)
            {
                bestScore = score;
                selected = candidate;
            }
        }

        if (selected == null)
        {
            info = default;
            return false;
        }

        Vector3 size = selected.bounds.size;
        float normalizedX = Mathf.Clamp(size.x / this.cellSize, 0.06f, 1f);
        float normalizedZ = Mathf.Clamp(size.z / this.cellSize, 0.06f, 1f);
        info = new BlockingVisualInfo(new Vector2(normalizedX, normalizedZ));
        return true;
    }
}
