using System.Diagnostics;
using UnityEngine;

// Requiere que el script base de interacción y un BoxCollider (trigger) estén presentes.
[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(BoxCollider))]
public class WeaponPickupActivator : MonoBehaviour, IInteractable
{
    [Header("Referencias del Player")]
    [Tooltip("Arrastra el GameObject del Player (o el objeto que tiene PistolAttack).")]
    [SerializeField] private GameObject playerReference;

    [Header("Componentes a Habilitar")]
    [Tooltip("El script 'Aim' que maneja el apuntado y la rotación.")]
    [SerializeField] private Aim aimScript;

    [Tooltip("El script 'PistolAttack' que maneja el disparo.")]
    [SerializeField] private PistolAttack pistolAttackScript;

    // Este mensaje lo leerá el script Interactable.
    public string InteractPrompt => "E para agarrar arma";

    public bool Interact()
    {
        if (aimScript == null || pistolAttackScript == null || playerReference == null)
        {
            UnityEngine.Debug.LogError("WeaponPickupActivator: Faltan referencias a scripts o al Player. No se puede activar la habilidad.", this);
            return false;
        }

        aimScript.enabled = true;
        pistolAttackScript.enabled = true;

        UnityEngine.Debug.Log("Habilidad de Apuntado y Disparo habilitada en el Player.");

        // 3. Opcional: Ocultar o destruir el modelo visual del arma en la mesa
        // Desactivamos el objeto actual (el arma en la mesa). 
        // El script PistolMount se encargará de equipar la versión correcta.
        // El PistolMount ya está implementado para hacer la lógica de equipar/destruir si es necesario, 
        // pero para este pickup, solo necesitamos que la habilidad quede activa.
        gameObject.SetActive(false);

        return true;
    }

    private void Awake()
    {
        // Si no se asigna el player, intentamos encontrarlo por Tag para robustez.
        if (playerReference == null)
        {
            playerReference = GameObject.FindWithTag("Player");
            if (playerReference != null)
            {
                // Intentamos buscar los componentes en el player si lo encontramos
                aimScript = playerReference.GetComponentInChildren<Aim>(true);
                pistolAttackScript = playerReference.GetComponentInChildren<PistolAttack>(true);
            }
        }
    }
}
