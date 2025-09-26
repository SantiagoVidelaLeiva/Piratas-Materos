
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
    public TMP_Dropdown resolucionDropdown;

    private Resolution[] resoluciones;
    private bool isPaused;

    private void Start()
    {
        // Mostrar solo el panel principal al inicio
        if (panelPrincipal) panelPrincipal.SetActive(true);
        if (panelOpciones) panelOpciones.SetActive(false);

        // Estado inicial fullscreen
        if (fullscreenToggle) fullscreenToggle.isOn = Screen.fullScreen;

        // Estado inicial volumen
        if (volumenSlider)
        {
            volumenSlider.minValue = 0f;
            volumenSlider.maxValue = 1f;
            volumenSlider.value = AudioListener.volume;
        }

        // Poblar el dropdown con resoluciones
        InitResoluciones();
    }

    private void InitResoluciones()
    {
        if (!resolucionDropdown) return;

        resoluciones = Screen.resolutions;
        var opciones = new System.Collections.Generic.List<string>();
        int actual = 0;

        for (int i = 0; i < resoluciones.Length; i++)
        {
            var r = resoluciones[i];

            // Convertir refreshRateRatio a Hz
            int hz = Mathf.RoundToInt((float)r.refreshRateRatio.numerator / r.refreshRateRatio.denominator);

            opciones.Add($"{r.width} x {r.height} @{hz}Hz");

            if (r.width == Screen.currentResolution.width &&
                r.height == Screen.currentResolution.height &&
                r.refreshRateRatio.Equals(Screen.currentResolution.refreshRateRatio))
            {
                actual = i;
            }
        }

        resolucionDropdown.ClearOptions();
        resolucionDropdown.AddOptions(opciones);
        resolucionDropdown.value = actual;
        resolucionDropdown.RefreshShownValue();
    }

    public void CambiarResolucion(int index)
    {
        if (resoluciones == null || index < 0 || index >= resoluciones.Length) return;

        var r = resoluciones[index];

        // Tomar el modo actual de pantalla
        var mode = Screen.fullScreenMode;

        // Crear un RefreshRate a partir de refreshRateRatio
        var rr = new RefreshRate
        {
            numerator = r.refreshRateRatio.numerator,
            denominator = r.refreshRateRatio.denominator
        };

        Screen.SetResolution(r.width, r.height, mode, rr);
    }

    public void CambiarPantallaCompleta(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void CambiarVolumen(float v)
    {
        AudioListener.volume = v;
    }

    // ---------- BOTONES PRINCIPALES ----------
    public void Jugar()
    {
        // Cambia "NombreEscenaJuego" por el nombre real de tu escena
        SceneManager.LoadScene("Mapa");
        Time.timeScale = 1.0f;
    }

    public void AbrirOpciones()
    {
        panelPrincipal.SetActive(false);
        panelOpciones.SetActive(true);
    }

    public void Volver()
    {
        panelOpciones.SetActive(false);
        panelPrincipal.SetActive(true);
    }

    public void Salir()
    {
        Application.Quit();
        Debug.Log("Salir del juego (en editor no se cierra).");
    }

    public void Pause()
    {
        if(!isPaused)
        {
            if (panelOpciones) panelOpciones.SetActive(true);
            isPaused = true;
        }
        else
        {
            if (panelOpciones) panelOpciones.SetActive(false);
            isPaused = false;
        }

    }
}
