using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform miniMapArea;
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private RectTransform playerDirectionArrow;

    [Header("World Bounds (X/Z)")]
    [SerializeField] private Vector2 worldMin = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 worldMax = new Vector2(50f, 50f);

    [Header("Marker")]
    [SerializeField] private bool clampMarkerInsideMap = true;

    private void LateUpdate()
    {
        if (player == null || miniMapArea == null || playerMarker == null)
        {
            return;
        }

        UpdatePlayerMarkerPosition();
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

    private void UpdatePlayerMarkerPosition()
    {
        var size = miniMapArea.rect.size;

        var safeRangeX = Mathf.Max(0.0001f, worldMax.x - worldMin.x);
        var safeRangeZ = Mathf.Max(0.0001f, worldMax.y - worldMin.y);

        var normalizedX = (player.position.x - worldMin.x) / safeRangeX;
        var normalizedY = (player.position.z - worldMin.y) / safeRangeZ;

        if (clampMarkerInsideMap)
        {
            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedY = Mathf.Clamp01(normalizedY);
        }

        var anchoredX = (normalizedX - 0.5f) * size.x;
        var anchoredY = (normalizedY - 0.5f) * size.y;

        playerMarker.anchoredPosition = new Vector2(anchoredX, anchoredY);
    }

    private void UpdatePlayerDirectionArrow()
    {
        if (playerDirectionArrow == null)
        {
            return;
        }

        var yaw = player.eulerAngles.y;

        // 미니맵은 북쪽(월드 +Z)이 항상 위를 향하도록 고정.
        // Unity UI 회전은 시계 방향이 음수이므로 yaw를 음수로 반영.
        playerDirectionArrow.localEulerAngles = new Vector3(0f, 0f, -yaw);

        // 화살표 기준점을 플레이어 마커와 항상 동일하게 유지.
        playerDirectionArrow.anchoredPosition = playerMarker.anchoredPosition;
    }
}
