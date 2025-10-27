using UnityEngine;

public class PistolMount : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform weaponSocket;  // el WeaponSocket del player
    [SerializeField] GameObject weaponPrefab; // tu prefab del arma (con SocketRH)

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

        // Si ya hay un arma, la borramos
        if (currentWeapon)
            Destroy(currentWeapon);

        // Instanciamos el arma
        currentWeapon = Instantiate(prefab, weaponSocket);

        // Buscamos el SocketRH dentro del arma
        Transform socketRH = currentWeapon.transform.Find("SocketRH");

        if (socketRH)
        {
            // Reposicionamos el arma para que el SocketRH coincida con el WeaponSocket
            Vector3 offsetPos = currentWeapon.transform.position - socketRH.position;
            currentWeapon.transform.position += offsetPos;

            // Alineamos la rotación también
            Quaternion offsetRot = Quaternion.Inverse(socketRH.rotation) * weaponSocket.rotation;
            currentWeapon.transform.rotation = currentWeapon.transform.rotation * offsetRot;
        }
        else
        {
            // fallback: si no hay SocketRH, simplemente lo pegamos directo
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;
        }
    }
}