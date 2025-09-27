using System;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TubeLight : MonoBehaviour
{
    private float _speed = 7; // unidades de step por segundo
    private float _initialStep = 0f;
    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;
    private static readonly int StepProp = Shader.PropertyToID("_Step");
    private float _step;        // 0..1
    private bool _connected;    // último pedido lógico
    private Mode _mode = Mode.Idle;
    private enum Mode { Idle, TurningOn, TurningOff }

    // ---- EVENTOS (compat + controller) ----
    public event Action OnFilled;              // se dispara cuando llega a 1
    public event Action OnEmptied;             // se dispara cuando llega a 0
    public event Action<TubeLight> OnTransitionCompleted; // unificado para el controller

    public bool IsFull => Mathf.Approximately(_step, 1f);
    public bool IsEmpty => Mathf.Approximately(_step, 0f);

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
        _step = Mathf.Clamp01(_initialStep);
        ApplyStep();
    }

    private void Update()
    {
        if (_mode == Mode.Idle) return;

        float target = _mode == Mode.TurningOn ? 1f : 0f;
        _step = Mathf.MoveTowards(_step, target, _speed * Time.deltaTime);
        ApplyStep();

        if (Mathf.Approximately(_step, target))
        {
            var finishedOn = _mode == Mode.TurningOn;
            _mode = Mode.Idle;

            // Compatibilidad con tus eventos previos
            if (finishedOn) OnFilled?.Invoke();
            else OnEmptied?.Invoke();

            // Evento unificado para el controller
            OnTransitionCompleted?.Invoke(this);
        }
    }

    private void ApplyStep()
    {
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(StepProp, _step);
        _renderer.SetPropertyBlock(_mpb);
    }

    // --- API pública sincrónica para el CONTROLLER ---
    /// <summary>
    /// Inicia transición. fill=true (llenar/encender), false (vaciar/apagar).
    /// Si ya está en el estado objetivo, dispara eventos inmediatamente.
    /// </summary>
    public void PlayFill(bool fill)
    {
        // Si ya está como se pide, notificar al instante para no trabar la cadena
        if (fill && IsFull)
        {
            OnFilled?.Invoke();
            OnTransitionCompleted?.Invoke(this);
            return;
        }
        if (!fill && IsEmpty)
        {
            OnEmptied?.Invoke();
            OnTransitionCompleted?.Invoke(this);
            return;
        }

        if (fill) TurnOn();
        else TurnOff();
    }

    public void TurnOn()
    {
        _connected = true;
        _mode = Mode.TurningOn;
    }

    public void TurnOff()
    {
        _connected = false;
        _mode = Mode.TurningOff;
    }

    public void SetInstant(bool fill)
    {
        // Setea instantáneamente el estado (útil para inicializar sin animación).
        _mode = Mode.Idle;
        _connected = fill;
        _step = fill ? 1f : 0f;
        ApplyStep();
    }
}