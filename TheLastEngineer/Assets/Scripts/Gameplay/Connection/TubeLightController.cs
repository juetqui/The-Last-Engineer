using System.Collections.Generic;
using UnityEngine;

public class TubeLightController : MonoBehaviour
{
    [SerializeField] private bool _autoReverseOnComplete = true; // invierte la lista al completar una corrida
    [SerializeField] private List<TubeLight> _tubes = new List<TubeLight>();
    private Connection _connection;

    public int Index => _index;
    private int _index = -1;
    private bool _isRunning = false;
    private bool _currentDesiredFill = true; // true=llenar/encender, false=vaciar/apagar
    private int _runId = 0;     // Para evitar callbacks "viejos" si reinicias la corrida en mitad de otra.

    private void OnEnable()
    {
        if (_connection != null)
            _connection.OnNodeConnected += OnConnectionStateChanged;
    }

    private void Start()
    {
        _connection = GetComponent<Connection>();
        if (_connection != null)
            _connection.OnNodeConnected += OnConnectionStateChanged;
        // Opcional: si tu Connection puede arrancar ya conectada, podés disparar la corrida al inicio.
        // if (_connection != null && _connection.StartsConnected)
        //     StartSequence(fill: true);
    }

    private void OnConnectionStateChanged(NodeType type, bool connected)
    {
        StartSequence(fill: connected);
    }

    public void StartSequence(bool fill)
    {
        if (_tubes == null || _tubes.Count == 0)
        {
            Debug.LogWarning($"{nameof(TubeLightController)}: Lista de tubos vacía.", this);
            return;
        }

        // Cancelar corrida previa y protegernos de eventos rezagados
        _runId++;
        _isRunning = true;
        _currentDesiredFill = fill;

        // Siempre arrancamos desde el primer elemento de la lista actual
        UnsubscribeFromCurrentTube();
        _index = 0;
        SubscribeAndPlayCurrent(_runId);
    }

    private void SubscribeAndPlayCurrent(int expectedRunId)
    {
        if (!_isRunning) return;

        if (_index < 0 || _index >= _tubes.Count)
        {
            // Seguridad: si por algún motivo el índice está fuera, consideramos corrida terminada.
            FinishRun(expectedRunId);
            return;
        }

        TubeLight tube = _tubes[_index];
        if (tube == null)
        {
            Debug.LogWarning($"{nameof(TubeLightController)}: TubeLight en índice {_index} es null. Se salta.", this);
            _index++;
            SubscribeAndPlayCurrent(expectedRunId);
            return;
        }

        // Nos aseguramos de no duplicar suscripciones
        tube.OnTransitionCompleted -= HandleTubeCompleted;
        tube.OnTransitionCompleted += HandleTubeCompleted;

        // Disparamos la acción del tubo actual
        tube.PlayFill(_currentDesiredFill);
    }

    private void HandleTubeCompleted(TubeLight tube)
    {
        if (!_isRunning) return;

        // Desuscribir del tubo recién completado
        if (tube != null)
            tube.OnTransitionCompleted -= HandleTubeCompleted;

        // Avanzar al siguiente
        _index++;

        if (_index >= _tubes.Count)
            FinishRun(_runId); // Terminó la pasada
        else
            SubscribeAndPlayCurrent(_runId); // Continuar cadena
    }

    /// <summary>
    /// Termina la corrida actual; invierte la lista si está habilitado.
    /// </summary>
    private void FinishRun(int expectedRunId)
    {
        if (!_isRunning) return;

        // Solo el run "vigente" puede finalizarse a sí mismo
        if (expectedRunId != _runId) return;

        _isRunning = false;
        UnsubscribeFromCurrentTube();

        if (_autoReverseOnComplete && _tubes != null && _tubes.Count > 1)
            _tubes.Reverse();

        // Queda listo para una próxima señal de la Connection.
        _index = -1;
    }

    private void UnsubscribeFromCurrentTube()
    {
        if (_index >= 0 && _index < (_tubes?.Count ?? 0))
        {
            var t = _tubes[_index];
            if (t != null)
                t.OnTransitionCompleted -= HandleTubeCompleted;
        }
    }

    private void OnDisable()
    {
        if (_connection != null)
            _connection.OnNodeConnected -= OnConnectionStateChanged;

        UnsubscribeFromCurrentTube();
        _isRunning = false;
    }
}