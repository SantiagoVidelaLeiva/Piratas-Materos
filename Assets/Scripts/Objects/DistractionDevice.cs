using UnityEngine;

public class DistractionDevice : MonoBehaviour
{
    [SerializeField] private float noiseRadius = 12f;
    private bool _hasCollided = false;
    private void Start()
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Stone"), LayerMask.NameToLayer("Player"), true);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (_hasCollided) return; // si ya chocó antes, salimos

        _hasCollided = true;
        NoiseSystem.Emit(transform.position, noiseRadius);
        Destroy(gameObject, 5f);
    }
}