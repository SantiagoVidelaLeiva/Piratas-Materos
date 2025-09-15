using UnityEngine;

public class Player : Character
{
    void Start()
    {
        SetHp(30f);
        SetCurrentHp(Hp);

        healthBar.SetMaxHealth(Hp);
    }

    void Update()
    {
        if (CurrentHp <= 0)
        {
            // Die()
        }
    }
    
    //public void Die()
    //{
    //    GameOver screen o directamente un método ResetLevel() x veces hasta que sale GameOver;
    //}
}
