using UnityEngine;
using UnityEngine.Events;

public sealed class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 10;

    [Header("Events")]
    [SerializeField] private UnityEvent onDamaged;
    [SerializeField] private UnityEvent onDied;
    [SerializeField] private UnityEvent<int, int> onHealthChanged;

    private int currentHealth;

    public int CurrentHealth
    {
        get { return this.currentHealth; }
    }

    public int MaxHealth
    {
        get { return this.maxHealth; }
    }

    public UnityEvent<int, int> OnHealthChanged
    {
        get { return this.onHealthChanged; }
    }

    private void Awake()
    {
        this.currentHealth = this.maxHealth;
        this.NotifyHealthChanged();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (this.currentHealth <= 0)
        {
            return;
        }

        this.currentHealth -= amount;

        if (this.onDamaged != null)
        {
            this.onDamaged.Invoke();
        }

        if (this.currentHealth <= 0)
        {
            this.currentHealth = 0;

            if (this.onDied != null)
            {
                this.onDied.Invoke();
            }
        }

        this.NotifyHealthChanged();
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (this.currentHealth <= 0)
        {
            return;
        }

        this.currentHealth += amount;
        if (this.currentHealth > this.maxHealth)
        {
            this.currentHealth = this.maxHealth;
        }

        this.NotifyHealthChanged();
    }

    public void Die()
    {
        Destroy(this);
    }

    private void NotifyHealthChanged()
    {
        if (this.onHealthChanged != null)
        {
            this.onHealthChanged.Invoke(this.currentHealth, this.maxHealth);
        }
    }
}
