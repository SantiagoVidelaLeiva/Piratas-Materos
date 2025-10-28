using UnityEngine;

public class WeaponMount : MonoBehaviour
{
    [SerializeField] Transform weaponSocketR;   // mano derecha
    [SerializeField] GameObject weaponPrefab;
    [SerializeField] Animator anim;
    [Range(0, 1)] public float leftIKWeight = 1f;

    GameObject currentWeapon;
    [SerializeField] Transform gripL;
    bool _dropped;

    CharacterHealth _health;  // para escuchar OnDied
    [SerializeField] Transform leftHandBone;
    [SerializeField] Transform rightHandBone;

    void Awake()
    {
        _health = GetComponentInParent<CharacterHealth>();
        if (_health) _health.OnDied += HandleDeath;  // soltar arma al morir
        if (anim && anim.isHuman)
        {
            if (!leftHandBone) leftHandBone = anim.GetBoneTransform(HumanBodyBones.LeftHand);
            if (!rightHandBone) rightHandBone = anim.GetBoneTransform(HumanBodyBones.RightHand);
        }
    }
    void OnDestroy()
    {
        if (_health) _health.OnDied -= HandleDeath;
    }

    void Start()
    {
        Equip(weaponPrefab);
    }

    public void Equip(GameObject prefab)
    {
        if (!prefab || !weaponSocketR || !anim) { Debug.LogError("Refs nulas"); return; }
        if (currentWeapon) Destroy(currentWeapon);

        _dropped = false;

        currentWeapon = Instantiate(prefab, weaponSocketR, false);
        var t = currentWeapon.transform;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        if (!gripL)
        {
            var maybe = t.Find("Grip_L");
            if (maybe) gripL = maybe;
            else Debug.LogWarning("No se encontró Grip_L en el arma.");
        }
    }

    void HandleDeath()
    {
        ResetHandRotations();
        DropWeapon();          // 🔹 des-parenta + física
        leftIKWeight = 0f;     // 🔹 apagá IK para que la mano no tire
        anim.enabled = false;
        // Si usás Animation Rigging, apagalo también en tu RagdollController (como te pasé antes).
    }
    void ResetHandRotations()
    {
        if (leftHandBone) leftHandBone.localRotation = Quaternion.identity;
        if (rightHandBone) rightHandBone.localRotation = Quaternion.identity;
    }

    public void DropWeapon()
    {
        if (_dropped || !currentWeapon) return;
        _dropped = true;

        // 1) Des-parentar
        var wt = currentWeapon.transform;
        wt.SetParent(null, true);

        // 2) Asegurar física en el arma
        var col = currentWeapon.GetComponent<Collider>();
        //if (!col) col = currentWeapon.AddComponent<BoxCollider>(); // o el que corresponda
        col.enabled = true;

        var rb = currentWeapon.GetComponent<Rigidbody>();
        //if (!rb) rb = currentWeapon.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // 3) Empujoncito suave para que no choque contra el torso
        var forward = transform.forward;
        rb.AddForce(forward * 2f, ForceMode.Impulse);

        // (Opcional) Ignorar colisiones arma↔cuerpo un ratito para evitar torques raros
        StartCoroutine(IgnoreCollisionsWithOwnerFor(0.5f, col));
    }

    System.Collections.IEnumerator IgnoreCollisionsWithOwnerFor(float seconds, Collider weaponCol)
    {
        if (!weaponCol) yield break;
        var ownerCols = GetComponentsInParent<Collider>(includeInactive: true);
        foreach (var c in ownerCols) if (c && c.enabled) Physics.IgnoreCollision(weaponCol, c, true);
        yield return new WaitForSeconds(seconds);
        foreach (var c in ownerCols) if (c && c.enabled) Physics.IgnoreCollision(weaponCol, c, false);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!anim || gripL == null || _dropped) return;  // 🔹 si se dropeó, no aplicar IK
        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftIKWeight);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftIKWeight);
        anim.SetIKPosition(AvatarIKGoal.LeftHand, gripL.position);
        anim.SetIKRotation(AvatarIKGoal.LeftHand, gripL.rotation);
    }
}
