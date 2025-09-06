using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform _target;   // El player
    [SerializeField] Vector3 _targetOffset = new Vector3(0, 1.5f, 0);  // Subo la camara en Y para que este a la altura de la cabeza

    [Header("Orbit")]
    [SerializeField] float _distance = 4f;
    [SerializeField] float _xSpeed = 250f;
    [SerializeField] float _ySpeed = 120f;
    [SerializeField] float _yMin = -35f, _yMax = 70f;

    float _yaw, _pitch;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!_target) return;

        bool leftHeld = Input.GetMouseButton(0);

        if (leftHeld)
        {
            _yaw += Input.GetAxis("Mouse X") * _xSpeed * Time.deltaTime;
            _pitch -= Input.GetAxis("Mouse Y") * _ySpeed * Time.deltaTime;
            _pitch = Mathf.Clamp(_pitch, _yMin, _yMax);
        }

        Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 focus = _target.position + _targetOffset;
        Vector3 camPos = focus - rot * Vector3.forward * _distance;

        transform.position = camPos;
        transform.rotation = rot;
    }

    public Vector3 ForwardOnPlane()
    {
        Vector3 f = transform.forward; // “hacia adelante” de la cámara
        f.y = 0;                       // lo proyecto al plano XZ (quito componente vertical)
        return f.normalized;
    }
}
