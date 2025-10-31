using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AimIKDriver : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] PistolAttack pistol;
    [SerializeField] Transform aimTarget;
    [SerializeField] Rig aimRig;
    [SerializeField] LayerMask aimMask = ~0;
    [Header("Tuning")]
    [SerializeField] float followSpeed = 20f;
    [SerializeField] float rigBlendSpeed = 12f;
    [SerializeField] float aimingWeight = 1f;
    [SerializeField] float hipWeight = 0f;
    [SerializeField] float verticalClamp = 45f;

    void LateUpdate()
    {
        if (!pistol || !aimTarget || !aimRig) return;

        var cam = Camera.main;
        if (!cam) return;

        float maxDistance = 60f;
        float sphereRadius = 0.15f;

        Vector3 dest = cam.transform.position + cam.transform.forward * 20f;
        if (Physics.SphereCast(cam.transform.position, sphereRadius, cam.transform.forward, out var hit, maxDistance, aimMask))
            dest = hit.point;

        Vector3 origin = transform.position;
        Vector3 dir = (dest - origin).normalized;
        Vector3 flatDir = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;

        float angle = Vector3.SignedAngle(flatDir, dir, cam.transform.right);
        angle = Mathf.Clamp(angle, -verticalClamp, verticalClamp);

        Quaternion pitchRot = Quaternion.AngleAxis(angle, cam.transform.right);
        Vector3 limitedDir = pitchRot * flatDir;

        float followDistance = hit.collider ? Vector3.Distance(origin, dest) : 20f;
        followDistance = Mathf.Max(followDistance, 2f);
        Vector3 targetPos = origin + limitedDir * followDistance;

        aimTarget.position = Vector3.Lerp(aimTarget.position, targetPos, Time.deltaTime * followSpeed);

        bool aiming = Input.GetMouseButton(1);
        float desired = aiming ? aimingWeight : hipWeight;
        aimRig.weight = Mathf.MoveTowards(aimRig.weight, desired, Time.deltaTime * rigBlendSpeed);
    }


}
