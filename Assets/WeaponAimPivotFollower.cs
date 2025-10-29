//using UnityEngine;

//public class WeaponAimPivotFollower : MonoBehaviour
//{
//    [SerializeField] PistolAttack pistol;   // arrastrá tu PistolAttack
//    [SerializeField] Transform upRef;       // suele ser el pelvis o el world up (si dudas, dejá null)
//    [SerializeField] float turnSpeed = 18f; // suavizado

//    void LateUpdate()
//    {
//        if (!pistol) return;
//        Vector3 target = pistol.LastAimPoint;

//        // dirección hacia el punto de mira
//        Vector3 dir = (target - transform.position);
//        if (dir.sqrMagnitude < 1e-4f) return;

//        // up para la rotación
//        Vector3 up = (upRef ? upRef.up : Vector3.up);

//        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, up);
//        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
//    }
//}
