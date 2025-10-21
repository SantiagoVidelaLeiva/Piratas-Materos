using UnityEngine;

public class WeaponMount : MonoBehaviour
{
    [SerializeField] Transform weaponSocketR;   // RightHand socket
    [SerializeField] GameObject weaponPrefab;
    [SerializeField] Animator anim;            // arrastrá el Animator del personaje
    [Range(0, 1)] public float leftIKWeight = 1f;

    GameObject currentWeapon;
    [SerializeField] Transform gripL;

    void Start()
    {
        Equip(weaponPrefab);
    }

    public void Equip(GameObject prefab)
    {
        if (!prefab || !weaponSocketR || !anim) { Debug.LogError("Refs nulas"); return; }
        if (currentWeapon) Destroy(currentWeapon);

        currentWeapon = Instantiate(prefab, weaponSocketR, false);
        var t = currentWeapon.transform;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        // Importante: poné un Empty llamado "Grip_L" en el prefab del arma
        //gripL = t.Find("Grip_L");
        if (!gripL) Debug.LogWarning("No se encontró Grip_L en el arma.");
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!anim || gripL == null) return;

        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftIKWeight);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftIKWeight);
        anim.SetIKPosition(AvatarIKGoal.LeftHand, gripL.position);
        anim.SetIKRotation(AvatarIKGoal.LeftHand, gripL.rotation);
    }
}