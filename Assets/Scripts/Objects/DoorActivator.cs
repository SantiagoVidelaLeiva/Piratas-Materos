using System.Diagnostics;
using UnityEngine;

public class DoorActivator : MonoBehaviour
{
    [SerializeField] private SingleUseSwitch[] _requiredSwitches;

    [Header("Configuracion de la Puerta")]
    [SerializeField] private Transform _doorObject;
    [SerializeField] private Vector3 _openPosition;
    [SerializeField] private float _openSpeed = 1f;

    private Vector3 _startPosition;
    private bool _isOpening = false;
    private int _activatedSwitchesCount = 0;

    private void Awake()
    {
        _startPosition = _doorObject.localPosition;

        if (_requiredSwitches.Length == 0)
        {
            UnityEngine.Debug.LogError("Referencia a la palanca (SingleUseSwitch) no encontrada. " +
                           "Asignala en el Inspector.", this);
            return;
        }

        foreach (SingleUseSwitch s in _requiredSwitches)
        {
            s.OnActivated += OnSwitchActivated;
        }
    }

    private void Update()
    {
        if (_isOpening)
        {
            _doorObject.localPosition = Vector3.MoveTowards(_doorObject.localPosition, _openPosition, _openSpeed * Time.deltaTime);
            if (_doorObject.localPosition == _openPosition)
            {
                _isOpening = false;
            }
        }
    }

    private void OnSwitchActivated()
    {
        _activatedSwitchesCount++;

        if (_activatedSwitchesCount >= _requiredSwitches.Length)
        {
            UnityEngine.Debug.Log("Door activated by all switches!");
            _isOpening = true;

            foreach (SingleUseSwitch s in _requiredSwitches)
            {
                s.OnActivated -= OnSwitchActivated;
            }
        }
    }

    private void OnDestroy()
    {
        if (_activatedSwitchesCount < _requiredSwitches.Length)
        {
            foreach (SingleUseSwitch s in _requiredSwitches)
            {
                if (s != null)
                {
                    s.OnActivated -= OnSwitchActivated;
                }
            }
        }
    }
}