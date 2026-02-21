using UnityEngine;

/// <summary>
/// Runtime access point for gameplay scripts (e.g., PlayerAttackCooldown) to write into CombatLog UI.
/// </summary>
public class CombatLogRuntimeBridge : MonoBehaviour
{
    public static CombatLogRuntimeBridge Instance { get; private set; }

    [SerializeField] private CombatLogController combatLogController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (combatLogController == null)
        {
            combatLogController = GetComponent<CombatLogController>();
        }
    }

    public static void ReportPlayerDamageToEnemy(int damage)
    {
        if (Instance == null || Instance.combatLogController == null)
        {
            return;
        }

        Instance.combatLogController.LogPlayerDamageToEnemy(damage);
    }
}
