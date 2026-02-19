using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Bars")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Text txtHealth;
    [SerializeField] private Image oxygenBar;
    [SerializeField] private Text txtOxygen;

    [Header("States")]
    [SerializeField] private Text txtCombatState;
    [SerializeField] private Text txtTurnState;
    [SerializeField] private Text txtPrompt;

    private void Start()
    {
        SetHealth(100f, 100f);
        SetOxygen(100f, 100f);
        SetCombatState(false);
        SetTurnState(true);
        SetPrompt("Ready.");
    }

    public void SetHealth(float current, float max)
    {
        ApplyBarAndLabel(healthBar, txtHealth, current, max, "HP");
    }

    public void SetOxygen(float current, float max)
    {
        ApplyBarAndLabel(oxygenBar, txtOxygen, current, max, "O2");
    }

    public void SetCombatState(bool inCombat)
    {
        if (txtCombatState != null)
        {
            txtCombatState.text = inCombat ? "In Combat" : "Exploration";
        }
    }

    public void SetTurnState(bool playerTurn)
    {
        if (txtTurnState != null)
        {
            txtTurnState.text = playerTurn ? "Your Turn" : "Enemy Turn";
        }
    }

    public void SetPrompt(string text)
    {
        if (txtPrompt != null)
        {
            txtPrompt.text = text;
        }
    }

    private static void ApplyBarAndLabel(Image bar, Text label, float current, float max, string prefix)
    {
        var safeMax = Mathf.Max(1f, max);
        var clampedCurrent = Mathf.Clamp(current, 0f, safeMax);
        var normalized = clampedCurrent / safeMax;

        if (bar != null)
        {
            bar.fillAmount = normalized;
        }

        if (label != null)
        {
            label.text = $"{prefix} {Mathf.RoundToInt(clampedCurrent)} / {Mathf.RoundToInt(safeMax)}";
        }
    }
}
