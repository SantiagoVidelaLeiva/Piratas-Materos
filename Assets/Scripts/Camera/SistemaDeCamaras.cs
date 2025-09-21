using System.Diagnostics;
using UnityEngine;

public class SistemaDeCamaras : MonoBehaviour
{
    [SerializeField] private Camera thirdPersonCamera;        // Tu cámara principal del personaje
    [SerializeField] private Camera[] securityCameras;        // Lista de cámaras de seguridad
    [SerializeField] private PlayerMovement playerMovement;    // Referencia al script de movimiento
    [SerializeField] private CameraOrbit cameraOrbit;          // Referencia al script de órbita
    [SerializeField] private UIManager uiManager;              // Referencia al UIManager

    private bool inSecurityMode = false;
    private int currentSecurityCamIndex = 0;

    void Start()
    {
        // Asegurarse de que solo la cámara del jugador esté activa al inicio
        EnableThirdPersonView();

        if (uiManager != null)
        {
            uiManager.ShowPlayerUI();
        }
    }

    void Update()
    {
        // Alternar entre modo de cámaras y modo de jugador con la tecla 'C'
        if (Input.GetKeyDown(KeyCode.C))
        {
            inSecurityMode = !inSecurityMode;

            if (inSecurityMode)
            {
                EnterSecurityCameraMode();
            }
            else
            {
                ExitSecurityCameraMode();
            }
        }

        // Si estamos en modo cámaras, podemos cambiar de cámara con las flechas
        if (inSecurityMode)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                NextCamera();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                PreviousCamera();
            }
        }
    }

    // Unimos la lógica de activación de cámara y UI en una sola función
    void EnterSecurityCameraMode()
    {
        // Desactiva la cámara principal y los scripts del jugador
        if (thirdPersonCamera != null) thirdPersonCamera.enabled = false;
        if (playerMovement != null) playerMovement.enabled = false;
        if (cameraOrbit != null) cameraOrbit.enabled = false;

        // Activa la primera cámara de seguridad
        if (securityCameras.Length > 0)
        {
            currentSecurityCamIndex = 0;
            UpdateSecurityCameras();
        }

        // === GESTIÓN DE UI ===
        // Mostramos la UI del modo Hacker
        if (uiManager != null)
        {
            uiManager.ShowHackerUI();
        }

        UnityEngine.Debug.Log("Modo cámara de seguridad activado.");
    }

    // Unimos la lógica de desactivación de cámara y UI en una sola función
    void ExitSecurityCameraMode()
    {
        // Desactiva todas las cámaras de seguridad
        DisableAllSecurityCameras();

        // Reactiva la cámara principal y los scripts del jugador
        if (playerMovement != null) playerMovement.enabled = true;
        if (cameraOrbit != null) cameraOrbit.enabled = true;
        if (thirdPersonCamera != null) EnableThirdPersonView();

        // === GESTIÓN DE UI ===
        // Mostramos la UI del modo de jugador
        if (uiManager != null)
        {
            uiManager.ShowPlayerUI();
        }

        UnityEngine.Debug.Log("Regresando al jugador.");
    }

    void NextCamera()
    {
        currentSecurityCamIndex = (currentSecurityCamIndex + 1) % securityCameras.Length;
        UpdateSecurityCameras();
    }

    void PreviousCamera()
    {
        currentSecurityCamIndex = (currentSecurityCamIndex - 1 + securityCameras.Length) % securityCameras.Length;
        UpdateSecurityCameras();
    }

    void UpdateSecurityCameras()
    {
        // Activa solo la cámara actual y desactiva el resto
        for (int i = 0; i < securityCameras.Length; i++)
        {
            if (securityCameras[i] != null)
            {
                securityCameras[i].enabled = (i == currentSecurityCamIndex);
            }
        }
    }

    void DisableAllSecurityCameras()
    {
        // Desactiva todas las cámaras en la lista
        foreach (Camera cam in securityCameras)
        {
            if (cam != null)
            {
                cam.enabled = false;
            }
        }
    }

    void EnableThirdPersonView()
    {
        // Asegura que la cámara principal esté activa
        if (thirdPersonCamera != null)
        {
            thirdPersonCamera.enabled = true;
        }
    }
}