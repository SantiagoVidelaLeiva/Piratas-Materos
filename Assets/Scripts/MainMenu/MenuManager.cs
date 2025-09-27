using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelPrincipal;   // Panel con Jugar / Opciones / Salir
    public GameObject panelOpciones;    // Panel con las opciones

    [Header("Opciones UI")]
    public Toggle fullscreenToggle;
    public Slider volumenSlider;

    private bool isPaused;

    private void Start()
    {
        // Mostrar solo el panel principal al inicio
        if (panelPrincipal) panelPrincipal.SetActive(true);
        if (panelOpciones) panelOpciones.SetActive(false);

        // Fullscreen
        if (fullscreenToggle)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(CambiarPantallaCompleta);
        }

        // Volumen (con preferencia guardada)
        if (volumenSlider)
        {
            volumenSlider.minValue = 0f;
            volumenSlider.maxValue = 100f;
            float vol = PlayerPrefs.GetFloat("vol_master", AudioListener.volume);
            AudioListener.volume = vol;
            volumenSlider.value = vol;
            volumenSlider.onValueChanged.AddListener(CambiarVolumen);
        }

        // Sin forzar resolución: Unity usa la nativa del dispositivo.
        // Si querés "ventana sin bordes" usando la resolución del monitor:
        // Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
    }

    // ---------- BOTONES PRINCIPALES ----------
    public void Jugar()
    {
        SceneManager.LoadScene("Mapa"); // Cambiá el nombre si corresponde
        Time.timeScale = 1.0f;
    }

    public void AbrirOpciones()
    {
        if (panelPrincipal) panelPrincipal.SetActive(false);
        if (panelOpciones) panelOpciones.SetActive(true);
    }

    public void Volver()
    {
        if (panelOpciones) panelOpciones.SetActive(false);
        if (panelPrincipal) panelPrincipal.SetActive(true);
    }

    public void Salir()
    {
        Application.Quit();
        Debug.Log("Salir del juego (en editor no se cierra).");
    }

    public void Pause()
    {
        isPaused = !isPaused;
        if (panelOpciones) panelOpciones.SetActive(isPaused);
    }

    // ---------- OPCIONES ----------
    public void CambiarPantallaCompleta(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        // Alternativa sin bordes:
        // Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }

    public void CambiarVolumen(float v)
    {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat("vol_master", v);
        PlayerPrefs.Save();
    }
}
