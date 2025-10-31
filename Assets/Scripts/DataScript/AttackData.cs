using UnityEngine;

[CreateAssetMenu(
    fileName = "NewAttackData",
    menuName = "AttackData")]
public class AttackData : ScriptableObject
{
    [Header("Identidad")]
    public string attackName = "Ranged Attack";

    [Header("Daño")]
    public float baseDamage = 30f;

    [Header("Ritmo / Cadencia")]
    public float fireRate = 3f;
    public float cooldown = 0.5f;

    [Header("Alcance (Ranged)")]
    public float maxRange = 8f;
    public float spreadDegrees = 2.5f;

    [Header("Alcance (Melee)")]
    public float meleeRadius = 1.3f;

    [Header("FX / Feedback")]
    public AudioClip attackSFX;
    public GameObject muzzleFlashPrefab;
    public float muzzleFlashLife = 0.1f;
}
