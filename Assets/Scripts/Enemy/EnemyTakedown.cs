using System.Diagnostics;
using UnityEngine;

public class EnemyTakedown : MonoBehaviour, IInteractable
{
    private CharacterHealth _enemyHealth;
    private bool _hasBeenUsed = false;

    [SerializeField] private string _interactPrompt = "E to takedown";

    private void Awake()
    {
        _enemyHealth = GetComponentInParent<CharacterHealth>();
        if (_enemyHealth == null)
        {
            UnityEngine.Debug.LogError("No se encontró el componente CharacterHealth en este objeto o en su padre.");
        }
    }

    public string InteractPrompt => _interactPrompt;

    public bool Interact()
    {
        if (_hasBeenUsed)
        {
            return false;
        }

        UnityEngine.Debug.Log("Takedown activated!");
        _hasBeenUsed = true;

        if (_enemyHealth == null)
        {
            UnityEngine.Debug.LogError("La referencia a CharacterHealth no está asignada en el Inspector. Por favor, arrastra el objeto padre del enemigo al campo '_enemyHealth' del script EnemyTakedown.");
            return false;
        }

        
        _enemyHealth.TakeDamage(1000f);

        return true;
    }
}
