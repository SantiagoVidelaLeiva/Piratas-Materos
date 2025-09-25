// Este script fue modificado
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // NUEVO: Variable para almacenar las vidas del jugador.
    [Header("Game Settings")]
    [SerializeField] private int maxLives = 3;
    private int _currentLives;

    // NUEVO: Evento para notificar a la UI cuando las vidas cambian.
    public event Action<int> OnLivesChanged;

    public int CurrentLives => _currentLives;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // NUEVO: Inicializamos las vidas al inicio del juego.
        _currentLives = maxLives;
    }

    private void Start()
    {
        // NO HACER NADA AQUI. El GameManager debe esperar a que
        // la escena se cargue para encontrar al jugador.
    }

    // NUEVO: Escucha el evento de cambio de escena para encontrar al jugador.
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // CAMBIO: Se eliminó el reset de vidas en el reinicio de la escena del mapa.
        // Ahora las vidas se conservan entre reintentos. El reset se hará al iniciar una nueva partida.

        // Buscamos al jugador en la escena del mapa.
        if (scene.name == "Mapa")
        {
            var playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null)
            {
                Health playerHealth = playerGo.GetComponent<Health>();
                if (playerHealth != null)
                {
                    // No es necesario suscribirse aquí si Health ya llama a OnPlayerDied.
                }
            }
        }

        // Actualizamos la UI con las vidas actuales al cargar la escena.
        OnLivesChanged?.Invoke(_currentLives);
    }

    // NUEVO: Este método es llamado desde el script Health cuando el jugador muere.
    public void OnPlayerDied()
    {
        _currentLives--;

        // Notificamos a la UI que las vidas han cambiado.
        OnLivesChanged?.Invoke(_currentLives);

        if (_currentLives > 0)
        {
            // Si le quedan vidas, reiniciamos la escena actual.
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            // Si se le acaban las vidas, carga la escena de derrota.
            _currentLives = 3;
            SceneManager.LoadScene("Lose");
        }
    }

    public void WinGame()
    {
        // Carga la escena de victoria.
        SceneManager.LoadScene("Win");
    }

    // NUEVO: Método público para restablecer las vidas a su máximo.
    // Esto debería ser llamado por el script del menú principal cuando el jugador
    // inicia una nueva partida.
    public void ResetLives()
    {
        _currentLives = maxLives;
        OnLivesChanged?.Invoke(_currentLives);
    }
}