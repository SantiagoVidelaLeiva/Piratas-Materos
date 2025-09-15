using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    [SerializeField] private Slider slider;
    [SerializeField] private Image fill;
	public void SetMaxHealth(float maxHealth)
	{
		slider.maxValue = maxHealth;
		slider.value = maxHealth;
	}

    public void UpdateHealth(float health)
	{
		slider.value = health;
	}

}
