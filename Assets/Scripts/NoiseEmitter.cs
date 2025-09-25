using UnityEngine;

public class NoiseEmitter : MonoBehaviour
{
    [SerializeField] private Transform emitPoint;   // dónde “suena” (si está vacío, usa este transform)
    [SerializeField] private float radius = 14f;
    [SerializeField] private float cooldown = 8f;


    private float _readyTime = 0f;

    // Método SIN parámetros para enganchar en UnityEvent
    public void EmitNoise()
    {
        if (Time.time < _readyTime) return;
        Debug.Log("Si");
        Vector3 pos = emitPoint ? emitPoint.position : transform.position;

        NoiseSystem.Emit(pos, radius);

        _readyTime = Time.time + cooldown;
    }

}
