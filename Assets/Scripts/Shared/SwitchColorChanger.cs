using System;
using System.Diagnostics;
using UnityEngine;

public class SwitchColorChanger : MonoBehaviour
{
    [SerializeField] private SingleUseSwitch _switch;
    [SerializeField] private ChangeColor _objectToColor;
    [SerializeField] private Color _colorToSet = Color.green;

    private void Awake()
    {
        // Asegura que las referencias existan.
        if (_switch == null)
        {
            UnityEngine.Debug.LogError("Referencia a SingleUseSwitch no encontrada. " +
                           "Por favor, asignala en el Inspector.", this);
            return;
        }

        if (_objectToColor == null)
        {
            UnityEngine.Debug.LogError("Referencia a ChangeColor no encontrada. " +
                           "Por favor, asignala en el Inspector.", this);
            return;
        }

        _switch.OnActivated += OnSwitchActivated;
    }

    private void OnDestroy()
    {
        if (_switch != null)
        {
            _switch.OnActivated -= OnSwitchActivated;
        }
    }

    private void OnSwitchActivated()
    {
        _objectToColor.SetColor(_colorToSet);

        this.enabled = false;
    }
}