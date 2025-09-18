using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject playerUIPanel;
    [SerializeField] private GameObject hackerUIPanel;

    private void Start()
    {
        // Al inicio del juego, muestra la UI del jugador
        ShowPlayerUI();
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