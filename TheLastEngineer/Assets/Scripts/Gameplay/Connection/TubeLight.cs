using System;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TubeLight : MonoBehaviour
{
    [Header("Opcional: fuente de conexión")]
    [SerializeField] private Connection _connection;
    [SerializeField] private NodeType _requiredNode = NodeType.Default;

    [Header("Animación")]
    [SerializeField, Min(0.01f)] private float _speed = 10f; // unidades de step por segundo

    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;
    private static readonly int StepProp = Shader.PropertyToID("_Step");

    private enum Mode { Idle, TurningOn, TurningOff }
    private Mode _mode = Mode.Idle;

    private float _step;        // 0..1
    private bool _connected;    // estado lógico pedido desde afuera

    public event Action OnFilled;   // se dispara cuando llega a 1
    public event Action OnEmptied;  // se dispara cuando llega a 0

    public bool IsFull => Mathf.Approximately(_step, 1f);
    public bool IsEmpty => Mathf.Approximately(_step, 0f);

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
        ApplyStep();
    }

    private void OnEnable()
    {
        if (_connection != null)
            _connection.OnNodeConnected += OnNodeConnected;
    }

    private void OnDisable()
    {
        if (_connection != null)
            _connection.OnNodeConnected -= OnNodeConnected;
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
            if (finishedOn) OnFilled?.Invoke();
            else OnEmptied?.Invoke();
        }
    }

    private void ApplyStep()
    {
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(StepProp, _step);
        _renderer.SetPropertyBlock(_mpb);
    }

    // --- Integración con Connection (opcional) ---
    private void OnNodeConnected(NodeType nodeType, bool connected)
    {
        if (nodeType != _requiredNode) return;
        if (connected) TurnOn();
        else TurnOff();
    }

    // --- API pública sincrónica ---
    public void TurnOn() { _connected = true; _mode = Mode.TurningOn; }
    public void TurnOff() { _connected = false; _mode = Mode.TurningOff; }

}
