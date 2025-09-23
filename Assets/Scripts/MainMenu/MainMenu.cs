using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    [Header("Opcional: nombre de la escena de juego (o usa índice 1)")]
    [SerializeField] private string gameSceneName = "";

    // Referencias encontradas por nombre
    private GameObject mainPanel;
    private GameObject optionPanel;

    private Button btnJugar;
    private Button btnOpciones;
    private Button btnSalir;
    private Button btnMenuVolver;

    private Toggle tglFullScreen;

    // Soporta TMP_Dropdown y Dropdown clásico
    private TMP_Dropdown tmpResolutionDropdown;
    private Dropdown uguiResolutionDropdown;

    private Resolution[] resolutions;
    private int currentResolutionIndex;

    void Awake()
    {
        // Busca dentro del Canvas
        var canvas = GetComponentInChildren<Canvas>(true);
        if (canvas == null)
        {
            Debug.LogError("MainMenu: No encontré un Canvas en la escena.");
            return;
        }

        // Paneles
        mainPanel   = FindChild(canvas.gameObject, "MainPanel");
        optionPanel = FindChild(canvas.gameObject, "OptionPanel");

        // Botones del MainPanel
        btnJugar    = FindChildComponent<Button>(mainPanel, "Jugar");
        btnOpciones = FindChildComponent<Button>(mainPanel, "Opciones");
        btnSalir    = FindChildComponent<Button>(mainPanel, "Salir");

        // Botón volver del OptionPanel (llamado 'Menu' en tu jerarquía)
        btnMenuVolver = FindChildComponent<Button>(optionPanel, "Menu");

        // Toggle y Dropdown de opciones
        tglFullScreen = FindChildComponent<Toggle>(optionPanel, "FullScreen");

        // Intentar con TMP_Dropdown primero; si no, con Dropdown clásico
        tmpResolutionDropdown = FindChildComponent<TMP_Dropdown>(optionPanel, "Resolucion");
        if (tmpResolutionDropdown == null)
            uguiResolutionDropdown = FindChildComponent<Dropdown>(optionPanel, "Resolucion");

        // Listeners
        if (btnJugar)    btnJugar.onClick.AddListener(PlayGame);
        if (btnOpciones) btnOpciones.onClick.AddListener(OpenOptions);
        if (btnSalir)    btnSalir.onClick.AddListener(QuitGame);
        if (btnMenuVolver) btnMenuVolver.onClick.AddListener(CloseOptions);

        if (tglFullScreen)
        {
            tglFullScreen.isOn = Screen.fullScreen;
            tglFullScreen.onValueChanged.AddListener(SetFullscreen);
        }

        // Dropdown resoluciones
        SetupResolutionDropdown();

        // Estado inicial de paneles
        if (mainPanel)   mainPanel.SetActive(true);
        if (optionPanel) optionPanel.SetActive(false);
    }

    // ---------- Acciones de botones ----------
    public void PlayGame()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
        else
            SceneManager.LoadScene(1); // Asume índice 1 es tu escena de juego
    }

    public void OpenOptions()
    {
        if (mainPanel) mainPanel.SetActive(false);
        if (optionPanel) optionPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionPanel) optionPanel.SetActive(false);
        if (mainPanel) mainPanel.SetActive(true);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- Opciones ----------
    private void SetupResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        var labels = new List<string>();
        currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string label = $"{resolutions[i].width} x {resolutions[i].height} @ {resolutions[i].refreshRateRatio.value}Hz";
            labels.Add(label);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }

        if (tmpResolutionDropdown != null)
        {
            tmpResolutionDropdown.ClearOptions();
            tmpResolutionDropdown.AddOptions(labels);
            tmpResolutionDropdown.value = currentResolutionIndex;
            tmpResolutionDropdown.RefreshShownValue();
            tmpResolutionDropdown.onValueChanged.AddListener(SetResolutionTMP);
        }
        else if (uguiResolutionDropdown != null)
        {
            uguiResolutionDropdown.ClearOptions();
            uguiResolutionDropdown.AddOptions(labels);
            uguiResolutionDropdown.value = currentResolutionIndex;
            uguiResolutionDropdown.RefreshShownValue();
            uguiResolutionDropdown.onValueChanged.AddListener(SetResolutionUGUI);
        }
    }

    private void SetResolutionTMP(int index)   => ApplyResolution(index);
    private void SetResolutionUGUI(int index)  => ApplyResolution(index);

    private void ApplyResolution(int index)
    {
        if (resolutions == null || resolutions.Length == 0) return;
        var res = resolutions[Mathf.Clamp(index, 0, resolutions.Length - 1)];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);
        currentResolutionIndex = index;
    }

    private void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // ---------- Helpers ----------
    private static GameObject FindChild(GameObject parent, string childName)
    {
        if (parent == null) return null;
        var trs = parent.GetComponentsInChildren<Transform>(true);
        foreach (var t in trs)
            if (t.name == childName) return t.gameObject;
        Debug.LogWarning($"MainMenu: No encontré '{childName}'.");
        return null;
    }

    private static T FindChildComponent<T>(GameObject parent, string childName) where T : Component
    {
        var go = FindChild(parent, childName);
        if (go == null) return null;
        var comp = go.GetComponent<T>();
        if (comp == null) Debug.LogWarning($"MainMenu: '{childName}' no tiene componente {typeof(T).Name}.");
        return comp;
    }
}
