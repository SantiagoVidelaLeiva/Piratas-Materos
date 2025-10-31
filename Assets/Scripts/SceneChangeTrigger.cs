using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class SceneChangeTrigger : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("El nombre de la escena a la que se cambiará. ¡Debe estar en Build Settings!")]
    [SerializeField] private string targetSceneName = "NextLevel";

    [Header("Opciones de Bloqueo")]
    [Tooltip("Si se debe bloquear el cursor (útil para escenas de UI como menús o victorias).")]
    [SerializeField] private bool lockCursorOnSceneChange = true;

    private void Awake()
    {
        // Asegura que el BoxCollider es un trigger para detectar entradas
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider)
        {
            boxCollider.isTrigger = true;
        }
        else
        {
            UnityEngine.Debug.LogError("SceneChangeTrigger requiere un BoxCollider.");
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Comprueba si el objeto que entró en el trigger es el jugador.
        if (other.CompareTag("Player"))
        {
            ChangeScene();
        }
    }

    public void ChangeScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            UnityEngine.Debug.LogError("El nombre de la escena destino no puede estar vacío en SceneChangeTrigger.", this);
            return;
        }

        UnityEngine.Debug.Log($"Player ha entrado en el trigger. Cargando escena: {targetSceneName}");

        // Bloqueo de cursor opcional
        if (lockCursorOnSceneChange)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Carga la escena.
        SceneManager.LoadScene(targetSceneName);

        enabled = false;
    }
}
