using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GridMap gridMap;
    [SerializeField] private GridOccupancyIndex occupancyIndex;
    [SerializeField] private RectTransform miniMapArea;
    [SerializeField] private RectTransform miniMapContent;
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private RectTransform playerDirectionArrow;

    [Header("Grid Window Minimap")]
    [SerializeField] private bool useGridWindowMinimap = true;
    [SerializeField] private Image gridCellPrefab;
    [SerializeField] private int viewRadiusCells = 5;
    [SerializeField] private float cellPixelSize = 14f;
    [SerializeField] private float refreshIntervalSeconds = 0.15f;

    [Header("Colors")]
    [SerializeField] private Color floorColor = new Color(0.15f, 0.17f, 0.2f, 0.9f);
    [SerializeField] private Color blockedColor = new Color(0.05f, 0.05f, 0.06f, 0.95f);
    [SerializeField] private Color enemyColor = new Color(0.78f, 0.2f, 0.2f, 0.95f);
    [SerializeField] private Color playerColor = new Color(0.2f, 0.78f, 0.3f, 0.95f);
    [SerializeField] private Color wallColor = new Color(0.8f, 0.82f, 0.86f, 0.95f);

    [Header("Wall Overlay")]
    [SerializeField] private bool drawBlockingWalls = true;
    [SerializeField] private float minimumWallPixelThickness = 1f;

    [Header("Map Bounds Source (Optional, Legacy Mode)")]
    [SerializeField] private Collider mapBoundsCollider;
    [SerializeField] private Renderer mapBoundsRenderer;
    [SerializeField] private bool applyBoundsFromMapSourceOnAwake = true;

    [Header("World Bounds (X/Z, Legacy Mode)")]
    [SerializeField] private Vector2 worldMin = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 worldMax = new Vector2(50f, 50f);

    [Header("Legacy Mode")]
    [SerializeField] private bool useViewWindowMode = true;
    [SerializeField] private bool clampMarkerInsideMap = true;

    private readonly Dictionary<Vector2Int, Image> windowCells = new Dictionary<Vector2Int, Image>();
    private readonly Dictionary<Vector2Int, RectTransform> wallOverlays = new Dictionary<Vector2Int, RectTransform>();
    private float refreshTimer;

    private void Awake()
    {
        if (this.gridMap == null)
        {
            this.gridMap = FindFirstObjectByType<GridMap>();
        }

        if (this.occupancyIndex == null)
        {
            this.occupancyIndex = FindFirstObjectByType<GridOccupancyIndex>();
        }

        if (this.player == null)
        {
            var playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                this.player = playerHealth.transform;
            }
        }

        if (this.useGridWindowMinimap)
        {
            this.BuildWindowGrid();
        }
        else if (applyBoundsFromMapSourceOnAwake)
        {
            ApplyWorldBoundsFromMapSource();
        }
    }

    private void LateUpdate()
    {
        if (miniMapArea == null || playerMarker == null || player == null)
        {
            return;
        }

        if (this.useGridWindowMinimap)
        {
            this.UpdateGridWindowMinimap();
            return;
        }

        UpdateMapAndMarkerPosition();
        UpdatePlayerDirectionArrow();
    }

    public void SetPlayer(Transform target)
    {
        player = target;
    }

    public void SetWorldBounds(Vector2 min, Vector2 max)
    {
        worldMin = min;
        worldMax = max;
    }

    public void SetMapBoundsSource(Collider source)
    {
        mapBoundsCollider = source;
        mapBoundsRenderer = null;
        ApplyWorldBoundsFromMapSource();
    }

    public void SetMapBoundsSource(Renderer source)
    {
        mapBoundsRenderer = source;
        mapBoundsCollider = null;
        ApplyWorldBoundsFromMapSource();
    }

    public void ApplyWorldBoundsFromMapSource()
    {
        if (!TryGetMapBounds(out var bounds))
        {
            return;
        }

        SetWorldBounds(new Vector2(bounds.min.x, bounds.min.z), new Vector2(bounds.max.x, bounds.max.z));
    }

    private void BuildWindowGrid()
    {
        if (this.miniMapContent == null || this.gridCellPrefab == null)
        {
            return;
        }

        foreach (var kv in this.windowCells)
        {
            if (kv.Value != null)
            {
                Destroy(kv.Value.gameObject);
            }
        }

        foreach (var kv in this.wallOverlays)
        {
            if (kv.Value != null)
            {
                Destroy(kv.Value.gameObject);
            }
        }

        this.windowCells.Clear();
        this.wallOverlays.Clear();

        int safeRadius = Mathf.Max(1, this.viewRadiusCells);
        int side = (safeRadius * 2) + 1;

        this.miniMapContent.sizeDelta = new Vector2(side * this.cellPixelSize, side * this.cellPixelSize);

        for (int y = -safeRadius; y <= safeRadius; y++)
        {
            for (int x = -safeRadius; x <= safeRadius; x++)
            {
                Image cell = Instantiate(this.gridCellPrefab, this.miniMapContent);
                cell.gameObject.SetActive(true);

                RectTransform rect = cell.rectTransform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(this.cellPixelSize, this.cellPixelSize);
                rect.anchoredPosition = new Vector2(x * this.cellPixelSize, y * this.cellPixelSize);

                this.windowCells[new Vector2Int(x, y)] = cell;

                var wallObject = new GameObject($"WallOverlay_{x}_{y}", typeof(RectTransform), typeof(Image));
                wallObject.transform.SetParent(this.miniMapContent, false);
                var wallRect = wallObject.GetComponent<RectTransform>();
                wallRect.anchorMin = new Vector2(0.5f, 0.5f);
                wallRect.anchorMax = new Vector2(0.5f, 0.5f);
                wallRect.pivot = new Vector2(0.5f, 0.5f);
                wallRect.anchoredPosition = rect.anchoredPosition;
                wallRect.sizeDelta = Vector2.zero;

                var wallImage = wallObject.GetComponent<Image>();
                wallImage.raycastTarget = false;
                wallImage.color = this.wallColor;
                wallObject.SetActive(false);

                this.wallOverlays[new Vector2Int(x, y)] = wallRect;
            }
        }

        this.playerMarker.anchoredPosition = Vector2.zero;
    }

    private void UpdateGridWindowMinimap()
    {
        this.refreshTimer -= Time.deltaTime;
        if (this.refreshTimer > 0f)
        {
            this.UpdatePlayerDirectionArrow();
            return;
        }

        this.refreshTimer = Mathf.Max(0.03f, this.refreshIntervalSeconds);

        if (this.gridMap == null)
        {
            return;
        }

        Vector2Int playerCell = this.gridMap.WorldToCell(this.gridMap.SnapToCellCenter(this.player.position));

        foreach (var kv in this.windowCells)
        {
            Vector2Int offset = kv.Key;
            Image cellImage = kv.Value;
            Vector2Int targetCell = playerCell + offset;

            Color color = this.blockedColor;

            if (this.gridMap.HasFloor(targetCell))
            {
                color = this.floorColor;

                if (this.occupancyIndex != null)
                {
                    GridOccupant occ = this.occupancyIndex.GetAtCell(targetCell);
                    if (occ != null && occ.GetComponent<EnemyHealth>() != null)
                    {
                        color = this.enemyColor;
                    }
                }
            }

            this.UpdateWallOverlay(offset, targetCell);

            if (offset == Vector2Int.zero)
            {
                color = this.playerColor;
            }

            if (cellImage != null)
            {
                cellImage.color = color;
            }
        }

        this.playerMarker.anchoredPosition = Vector2.zero;
        this.UpdatePlayerDirectionArrow();
    }

    private void UpdateWallOverlay(Vector2Int offset, Vector2Int targetCell)
    {
        if (!this.wallOverlays.TryGetValue(offset, out RectTransform wallRect) || wallRect == null)
        {
            return;
        }

        if (!this.drawBlockingWalls || this.gridMap == null)
        {
            wallRect.gameObject.SetActive(false);
            return;
        }

        if (!this.gridMap.TryGetBlockingVisual(targetCell, out GridMap.BlockingVisualInfo visual))
        {
            wallRect.gameObject.SetActive(false);
            return;
        }

        float width = Mathf.Max(this.minimumWallPixelThickness, visual.NormalizedSize.x * this.cellPixelSize);
        float height = Mathf.Max(this.minimumWallPixelThickness, visual.NormalizedSize.y * this.cellPixelSize);

        wallRect.sizeDelta = new Vector2(width, height);
        wallRect.gameObject.SetActive(true);
    }

    private bool TryGetMapBounds(out Bounds bounds)
    {
        if (mapBoundsCollider != null)
        {
            bounds = mapBoundsCollider.bounds;
            return true;
        }

        if (mapBoundsRenderer != null)
        {
            bounds = mapBoundsRenderer.bounds;
            return true;
        }

        bounds = default;
        return false;
    }

    private void UpdateMapAndMarkerPosition()
    {
        var normalized = GetNormalizedPosition();

        if (useViewWindowMode && miniMapContent != null)
        {
            CenterPlayerMarker();
            UpdateMapContentOffset(normalized);
            return;
        }

        UpdatePlayerMarkerOnArea(normalized);
    }

    private Vector2 GetNormalizedPosition()
    {
        var safeRangeX = Mathf.Max(0.0001f, worldMax.x - worldMin.x);
        var safeRangeZ = Mathf.Max(0.0001f, worldMax.y - worldMin.y);

        var worldPosition = player.position;
        var normalizedX = (worldPosition.x - worldMin.x) / safeRangeX;
        var normalizedY = (worldPosition.z - worldMin.y) / safeRangeZ;

        if (clampMarkerInsideMap)
        {
            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedY = Mathf.Clamp01(normalizedY);
        }

        return new Vector2(normalizedX, normalizedY);
    }

    private void UpdatePlayerMarkerOnArea(Vector2 normalized)
    {
        var size = miniMapArea.rect.size;
        var anchoredX = (normalized.x - 0.5f) * size.x;
        var anchoredY = (normalized.y - 0.5f) * size.y;
        playerMarker.anchoredPosition = new Vector2(anchoredX, anchoredY);
    }

    private void CenterPlayerMarker()
    {
        playerMarker.anchoredPosition = Vector2.zero;
    }

    private void UpdateMapContentOffset(Vector2 normalized)
    {
        var areaSize = miniMapArea.rect.size;
        var contentSize = miniMapContent.rect.size;

        var playerOnContentX = (normalized.x - 0.5f) * contentSize.x;
        var playerOnContentY = (normalized.y - 0.5f) * contentSize.y;

        var targetX = -playerOnContentX;
        var targetY = -playerOnContentY;

        var moveLimitX = Mathf.Max(0f, (contentSize.x - areaSize.x) * 0.5f);
        var moveLimitY = Mathf.Max(0f, (contentSize.y - areaSize.y) * 0.5f);

        targetX = Mathf.Clamp(targetX, -moveLimitX, moveLimitX);
        targetY = Mathf.Clamp(targetY, -moveLimitY, moveLimitY);

        miniMapContent.anchoredPosition = new Vector2(targetX, targetY);
    }

    private void UpdatePlayerDirectionArrow()
    {
        if (playerDirectionArrow == null)
        {
            return;
        }

        var yaw = player.eulerAngles.y;
        playerDirectionArrow.localEulerAngles = new Vector3(0f, 0f, -yaw);
        playerDirectionArrow.anchoredPosition = playerMarker.anchoredPosition;
    }
}
