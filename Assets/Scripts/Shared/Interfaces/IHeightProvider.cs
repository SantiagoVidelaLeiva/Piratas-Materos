using UnityEngine;

public interface IHeightProvider
{
    // Devuelve un float que indica la altura de mi personaje con respecto a sus ojos
    float GetEyeHeight();
}
