using System.Collections.Generic;
using UnityEngine;

public class TubeLightSequence : MonoBehaviour
{
    [Header("Orden de tubos a encadenar")]
    [SerializeField] private List<TubeLight> _tubes = new List<TubeLight>();

    [Header("Opcional: disparar por una Connection")]
    [SerializeField] private Connection _connection;
    [SerializeField] private NodeType _requiredNode = NodeType.Default;

    private int _index;           // índice actual en la cadena
    private bool _isTurningOn;    // estamos encendiendo (true) o apagando (false)

    private void OnEnable()
    {
        foreach (var t in _tubes)
        {
            if (t == null) continue;
            t.OnFilled += HandleTubeFilled;
            t.OnEmptied += HandleTubeEmptied;
        }

        if (_connection != null)
            _connection.OnNodeConnected += OnNodeConnected;
    }

    private void OnDisable()
    {
        foreach (var t in _tubes)
        {
            if (t == null) continue;
            t.OnFilled -= HandleTubeFilled;
            t.OnEmptied -= HandleTubeEmptied;
        }

        if (_connection != null)
            _connection.OnNodeConnected -= OnNodeConnected;
    }

    // Si querés disparar manualmente (sin Connection):
    [ContextMenu("Start Turn On Sequence")]
    public void StartTurnOnSequence()
    {
        _isTurningOn = true;
        _index = 0;
        if (_tubes.Count > 0) _tubes[0].TurnOn();
    }

    [ContextMenu("Start Turn Off Sequence")]
    public void StartTurnOffSequence()
    {
        _isTurningOn = false;
        _index = _tubes.Count - 1;
        if (_tubes.Count > 0) _tubes[_index].TurnOff();
    }

    // Integración con Connection (opcional)
    private void OnNodeConnected(NodeType nodeType, bool connected)
    {
        if (nodeType != _requiredNode) return;
        if (connected) StartTurnOnSequence();
        else StartTurnOffSequence();
    }

    // Callback: se terminó de llenar el tubo actual => avanzamos al siguiente
    private void HandleTubeFilled()
    {
        if (!_isTurningOn) return;               // ignorar si estamos en modo apagar
        if (_index >= _tubes.Count) return;

        var current = _tubes[_index];
        if (!current.IsFull) return;             // seguridad

        _index++;
        if (_index < _tubes.Count)
        {
            _tubes[_index].TurnOn();             // dispara el siguiente recién aquí
        }
        // si no, fin de la secuencia de encendido
    }

    // Callback: se vació el tubo actual => retrocedemos al anterior
    private void HandleTubeEmptied()
    {
        if (_isTurningOn) return;                // ignorar si estamos en modo encender
        if (_index < 0 || _tubes.Count == 0) return;

        var current = _tubes[_index];
        if (!current.IsEmpty) return;            // seguridad

        _index--;
        if (_index >= 0)
        {
            _tubes[_index].TurnOff();            // dispara el anterior recién aquí
        }
        // si no, fin de la secuencia de apagado
    }
}
