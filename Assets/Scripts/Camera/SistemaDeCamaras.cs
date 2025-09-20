using UnityEngine; 

public class SistemadeCamaras : MonoBehaviour
{
    public Camera thirdPersonCamera;           // Tu c�mara principal del personaje
    public Camera[] securityCameras;           // Lista de c�maras de seguridad

    public PlayerMovement playerMovement;      // Codigos publicos para poder desactivar 
    public CameraOrbit cameraOrbit;

    private bool inSecurityMode = false;
    private int currentSecurityCamIndex = 0;

    void Start()
    {
        // Asegurarse de que solo la c�mara del jugador est� activa al inicio
        EnableThirdPersonView();
    }

    void Update()
    {
        // Activar / salir del modo de c�maras
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

        // Si estamos en modo c�maras, podemos cambiar de c�mara
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

    void EnterSecurityCameraMode()
    {
        thirdPersonCamera.enabled = false;    // desactivo de scripts player movement, camaraorbit y la camara principal
        playerMovement.enabled = false;
        cameraOrbit.enabled = false;

        currentSecurityCamIndex = 0;
        UpdateSecurityCameras();
        Debug.Log("Modo c�mara de seguridad activado.");
    }

    void ExitSecurityCameraMode()
    {
        playerMovement.enabled = true;     // reactivacion de scripts player movement y camara orbit
        cameraOrbit.enabled = true;

        DisableAllSecurityCameras();
        EnableThirdPersonView();
        Debug.Log("Regresando al jugador.");
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
        for (int i = 0; i < securityCameras.Length; i++)
        {
            securityCameras[i].enabled = (i == currentSecurityCamIndex);
        }
    }

    void DisableAllSecurityCameras()
    {
        foreach (Camera cam in securityCameras)
        {
            cam.enabled = false;
        }
    }

    void EnableThirdPersonView()
    {
        thirdPersonCamera.enabled = true;
    }
}