using UnityEngine;

public interface IVisionProvider
{
    // Devuelve true si ve al jugador y entrega la posición vista (seenPos)
    bool TrySeePlayer(out Vector3 seenPos);
}