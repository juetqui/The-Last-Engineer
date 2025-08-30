using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Puzzles/Laser Beam Controller")]
[DisallowMultipleComponent]
public class LaserBeamController : MonoBehaviour
{
    #region Types
    [Serializable]
    public enum EnergyType { Any = 0, Blue = 1, Red = 2, Green = 3 }


    [Serializable]
    public struct Payload
    {
        public EnergyType Type; // Tipo de energía del láser (para receptores que filtren).
        public float Intensity; // 0..1 – útil para modulaciones futuras.
        public bool AntiCorruption; // Si está activo, los receptores pueden reaccionar distinto.


        public Payload(EnergyType type, float intensity, bool anti)
        {
            Type = type; Intensity = Mathf.Clamp01(intensity); AntiCorruption = anti;
        }
    }


    public struct HitInfo
    {
        public Vector3 Origin;
        public Vector3 Direction;
        public RaycastHit Hit;
        public Payload Payload;
    }


    public interface ILaserReactive
    {
        void OnLaserEnter(HitInfo info);
        void OnLaserStay(HitInfo info);
        void OnLaserExit();
    }
    #endregion

    [Header("Estado")]
    [SerializeField] private bool _enabledAtStart = true;

    [Header("Haz de luz")]
    [SerializeField] private float _maxDistance = 50f;
    [SerializeField, Min(0)] private int _maxBounces = 0;
    [SerializeField] private LayerMask _hitLayers = ~0;
    [SerializeField] private LineRenderer _line; // opcional

    [Header("Energía")]
    [SerializeField] private EnergyType _energyType = EnergyType.Blue;
    [Range(0f, 1f)][SerializeField] private float _intensity = 1f;
    [SerializeField] private bool _antiCorruption = false;

    [Header("Eventos (opcionales)")]
    public UnityEvent OnFirstHit; // Se dispara cuando el rayo comienza a tocar algo reactivo
    public UnityEvent OnLostAll; // Se dispara cuando deja de tocar cualquier cosa reactiva

    // --- Runtime
    private readonly HashSet<ILaserReactive> _currentReactives = new();
    private readonly HashSet<ILaserReactive> _frameReactives = new();
    private bool _isOn;

    private void Awake()
    {
        _isOn = _enabledAtStart;
        if (_line)
        {
            _line.positionCount = 0;
            _line.useWorldSpace = true;
        }
    }

    private void Update()
    {
        if (!_isOn)
        {
            RenderEmpty();
            // Salida a todos los receptores tocados el frame anterior
            if (_currentReactives.Count > 0)
            {
                foreach (var r in _currentReactives) r.OnLaserExit();
                _currentReactives.Clear();
                OnLostAll?.Invoke();
            }
            return;
        }

        TraceAndNotify();
    }


    #region Public API
    public void SetEnabled(bool value)
    {
        if (_isOn == value) return;
        _isOn = value;
        if (!value) RenderEmpty();
    }

    public void Toggle() => SetEnabled(!_isOn);
    public void SetAntiCorruption(bool active) => _antiCorruption = active;
    public void SetEnergy(EnergyType type, float intensity = 1f)
    {
        _energyType = type; _intensity = Mathf.Clamp01(intensity);
    }
    #endregion

    #region Core
    private void TraceAndNotify()
    {
        _frameReactives.Clear();


        var points = new List<Vector3>(1 + _maxBounces + 1);
        var origin = transform.position;
        var direction = transform.forward;


        points.Add(origin);
        var payload = new Payload(_energyType, _intensity, _antiCorruption);


        int bounces = 0;
        bool hitAnythingReactiveThisFrame = false;


        while (bounces <= _maxBounces)
        {
            if (!Physics.Raycast(origin, direction, out var hit, _maxDistance, _hitLayers, QueryTriggerInteraction.Collide))
            {
                // No golpea nada: extender la línea al máximo
                points.Add(origin + direction * _maxDistance);
                break;
            }


            points.Add(hit.point);


            // Notificar al objeto golpeado si es reactivo
            if (hit.collider.TryGetComponent<ILaserReactive>(out var reactive))
            {
                var info = new HitInfo { Origin = origin, Direction = direction, Hit = hit, Payload = payload };
                var isNew = _currentReactives.Add(reactive);
                _frameReactives.Add(reactive);
                if (isNew) reactive.OnLaserEnter(info);
                reactive.OnLaserStay(info);
                hitAnythingReactiveThisFrame = true;
            }


            // ¿Superficie reflectante?
            if (hit.collider.CompareTag("Mirror"))
            {
                // Rebote especular simple
                origin = hit.point + hit.normal * 0.001f; // pequeño offset para evitar re-golpe
                direction = Vector3.Reflect(direction, hit.normal);
                bounces++;
                continue;
            }


            // Si no hay rebote, el rayo termina aquí
            break;
        }

        // Exit para reactivos que NO fueron tocados este frame
        if (_currentReactives.Count > 0)
        {
            // Si nada fue tocado este frame y antes sí, disparar OnLostAll
            bool willLoseAll = hitAnythingReactiveThisFrame == false;


            var toRemove = new List<ILaserReactive>();
            foreach (var r in _currentReactives)
            {
                if (_frameReactives.Contains(r)) continue;
                r.OnLaserExit();
                toRemove.Add(r);
            }
            foreach (var r in toRemove) _currentReactives.Remove(r);


            if (willLoseAll) OnLostAll?.Invoke();
        }
        else if (hitAnythingReactiveThisFrame)
        {
            OnFirstHit?.Invoke();
        }

        Render(points);
    }
    #endregion

    #region Rendering
    private void Render(IReadOnlyList<Vector3> points)
    {
        if (!_line) return;
        _line.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
            _line.SetPosition(i, points[i]);
    }

    private void RenderEmpty()
    {
        if (_line) _line.positionCount = 0;
    }
    #endregion
}