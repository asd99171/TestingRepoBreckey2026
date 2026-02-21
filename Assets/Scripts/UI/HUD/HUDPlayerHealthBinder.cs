using UnityEngine;

/// <summary>
/// Connects PlayerHealth runtime values to HUDController health UI.
/// </summary>
public class HUDPlayerHealthBinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HUDController hudController;
    [SerializeField] private PlayerHealth playerHealth;

    private void Awake()
    {
        if (this.hudController == null)
        {
            this.hudController = GetComponent<HUDController>();
        }

        if (this.playerHealth == null)
        {
            this.playerHealth = FindFirstObjectByType<PlayerHealth>();
        }
    }

    private void OnEnable()
    {
        if (this.playerHealth != null)
        {
            this.playerHealth.OnHealthChanged.AddListener(this.OnHealthChanged);
            this.ApplyCurrentHealth();
        }
    }

    private void OnDisable()
    {
        if (this.playerHealth != null)
        {
            this.playerHealth.OnHealthChanged.RemoveListener(this.OnHealthChanged);
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        if (this.hudController != null)
        {
            this.hudController.SetHealth(current, max);
        }
    }

    private void ApplyCurrentHealth()
    {
        if (this.hudController == null || this.playerHealth == null)
        {
            return;
        }

        this.hudController.SetHealth(this.playerHealth.CurrentHealth, this.playerHealth.MaxHealth);
    }
}
