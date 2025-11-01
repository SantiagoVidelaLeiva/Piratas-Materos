using System.Diagnostics;
using UnityEngine;

public class EnemyTakedown : MonoBehaviour, IInteractable
{
    private CharacterHealth _enemyHealth;
    private EnemyControllerBase _enemyController;
    private BoxCollider _boxColl;
    private bool _hasBeenUsed = false;

    [SerializeField] private string _interactPrompt = "E to takedown";

    private void Awake()
    {
        _enemyHealth = GetComponentInParent<CharacterHealth>();
        _enemyController = GetComponentInParent<EnemyControllerBase>();

        _boxColl = GetComponent<BoxCollider>();

        if (_enemyHealth == null)
        {
            UnityEngine.Debug.LogError("No se encontr� el componente CharacterHealth en este objeto o en su padre.");
        }
        if (_enemyController == null)
        {
            UnityEngine.Debug.LogError("No se encontr� el componente EnemyController en este objeto o en su padre.");
        }
    }

    void Update()
    {
        if (_enemyController.State == EnemyControllerBase.EnemyState.Danger)
        {
            _boxColl.enabled = false;
        }

        if (_enemyController.State == EnemyControllerBase.EnemyState.Suspicious || _enemyController.State == EnemyControllerBase.EnemyState.Patrolling)
        {
            _boxColl.enabled = true;
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
            UnityEngine.Debug.LogError("La referencia a CharacterHealth no est� asignada en el Inspector. Por favor, arrastra el objeto padre del enemigo al campo '_enemyHealth' del script EnemyTakedown.");
            return false;
        }


        _enemyHealth.TakeDamage(1000f);
        gameObject.SetActive(false);

        return true;
    }
}
