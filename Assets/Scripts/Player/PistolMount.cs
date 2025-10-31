using UnityEngine;

public class PistolMount : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform weaponSocket;
    [SerializeField] GameObject weaponPrefab;

    GameObject currentWeapon;

    void Start()
    {
        EquipWeapon(weaponPrefab);
    }

    public void EquipWeapon(GameObject prefab)
    {
        if (!weaponSocket || !prefab)
        {
            Debug.LogWarning("WeaponMount: faltan referencias.");
            return;
        }

        if (currentWeapon)
            Destroy(currentWeapon);

        currentWeapon = Instantiate(prefab, weaponSocket);

        Transform socketRH = currentWeapon.transform.Find("SocketRH");

        if (socketRH)
        {
            Vector3 offsetPos = currentWeapon.transform.position - socketRH.position;
            currentWeapon.transform.position += offsetPos;

            Quaternion offsetRot = Quaternion.Inverse(socketRH.rotation) * weaponSocket.rotation;
            currentWeapon.transform.rotation = currentWeapon.transform.rotation * offsetRot;
        }
        else
        {
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;
        }
    }
}