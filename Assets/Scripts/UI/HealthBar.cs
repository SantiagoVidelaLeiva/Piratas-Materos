using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class HealthBar : MonoBehaviour
{

    [SerializeField] private Slider _slider;
    [SerializeField] private Health _health;
    void Start()
    {
        _health = GameManager.Instance.PlayerHealth;
        _slider = GetComponent<Slider>();
        if (_health) _health.OnHealthChanged += UpdateHealth;
        SetMaxHealth(_health.MaxHealth);
    }
    //private void HandleHealthChanged(float current)
    //{
    //    UpdateHealth(current);
    //}
    public void SetMaxHealth(float maxHealth)
	{
		_slider.maxValue = maxHealth;
        _slider.value = maxHealth;
	}

    public void UpdateHealth(float health)
	{
		_slider.value = health;
	}

}
