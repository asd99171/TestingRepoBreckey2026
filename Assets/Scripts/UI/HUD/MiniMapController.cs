using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform miniMapArea;
    [SerializeField] private RectTransform miniMapContent;
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private RectTransform playerDirectionArrow;

    [Header("Map Bounds Source (Optional)")]
    [SerializeField] private Collider mapBoundsCollider;
    [SerializeField] private Renderer mapBoundsRenderer;
    [SerializeField] private bool applyBoundsFromMapSourceOnAwake = true;

    [Header("World Bounds (X/Z)")]
    [SerializeField] private Vector2 worldMin = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 worldMax = new Vector2(50f, 50f);

    [Header("Mode")]
    [SerializeField] private bool useViewWindowMode = true;
    [SerializeField] private bool clampMarkerInsideMap = true;

    private void Awake()
    {
        if (applyBoundsFromMapSourceOnAwake)
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
