using System;
using UnityEngine;

public static class NoiseSystem
{
    // Publisher/ Subscriber - Patron de diseño parecido al observer 
    public static event Action<Vector3, float> OnNoise; // Las clases se subscriben aca
    public static void Emit(Vector3 worldPos, float radius) => OnNoise?.Invoke(worldPos, radius); // Otras clases tocan el "timbre" y ejecutan los metodos 
}