using UnityEngine;

public class CharacterHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private GameObject characterCanvas;
    [SerializeField] private CharacterHealthBar characterHealthBar;
    [SerializeField] private float maxHealth = 100;
    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public System.Action OnDied;

    void Awake()
    {
        currentHealth = maxHealth;
        characterHealthBar.SetMaxHealth(maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (!characterCanvas.activeSelf)
        {
            characterCanvas.SetActive(true);
        }

        currentHealth -= amount;
        characterHealthBar.SetHealth(currentHealth);
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
        if (characterCanvas.activeSelf)
        {
            characterCanvas.SetActive(false);
        }

        Debug.Log($"{gameObject.name} muri�.");

        OnDied?.Invoke();

        Destroy(gameObject);
    }
}