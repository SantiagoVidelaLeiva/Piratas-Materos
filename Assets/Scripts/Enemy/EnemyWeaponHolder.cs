using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyWeaponHolder : MonoBehaviour
{
    public Transform weaponSocket;       // hijo del RightHand (opcional; se resuelve si es null)
    public GameObject riflePrefab;

    private GameObject rifleInstance;
    private Transform leftGrip;
    private Animator animator;

    public Transform CurrentLeftGrip => leftGrip;   // <-- expuesto para el IK

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (!weaponSocket && animator)
            weaponSocket = animator.GetBoneTransform(HumanBodyBones.RightHand);
    }

    void Start()
    {
        GiveWeapon();   // instanciar al inicio (o llamalo desde afuera cuando equipe)
    }

    public void GiveWeapon()
    {
        if (!weaponSocket || !riflePrefab) return;

        if (rifleInstance) Destroy(rifleInstance);

        rifleInstance = Instantiate(riflePrefab, weaponSocket);
        rifleInstance.transform.localPosition = Vector3.zero;
        rifleInstance.transform.localRotation = Quaternion.identity;

        leftGrip = rifleInstance.transform.Find("Grip_L");
        // Avisar a quien haga IK que ya existe el grip:
        GetComponent<EnemyTwoHandIK>()?.CacheLeftGrip();
    }
}