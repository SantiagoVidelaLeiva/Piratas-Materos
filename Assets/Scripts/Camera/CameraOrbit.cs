using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(Camera))]
public class CameraOrbit : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform _target;
    [SerializeField] Vector3 _targetOffset = new(0, 1.5f, 0);

    [Header("Orbit")]
    [SerializeField] float _distance = 1.2f;
    [SerializeField] float _xSpeed = 200f;
    [SerializeField] float _ySpeed = 100f;
     float _yMin = -50f, _yMax = 85f;

    [Header("Aim (Right Shoulder)")]
    [SerializeField] Vector3 _aimLocalOffset = new(0.35f, 0.15f, 0.0f);
    [SerializeField] float _aimDistance = 0.9f;
    [SerializeField] float _normalFov = 60f;
    [SerializeField] float _aimFov = 45f;
    [SerializeField] float _aimLerp = 10f;

    [Header("Player Rotation")]
    [SerializeField] bool _rotatePlayerOnAim = true;
    [SerializeField] float _turnSpeed = 12f;
    [SerializeField] float _shoulderOffsetY = 0f;

    [Header("Spawn arma al apuntar")]
    [SerializeField] GameObject weaponPrefab;       // tu prefab del arma
    [SerializeField] Transform weaponSocket;        // socket de la mano derecha
    [SerializeField] Transform aimTarget;
    Camera _cam;
    [SerializeField] float _yaw;
    float _pitch;
    float _aimT;
    Rigidbody _playerRb;
    GameObject _spawnedWeapon;
    [SerializeField] Vector2 _aimLockLocalXZ = new Vector2(-0.024f, 0.0361f);
    [SerializeField] float _lockSnap = 50f;
    [SerializeField] GameObject _crosshair;

    void Awake()
    {
        _cam = GetComponent<Camera>();

        if (!_target)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player) _target = player.transform;
        }

        if (_target) _playerRb = _target.GetComponent<Rigidbody>();
        if (_cam) _cam.fieldOfView = _normalFov;
    }

    void LateUpdate()
    {
        if (!_target) return;

        bool aiming = Input.GetMouseButton(1);

        // === Cámara ===
        _yaw += Input.GetAxis("Mouse X") * _xSpeed * Time.deltaTime;
        _pitch -= Input.GetAxis("Mouse Y") * _ySpeed * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, _yMin, _yMax);

        _aimT = Mathf.MoveTowards(_aimT, aiming ? 1f : 0f, _aimLerp * Time.deltaTime);
        if (_crosshair)
            _crosshair.SetActive(_aimT > 0.15f);
        Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 focus = _target.position + _targetOffset;

        float currDist = Mathf.Lerp(_distance, _aimDistance, _aimT);
        Vector3 localOffset = Vector3.Lerp(Vector3.zero, _aimLocalOffset, _aimT);

        Vector3 camPos = focus + rot * localOffset - rot * Vector3.forward * currDist;
        transform.SetPositionAndRotation(camPos, rot);

        if (_cam)
            _cam.fieldOfView = Mathf.Lerp(_normalFov, _aimFov, _aimT);

        // === Rotar jugador al apuntar ===
        if (_rotatePlayerOnAim && aiming)
        {
            // usar el forward de la cámara pero sin componente vertical
            Vector3 dir = transform.forward;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                targetRot *= Quaternion.Euler(0f, _shoulderOffsetY, 0f);
                Quaternion smooth = Quaternion.Slerp(_playerRb.rotation, targetRot, _turnSpeed * Time.fixedDeltaTime);
                _playerRb.MoveRotation(smooth);
            }
        }

        if (aiming && _spawnedWeapon == null && weaponPrefab && weaponSocket)
        {
            _spawnedWeapon = Instantiate(weaponPrefab, weaponSocket);

            _spawnedWeapon.transform.localPosition = new Vector3(-0.024f, 0.1516f, 0.0361f);
            _spawnedWeapon.transform.localEulerAngles = new Vector3(270f, 90f, 0f);
            _spawnedWeapon.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

            // ← aquí assignamos muzzle al PistolAttack
            var weaponComp = _spawnedWeapon.GetComponent<Muzzle>();
            if (weaponComp != null && weaponComp.muzzle != null)
            {
                // asumimos que PistolAttack está en el player o en un hijo
                var pistolAttack = _target.GetComponentInChildren<PistolAttack>();
                if (pistolAttack != null)
                    pistolAttack.muzzle = weaponComp.muzzle;
            }
        }
        else if (!aiming && _spawnedWeapon != null)
        {
            // quitar referencia antes de destruir el arma
            var pistolAttack = _target.GetComponentInChildren<PistolAttack>();
            if (pistolAttack != null)
                pistolAttack.muzzle = null;

            Destroy(_spawnedWeapon);
            _spawnedWeapon = null;
        }

    }

    public Vector3 ForwardOnPlane()
    {
        Vector3 f = transform.forward;
        f.y = 0;
        return f.normalized;
    }

    public Ray GetAimRay() =>
        _cam ? _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)) : new Ray(transform.position, transform.forward);

    public float AimT => _aimT;
}
