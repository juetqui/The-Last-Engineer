using UnityEngine;
using PrimeTween;

/// <summary>
/// Base de las vistas que pulsan mientras la Connection espera un nodo y quedan encendidas fijas
/// al recibir el correcto. Las subclases solo dicen de donde leen y donde escriben el color.
///
/// El estado se aplica por evento Y por consulta en Start: asi no importa si Connection.Start
/// (que anuncia el nodo pre-asignado) corre antes o despues que el de esta vista.
/// </summary>
public abstract class ConnectionPulseView : MonoBehaviour
{
    [SerializeField] protected Connection _connection;
    [SerializeField] private Ease _tweenType = Ease.InOutSine;
    [SerializeField] private float _duration = 0.6f;

    // Un unico handle: antes el pulso se encadenaba por OnComplete y un cambio de estado a mitad
    // de camino dejaba dos tweens escribiendo el mismo color (parpadeo).
    private Tween _tween;
    private Color _onColor;

    protected abstract Color OffColor { get; }
    protected abstract Color CurrentColor { get; }
    protected abstract Color OnColorFor(GlitchState level);
    protected abstract void ApplyColor(Color color);
    protected abstract void CacheComponents();

    // Suscripcion en Awake: todos los Awake corren antes que cualquier Start, asi que el evento
    // que dispara Connection.Start con un nodo pre-asignado siempre nos encuentra escuchando.
    private void Awake()
    {
        CacheComponents();
        _onColor = OnColorFor(_connection.RequiredLevel);
        _connection.OnNodeConnected += OnNodeConnected;
    }

    private void Start()
    {
        ApplyState(_connection.IsCorrectlyConnected);
    }

    private void OnDestroy()
    {
        _tween.Stop();

        if (_connection != null)
            _connection.OnNodeConnected -= OnNodeConnected;
    }

    private void OnNodeConnected(GlitchState _, bool connected) => ApplyState(connected);

    // Idempotente: aplicarlo dos veces (evento + Start) solo reinicia la transicion al mismo destino.
    private void ApplyState(bool keepOn)
    {
        _tween.Stop();
        _tween = Tween.Custom(this, CurrentColor, _onColor, _duration, (view, c) => view.ApplyColor(c), _tweenType);

        if (!keepOn)
            _tween.OnComplete(this, view => view.StartPulse(), warnIfTargetDestroyed: false);
    }

    private void StartPulse()
    {
        _tween = Tween.Custom(this, _onColor, OffColor, _duration, (view, c) => view.ApplyColor(c), _tweenType,
            cycles: -1, cycleMode: CycleMode.Yoyo);
    }
}
