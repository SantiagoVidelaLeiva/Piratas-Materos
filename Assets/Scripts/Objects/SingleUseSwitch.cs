using System.Diagnostics;
using UnityEngine;

// Script para un interactuable de un solo uso
public class SingleUseSwitch : MonoBehaviour, IInteractable
{
    private bool _hasBeenUsed = false;

    [SerializeField] private string _interactPrompt = "Press E to use";

    public event System.Action OnActivated;

    public string InteractPrompt => _interactPrompt;

    public bool Interact()
    {
        if (_hasBeenUsed)
        {
            return true;
        }

        _hasBeenUsed = true;

        UnityEngine.Debug.Log("Single Use Switch has been activated!");

        OnActivated?.Invoke();


        return true;
    }
}