using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100;
    private float currentHealth;

    public float CurrentHealth
    {
        get { return CurrentHealth; }
        private set { CurrentHealth = value; }
    }
    public float MaxHealth
    {
        get { return maxHealth; }
        private set { maxHealth = value; }
    }        

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} recibi� {amount} de da�o. Vida actual: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"{gameObject.name} curado. Vida actual: {currentHealth}");
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} muri�.");
        Destroy(gameObject); // pod�s cambiarlo a SetActive(false) si prefer�s
    }
}