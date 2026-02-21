using UnityEngine;
using UnityEngine.Events;

public sealed class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 10;

    [Header("Events")]
    [SerializeField] private UnityEvent onDamaged;
    [SerializeField] private UnityEvent onDied;

    private int currentHealth;

    public int CurrentHealth
    {
        get { return this.currentHealth; }
    }

    private void Awake()
    {
        this.currentHealth = this.maxHealth;
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
    }

    public void Die()
    {
        Destroy(this);
    }
}
