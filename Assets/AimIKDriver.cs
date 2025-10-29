using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AimIKDriver : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] PistolAttack pistol;   // tu script actual (expone LastAimPoint)
    [SerializeField] Transform aimTarget;   // Empty asignado como Target del AimConstraint
    [SerializeField] Rig aimRig;            // el Rig que contiene el AimConstraint
    [SerializeField] LayerMask aimMask = ~0;
    [Header("Tuning")]
    [SerializeField] float followSpeed = 20f;     // rapidez con la que el AimTarget sigue el punto de mira
    [SerializeField] float rigBlendSpeed = 12f;   // rapidez con la que se activa/desactiva el Rig
    [SerializeField] float aimingWeight = 1f;     // peso del Rig al apuntar
    [SerializeField] float hipWeight = 0f;        // peso del Rig en reposo (0 = desactivado)
    [SerializeField] float verticalClamp = 45f;   // límite de inclinación hacia arriba/abajo

    void LateUpdate()
    {
        if (!pistol || !aimTarget || !aimRig) return;

        var cam = Camera.main;
        if (!cam) return;

        // 1) Ray estable desde la cámara
        float maxDistance = 60f;
        float sphereRadius = 0.15f;

        // Excluí Player y Weapon en el inspector (aimMask)
        Vector3 dest = cam.transform.position + cam.transform.forward * 20f;
        if (Physics.SphereCast(cam.transform.position, sphereRadius, cam.transform.forward, out var hit, maxDistance, aimMask))
            dest = hit.point;

        // 2) Clamp vertical relativo a la CÁMARA (no al player)
        Vector3 origin = transform.position;
        Vector3 dir = (dest - origin).normalized;
        Vector3 flatDir = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;

        float angle = Vector3.SignedAngle(flatDir, dir, cam.transform.right);
        angle = Mathf.Clamp(angle, -verticalClamp, verticalClamp);

        Quaternion pitchRot = Quaternion.AngleAxis(angle, cam.transform.right);
        Vector3 limitedDir = pitchRot * flatDir;

        float followDistance = hit.collider ? Vector3.Distance(origin, dest) : 20f;
        followDistance = Mathf.Max(followDistance, 2f); // evita “pegotearse” a distancias cortas
        Vector3 targetPos = origin + limitedDir * followDistance;

        // 3) Suavizado
        aimTarget.position = Vector3.Lerp(aimTarget.position, targetPos, Time.deltaTime * followSpeed);

        // 4) Blend del Rig
        bool aiming = Input.GetMouseButton(1);
        float desired = aiming ? aimingWeight : hipWeight;
        aimRig.weight = Mathf.MoveTowards(aimRig.weight, desired, Time.deltaTime * rigBlendSpeed);
    }


}
