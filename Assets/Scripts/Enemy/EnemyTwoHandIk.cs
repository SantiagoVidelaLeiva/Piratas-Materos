using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyTwoHandIK : MonoBehaviour
{
    public EnemyWeaponHolder holder;   // referencia al holder (mismo GameObject o en el root)
    public Transform leftElbowHint;    // opcional: ayuda a estabilizar el codo
    public float blendSpeed = 12f;     // suaviza el peso del IK

    private Animator anim;
    private Transform leftGrip;
    private float ikWeight;            // blending suave 0..1

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (!holder) holder = GetComponent<EnemyWeaponHolder>();
    }

    // Llamar después de instanciar el arma (el holder lo llama en GiveWeapon)
    public void CacheLeftGrip()
    {
        leftGrip = holder ? holder.CurrentLeftGrip : null;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!anim) return;

        // Podés condicionar por estado/Layer (ej: solo cuando "Rifle" está activo)
        bool useIK = leftGrip != null;

        // Lerp de pesos para evitar pops
        float target = useIK ? 1f : 0f;
        ikWeight = Mathf.MoveTowards(ikWeight, target, blendSpeed * Time.deltaTime);

        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);

        if (leftElbowHint)
        {
            anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, ikWeight);
            anim.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowHint.position);
        }

        if (useIK)
        {
            anim.SetIKPosition(AvatarIKGoal.LeftHand, leftGrip.position);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, leftGrip.rotation);
        }
    }
}