using UnityEngine;

public class Character : MonoBehaviour, IDamageable
{
    public HealthBar healthBar;
    private float maxHp;
    public float Hp => maxHp;
    public void SetHp(float newHp)
    {
        maxHp = newHp;
    }

    private float currentHp;
    public float CurrentHp => currentHp;
    public void SetCurrentHp(float hp)
    {
        currentHp = hp;
    }

    public void TakeDamage(float amount)
    {
        currentHp -= amount;
        healthBar.UpdateHealth(currentHp);
        Debug.Log("Vida restante: " + currentHp);
    }
}
