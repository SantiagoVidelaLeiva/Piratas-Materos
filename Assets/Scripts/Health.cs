using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100;
    private float currentHealth;

    //  Eventos
    public event Action<float> OnHealthChanged; // vida actual, vida máxima

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} recibió {amount} de daño. Vida actual: {currentHealth}");

        // Avisos
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"{gameObject.name} curado. Vida actual: {currentHealth}");

        // Avisos
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} murió.");
        SceneManager.LoadScene("Lose");   //  Notifico a todos
        //gameObject.SetActive(false);
    }
}