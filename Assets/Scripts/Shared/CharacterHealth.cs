using UnityEngine;

public class CharacterHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private GameObject characterCanvas;
    [SerializeField] private CharacterHealthBar characterHealthBar;
    [SerializeField] private float maxHealth = 100;
    private float currentHealth;
    private bool isDead = false;
    public bool IsDead => isDead;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public System.Action OnDied;
    public Vector3 LastHitPoint { get; private set; }
    public Vector3 LastHitForce { get; private set; }

    void Awake()
    {
        currentHealth = maxHealth;
        characterHealthBar.SetMaxHealth(maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

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
    public void TakeDamage1(float amount, Vector3 hitPoint, Vector3 hitForce)
    {
        if (isDead) return;


        currentHealth -= amount;
        LastHitPoint = hitPoint;
        LastHitForce = hitForce;
        Debug.Log($"{gameObject.name} recibi� {amount} de da�o. Vida actual: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public  void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        characterHealthBar.SetHealth(currentHealth);
        Debug.Log($"{gameObject.name} curado. Vida actual: {currentHealth}");
    }

    protected  void Die()
    {
        if (isDead) return; 
        isDead = true;

        if (characterCanvas.activeSelf)
            characterCanvas.SetActive(false);

        Debug.Log($"{gameObject.name} murió.");

        OnDied?.Invoke();

    }
}