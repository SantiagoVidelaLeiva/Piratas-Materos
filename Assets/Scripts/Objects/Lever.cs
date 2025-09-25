using System.Diagnostics;
using UnityEngine;

public class Lever : MonoBehaviour, IInteractable
{
    // Event to notify when the lever's state changes.
    public event System.Action<bool> OnLeverStateChange;

    private bool _isActivated = false;

    [SerializeField] private string _interactPrompt = "Press E to use ";

    public string InteractPrompt => _interactPrompt;

    // Defines the specific functionality of this object.
    public bool Interact()
    {
        _isActivated = !_isActivated; // Toggles the state
        UnityEngine.Debug.Log("Lever is now " + (_isActivated ? "activated" : "deactivated"));

        OnLeverStateChange?.Invoke(_isActivated);

        return false;

    }
}

