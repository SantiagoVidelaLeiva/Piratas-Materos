using UnityEngine;

public interface IVisionProvider
{
    // Devuelve true si ve al jugador y entrega la posici�n vista (seenPos)
    bool TrySeePlayer(out Vector3 seenPos);
}