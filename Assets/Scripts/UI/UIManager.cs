using UnityEngine;
using static System.Net.Mime.MediaTypeNames;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject playerUIPanel;
    [SerializeField] private GameObject hackerUIPanel;
    [SerializeField] private EnemyStateAggregator enemyStateAggregator;

    [Header("UI del Ojo")]
    [SerializeField] private UnityEngine.UI.Image eyeImage;
    [SerializeField] private Sprite patrollingEyeSprite;
    [SerializeField] private Sprite suspiciousEyeSprite;
    [SerializeField] private Sprite dangerEyeSprite;

    [Header("UI de Texto de Estado")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private string patrollingText = "Undetected";
    [SerializeField] private string suspiciousText = "Suspicious";
    [SerializeField] private string dangerText = "SEEN";

    private void Start()
    {
        // Al inicio del juego, muestra la UI del jugador
        ShowPlayerUI();

        if (enemyStateAggregator != null)
        {
            enemyStateAggregator.OnGlobalStateChange += OnEnemyStateChange;
        }
    }

    private void OnDestroy()
    {
        // Limpiamos la suscripcin al evento del agregador para evitar errores
        if (enemyStateAggregator != null)
        {
            enemyStateAggregator.OnGlobalStateChange -= OnEnemyStateChange;
        }
    }

    // Este metodo es llamado por el agregador de estados enemigo con el estado mas grave.
    private void OnEnemyStateChange(EnemyControllerBase.EnemyState state)
    {
        if (eyeImage == null) return;

        switch (state)
        {
            case EnemyControllerBase.EnemyState.Patrolling:
                eyeImage.sprite = patrollingEyeSprite;
                break;
            case EnemyControllerBase.EnemyState.Suspicious:
                eyeImage.sprite = suspiciousEyeSprite;
                break;
            case EnemyControllerBase.EnemyState.Danger:
                eyeImage.sprite = dangerEyeSprite;
                break;
        }

        if (statusText != null)
        {
            switch (state)
            {
                case EnemyControllerBase.EnemyState.Patrolling:
                    statusText.text = patrollingText;
                    break;
                case EnemyControllerBase.EnemyState.Suspicious:
                    statusText.text = suspiciousText;
                    break;
                case EnemyControllerBase.EnemyState.Danger:
                    statusText.text = dangerText;
                    break;
            }
        }
    }

    // Método para mostrar la UI del jugador y ocultar la del hacker
    public void ShowPlayerUI()
    {
        if (playerUIPanel != null)
        {
            playerUIPanel.SetActive(true);
        }
        if (hackerUIPanel != null)
        {
            hackerUIPanel.SetActive(false);
        }
    }

    // Método para mostrar la UI del hacker y ocultar la del jugador
    public void ShowHackerUI()
    {
        if (hackerUIPanel != null)
        {
            hackerUIPanel.SetActive(true);
        }
        if (playerUIPanel != null)
        {
            playerUIPanel.SetActive(false);
        }
    }
}