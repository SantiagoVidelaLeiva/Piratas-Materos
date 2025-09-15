using UnityEngine;

public class Enemy : Character
{
    void Start()
    {
        SetHp(100f);
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
    //    EnemyPool.Instance.DisableEnemy(gameObject);
    //    SetHp(100f);
    //    SetCurrentHp(Hp);
    //    healthBar.SetMaxHealth(Hp);

    //    GameObject.Destroy si no hay respawn
    //}
}
