using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatLogController : MonoBehaviour
{
    [Header("Scroll View")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private Text logEntryTemplate;
    [SerializeField] private int maxEntries = 100;
    [SerializeField] private bool autoScrollToLatest = true;

    private readonly List<Text> spawnedEntries = new List<Text>();
    private Coroutine scrollCoroutine;

    private void Awake()
    {
        if (logEntryTemplate != null)
        {
            logEntryTemplate.gameObject.SetActive(false);
        }
    }

    public void AppendLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (logEntryTemplate == null || contentRoot == null)
        {
            Debug.LogWarning("CombatLogController is missing template/content references.");
            return;
        }

        var entry = Instantiate(logEntryTemplate, contentRoot);
        entry.gameObject.SetActive(true);
        entry.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
        entry.transform.SetAsLastSibling();
        spawnedEntries.Add(entry);

        TrimEntries();
        if (autoScrollToLatest)
        {
            ScrollToBottom();
        }
    }

    public void ClearLog()
    {
        for (var i = 0; i < spawnedEntries.Count; i++)
        {
            var entry = spawnedEntries[i];
            if (entry != null)
            {
                Destroy(entry.gameObject);
            }
        }

        spawnedEntries.Clear();

        if (autoScrollToLatest)
        {
            ScrollToBottom();
        }
    }

    public void LogPlayerDamageToEnemy(int damage)
    {
        var safeDamage = Mathf.Max(0, damage);
        AppendLog($"Player does {safeDamage} damage to Enemy.");
    }

    private void TrimEntries()
    {
        var safeMax = Mathf.Max(1, maxEntries);
        while (spawnedEntries.Count > safeMax)
        {
            var old = spawnedEntries[0];
            spawnedEntries.RemoveAt(0);
            if (old != null)
            {
                Destroy(old.gameObject);
            }
        }
    }

    private void ScrollToBottom()
    {
        if (scrollRect == null)
        {
            return;
        }

        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
        }

        scrollCoroutine = StartCoroutine(CoScrollToBottom());
    }

    private IEnumerator CoScrollToBottom()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
        scrollRect.verticalNormalizedPosition = 0f;
        scrollCoroutine = null;
    }
}
