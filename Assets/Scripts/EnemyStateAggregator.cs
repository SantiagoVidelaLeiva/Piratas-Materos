using System.Collections.Generic;
using UnityEngine;

// Este script escucha a todos los enemigos y determina el estado de amenaza ms alto.
public class EnemyStateAggregator : MonoBehaviour
{
    // Evento que otros scripts pueden escuchar para saber el estado de amenaza actual
    public event System.Action<EnemyControllerBase.EnemyState> OnGlobalStateChange;

    private List<EnemyControllerBase> allEnemies = new List<EnemyControllerBase>();
    private EnemyControllerBase.EnemyState currentGlobalState = EnemyControllerBase.EnemyState.Patrolling;

    private void Start()
    {
        // Busca todos los enemigos en la escena y se suscribe a sus eventos
        allEnemies.AddRange(FindObjectsOfType<EnemyControllerBase>());

        foreach (var enemy in allEnemies)
        {
            if (enemy != null)
            {
                enemy.OnStateChange += OnEnemyStateChange;
                enemy.OnEnemyDestroyed += () => OnEnemyDestroyed(enemy);
            }
        }
    }

    private void OnDestroy()
    {
        // Limpia la suscripcin al evento para evitar errores
        foreach (var enemy in allEnemies)
        {
            if (enemy != null)
            {
                enemy.OnStateChange -= OnEnemyStateChange;
                enemy.OnEnemyDestroyed -= () => OnEnemyDestroyed(enemy);
            }
        }
    }

    private void OnEnemyDestroyed(EnemyControllerBase enemy)
    {
        allEnemies.Remove(enemy);
        OnEnemyStateChange(EnemyControllerBase.EnemyState.Patrolling);
    }

    private void OnEnemyStateChange(EnemyControllerBase.EnemyState enemyState)
    {
        // Determina el estado de amenaza ms alto entre todos los enemigos
        EnemyControllerBase.EnemyState highestState = EnemyControllerBase.EnemyState.Patrolling;

        allEnemies.RemoveAll(item => item == null);

        foreach (var enemy in allEnemies)
        {
            // Solo si el enemigo est activo
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                // La lgica de prioridad: Danger > Suspicious > Patrolling
                if (enemy.CurrentState == EnemyControllerBase.EnemyState.Danger)
                {
                    highestState = EnemyControllerBase.EnemyState.Danger;
                    break; // No necesitamos chequear ms, es el ms alto
                }
                else if (enemy.CurrentState == EnemyControllerBase.EnemyState.Suspicious && highestState < EnemyControllerBase.EnemyState.Suspicious)
                {
                    highestState = EnemyControllerBase.EnemyState.Suspicious;
                }
            }
        }

        // Si el estado global ha cambiado, notifica a los suscriptores
        if (highestState != currentGlobalState)
        {
            currentGlobalState = highestState;
            OnGlobalStateChange?.Invoke(currentGlobalState);
        }
    }
}