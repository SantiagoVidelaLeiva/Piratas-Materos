using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100;
    private float currentHealth;

    [Header("Referencias")]
    private HealthBar _healthBar;

    public float CurrentHealth
    {
        get { return currentHealth; }
        private set { currentHealth = value; }
    }
    public float MaxHealth
    {
        get { return maxHealth; }
        private set { maxHealth = value; }
    }        

    void Awake()
    {
        if (!_healthBar) _healthBar = GetComponentInChildren<HealthBar>();
        currentHealth = maxHealth;
        _healthBar.SetMaxHealth(maxHealth);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} recibi� {amount} de da�o. Vida actual: {currentHealth}");
        if (_healthBar) _healthBar.UpdateHealth(currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"{gameObject.name} curado. Vida actual: {currentHealth}");
        if (_healthBar) _healthBar.UpdateHealth(currentHealth);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} muri�.");
        gameObject.SetActive(false);
    }
}