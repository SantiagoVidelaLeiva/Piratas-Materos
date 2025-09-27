using UnityEngine;
using UnityEngine.UI;

public class CharacterHealthBar : MonoBehaviour
{
    public Slider slider;
	public Image fill;

	public void SetMaxHealth(float health)
	{
		slider.maxValue = health;
		slider.value = health;
	}

    public void SetHealth(float health)
	{
		slider.value = health;
	}
}
