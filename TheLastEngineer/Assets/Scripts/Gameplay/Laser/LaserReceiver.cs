using System;
using UnityEngine;
using UnityEngine.Events;


[AddComponentMenu("Puzzles/Laser Receiver")]
[DisallowMultipleComponent]
public class LaserReceiver : MonoBehaviour, LaserBeamController.ILaserReactive
{
    #region Config
    [Header("Filtro de energía")]
    [SerializeField] private LaserBeamController.EnergyType _required = LaserBeamController.EnergyType.Any;

    [Header("Llenado / Vaciamiento")]
    [Tooltip("Segundos de exposición continua para activarse (1.0 de carga).")]
    [Min(0.01f)][SerializeField] private float _fillSeconds = 1.5f;
    [Tooltip("Segundos en vaciarse cuando deja de recibir láser.")]
    [Min(0.01f)][SerializeField] private float _drainSeconds = 0.75f;
    [Tooltip("Requiere estar activado continuamente (si se corta, se empieza a vaciar). Si se desactiva, no mantiene el estado.")]
    [SerializeField] private bool _requireContinuous = false;

    [Header("Chequeos opcionales")]
    [Tooltip("Si está activo, sólo acepta impactos de frente (dot >= minDot)")]
    [SerializeField] private bool _requireFrontHit = false;
    [Range(0f, 1f)][SerializeField] private float _minDot = 0.4f; // 1 = totalmente de frente

    [Tooltip("Si el payload AntiCorruption está activo, invierte la lógica (ej: drena en vez de llenar).")]
    [SerializeField] private bool _invertOnAntiCorruption = true;

    [Header("Eventos")]
    public UnityEvent<float> OnFillChanged; // 0..1
    public UnityEvent OnActivated; // al pasar de <1 a ==1
    public UnityEvent OnDeactivated; // al pasar de >0 a ==0 (si _requireContinuous) o al perder activación
    #endregion

    #region Runtime State
    private float _fill; // 0..1
    private bool _isLit; // ¿está recibiendo láser este frame?
    private bool _active; // ¿ya alcanzó 1.0 y está en estado ACTIVADO?
    private float _lastEventFill; // cache para evitar spam

    private Collider _collider; // opcional: para gizmos/front check precisos
    #endregion

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void Update()
    {
        // Update de carga
        var targetDelta = ComputeDelta();
        if (!Mathf.Approximately(targetDelta, 0f))
        {
            var prev = _fill;
            _fill = Mathf.Clamp01(_fill + targetDelta * Time.deltaTime);


            if (!Mathf.Approximately(prev, _fill))
            {
                OnFillChanged?.Invoke(_fill);
            }
        }


        // Cambios de estado
        if (!_active && _fill >= 1f - 1e-4f)
        {
            _active = true;
            OnActivated?.Invoke();
        }
        else if (_active)
        {
            if (_requireContinuous && !_isLit)
            {
                _active = false;
                OnDeactivated?.Invoke();
            }
            else if (!_requireContinuous && _fill <= 0f + 1e-4f)
            {
                _active = false;
                OnDeactivated?.Invoke();
            }
        }


        _isLit = false; // reset para el siguiente frame
    }
    private float ComputeDelta()
    {
        if (!_isLit)
        {
            // drenar
            if (_fill <= 0f) return 0f;
            return -(1f / Mathf.Max(_drainSeconds, 0.0001f));
        }


        // está iluminado: llenar
        return (1f / Mathf.Max(_fillSeconds, 0.0001f));
    }


    #region ILaserReactive
    public void OnLaserEnter(LaserBeamController.HitInfo info)
    {
        // Validaciones únicas al entrar (si hiciera falta). Por ahora, nada especial.
    }


    public void OnLaserStay(LaserBeamController.HitInfo info)
    {
        // Filtros: tipo de energía
        if (_required != LaserBeamController.EnergyType.Any && info.Payload.Type != _required)
            return;


        // Filtro: de frente
        if (_requireFrontHit)
        {
            var dot = Vector3.Dot(-info.Hit.normal, info.Direction.normalized);
            if (dot < _minDot) return;
        }


        // Anti-corruption: invertir si aplica
        if (_invertOnAntiCorruption && info.Payload.AntiCorruption)
        {
            // Si el láser es anti-corrupción, lo tratamos como "drena" (considerar como NO iluminado)
            // Alternativa: podrías hacer que reduzca más rápido (doble velocidad):
            _isLit = false;
            return;
        }


        // Aceptar iluminación este frame
        _isLit = true;
    }
    public void OnLaserExit()
    {
        // Nada específico: Update se encargará de drenar.
    }
    #endregion


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!_collider) _collider = GetComponent<Collider>();
        var c = Color.Lerp(Color.red, Color.green, _fill);
        c.a = 0.5f;
        Gizmos.color = c;
        if (_collider)
        {
            var bounds = _collider.bounds;
            Gizmos.DrawCube(bounds.center, bounds.size);
        }
        else
        {
            Gizmos.DrawSphere(transform.position, 0.25f);
        }
    }
#endif
}