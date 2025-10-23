using UnityEngine;

public class Aim : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform pivot;   // Pivot que sigue al jugador (colocalo como hijo del player en la cabeza/pecho)
    [SerializeField] Transform cam;     // Transform de la Camera

    [Header("Input")]
    [SerializeField] KeyCode aimKey = KeyCode.Mouse1;

    [Header("Offsets")]
    [SerializeField] Vector3 hipOffset = new Vector3(0f, 0.6f, -3.2f);
    [SerializeField] Vector3 aimOffset = new Vector3(0.6f, 0.55f, -2.6f); // hombro derecho

    [Header("Rotación (Yaw/Pitch)")]
    [SerializeField] float hipXSens = 180f;
    [SerializeField] float hipYSens = 110f;
    [SerializeField] Vector2 hipPitchClamp = new Vector2(-40f, 70f);

    [SerializeField] float aimXSens = 300f; // “más amplitud”
    [SerializeField] float aimYSens = 180f;
    [SerializeField] Vector2 aimPitchClamp = new Vector2(-65f, 85f);

    [Header("Suavizado")]
    [SerializeField] float offsetLerp = 12f;
    [SerializeField] float rotLerp = 14f;

    float yaw;   // rotación horizontal
    float pitch; // rotación vertical

    void Start()
    {
        if (!pivot) pivot = transform;
        if (!cam) cam = Camera.main.transform;

        Vector3 angles = pivot.rotation.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        cam.localPosition = hipOffset;
    }

    void LateUpdate()
    {
        bool aiming = Input.GetKey(aimKey);
        if (aiming) Debug.Log("Apuntando!");
        // Sensibilidad y clamps según estado
        float xs = aiming ? aimXSens : hipXSens;
        float ys = aiming ? aimYSens : hipYSens;
        Vector2 clamp = aiming ? aimPitchClamp : hipPitchClamp;

        // Input mouse
        float mx = Input.GetAxisRaw("Mouse X");
        float my = Input.GetAxisRaw("Mouse Y");

        yaw += mx * xs * Time.deltaTime;
        pitch -= my * ys * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, clamp.x, clamp.y);

        // Rotación del pivot
        Quaternion targetRot = Quaternion.Euler(pitch, yaw, 0f);
        pivot.rotation = Quaternion.Slerp(pivot.rotation, targetRot, rotLerp * Time.deltaTime);

        // Offset cámara (hombro vs normal)
        Vector3 targetOffset = aiming ? aimOffset : hipOffset;
        cam.localPosition = Vector3.Lerp(cam.localPosition, targetOffset, offsetLerp * Time.deltaTime);
        cam.localRotation = Quaternion.identity; // cámara mira hacia adelante del pivot
    }
}