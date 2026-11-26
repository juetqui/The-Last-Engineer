using PrimeTween;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int _maxDist = 100;
    [SerializeField] private float _offsetZ = 1f;
    [SerializeField] private float _easeTime = 0.5f;
    [SerializeField] private bool _startsInitialized = true;
    [SerializeField] private bool _debug = false;
    [SerializeField] private Color _gizmosColor = Color.red;

    // Turning on, the light comes up first and the beam follows; turning off, the beam retracts first
    // and the light follows. Optional: without a Light the beam toggles right away.
    [Header("Light")]
    [SerializeField] private Light _light;
    [SerializeField] private Ease _lightEaseType = Ease.InOutSine;
    [SerializeField] private float _lightEaseTime = 0.5f;
    [SerializeField] private float _lightMaxIntensity = 1f;

    private const float LightOffIntensity = 0f;

    private Tween _lightTween;
    private float _lightTarget = LightOffIntensity;
    // Set when the beam starts retracting: the light goes off once the retraction is over.
    private bool _lightOffPending = false;

    private LaserModel _model;
    private LaserView _view;

    private bool _isInitialized = false;
    private bool _isIdle = false;
    private bool _isTransitioning = false;
    private bool _isToggling = false;
    private bool _idleSetupDone = false;

    // Beam length of the previous frame, to tell when it has stopped changing. NoDistance forces
    // the next frame to count as a change.
    private const float NoDistance = -1f;
    private float _lastDist = NoDistance;

    private ILaserReceptor _ownReceptor;
    private Glitcheable _glitcheable;
    private Intangible _intangible;
    private GlitchComponent _glitch;

    public bool StartsInitialized => _startsInitialized;

    // What receptors validate against. A laser without a GlitchComponent above it counts as Clean.
    public GlitchState EmitterState => _glitch != null ? _glitch.CurrentState : GlitchState.Clean;

    // La máscara es la misma para todos los láseres: sale de Resources/LaserData.asset.
    private static LayerMask Layer => LaserData.Default.layer;

    // Buffer compartido para el raycast del láser intangible (evita allocs por frame).
    private static readonly RaycastHit[] _hitBuffer = new RaycastHit[16];

    private void Awake()
    {
        _model = new LaserModel(_maxDist, Layer, _easeTime, _debug);
        _view = GetComponent<LaserView>();

        _view.Init(1);
        _ownReceptor = GetComponentInParent<ILaserReceptor>();
        _glitcheable = GetComponentInParent<Glitcheable>();
        _intangible = GetComponentInParent<Intangible>();
        _glitch = GetComponentInParent<GlitchComponent>();

        if (_glitcheable != null)
            _glitcheable.FSM.OnStateChanged += UpdateGlitchedBehaviour;
        
        _isInitialized = _startsInitialized;
    }

    private void Start()
    {
        // GlitchComponent never raises its initial state: pull it here, then follow the changes.
        ApplyEmissionColor(EmitterState);
        if (_glitch != null) _glitch.OnGlitchStateChanged += ApplyEmissionColor;

        _lightTarget = _isInitialized ? _lightMaxIntensity : LightOffIntensity;
        if (_light != null) _light.intensity = _lightTarget;
    }

    private void OnDestroy()
    {
        _lightTween.Stop();

        if (_glitcheable != null)
            _glitcheable.FSM.OnStateChanged -= UpdateGlitchedBehaviour;

        if (_glitch != null) _glitch.OnGlitchStateChanged -= ApplyEmissionColor;
    }

    private void ApplyEmissionColor(GlitchState state)
    {
        _view.SetEmissionColor(GlitchPalette.Default.EmissionFor(state));
    }

    private void Update()
    {
        if (_glitcheable != null)
            CheckGlitchedBehaviour();
        else
            CheckCommonBehaviour();
    }

    private void CheckGlitchedBehaviour()
    {
        if (_isIdle) IdleBehaviour();
        else if (_isTransitioning) TransitioningBehaviour();
    }

    private void UpdateGlitchedBehaviour(IState newState)
    {
        (_isIdle, _isTransitioning) = newState switch
        {
            GlitchReintegratingState or GlitchIdleState => (true, false),
            GlitchDisintegratingState or GlitchMovingState => (false, true),
            _ => (false, false)
        };

        _isToggling = false;
        _idleSetupDone = false;
    }

    private void IdleBehaviour()
    {
        if (!_isInitialized)
        {
            if (!_isToggling)
            {
                FinishPendingLightOff();
                return;
            }

            _model.UpdateRaycastDistance();
            var origin = GetLaserOrigin();
            var dir = transform.forward;
            _view.SetLaserPositions(origin, origin + dir * _model.CurrentDist);

            if (_model.IsTransitioning) return;

            _isToggling = false;
            _view.EnableBeam(false);
            _view.StopHitEffect();
            _view.StopAudio();
            FinishPendingLightOff();
            return;
        }

        if (!_idleSetupDone)
        {
            // Reintegrating: the light comes up first, the beam waits for it (and receptors with it).
            if (!IsLightOn)
            {
                if (!IsLightRampingUp) TryTweenLight(_lightMaxIntensity);
                return;
            }

            var hit = CheckRaycast(out var origin, out var dir, out var didHit);

            StartLengthTransition(didHit ? hit.distance : _maxDist);
            _isToggling = true;
            _idleSetupDone = true;
        }

        _view.EnableBeam(true);
        CheckCommonBehaviour();
    }

    private void TransitioningBehaviour()
    {
        if (!_isInitialized)
        {
            FinishPendingLightOff();
            return;
        }

        if (!_isToggling)
        {
            _isToggling = true;
            StartLengthTransition(0f);
        }

        _model.ClearReceptor();
        _model.UpdateRaycastDistance();

        var origin = GetLaserOrigin();
        var dir = transform.forward;
        var end = origin + dir * _model.CurrentDist;

        _view.SetLaserPositions(origin, end);

        if (_model.IsTransitioning) return;

        _isToggling = false;
        _view.EnableBeam(false);
        _view.StopHitEffect();
        _view.StopAudio();
        _lightOffPending = false;
        TryTweenLight(LightOffIntensity);
    }

    private RaycastHit CheckRaycast(out Vector3 origin, out Vector3 dir, out bool didHit)
    {
        origin = GetLaserOrigin();
        dir = transform.forward;

        if (!PassesThroughIntangibles)
        {
            didHit = Physics.Raycast(origin, dir, out var hit, _maxDist, Layer, QueryTriggerInteraction.Ignore);
            return hit;
        }

        // Un láser intangible atraviesa los objetos Intangible que también están en Intangible: hace falta
        // ver detrás del primer impacto, así que se piden todos los hits y se queda con el más cercano
        // que no sea ignorable (RaycastNonAlloc no garantiza orden).
        int count = Physics.RaycastNonAlloc(origin, dir, _hitBuffer, _maxDist, Layer, QueryTriggerInteraction.Ignore);

        // El jugador con un nodo Intangible en la mano también se atraviesa. Se consulta en cada
        // raycast (sin caché): si deja de serlo dentro del haz, este mismo frame vuelve a pegarle.
        var player = PlayerController.Instance;
        var ignorePlayer = player != null && player.IsIntangible;

        var closest = default(RaycastHit);
        didHit = false;

        for (int i = 0; i < count; i++)
        {
            var candidate = _hitBuffer[i];

            if (didHit && candidate.distance >= closest.distance) continue;
            if (ignorePlayer && player.IsOwnCollider(candidate.collider)) continue;
            if (IsPhasedIntangible(candidate.collider)) continue;

            closest = candidate;
            didHit = true;
        }

        return closest;
    }

    // Only the Intangible laser variant, and only while its own container is Intangible.
    private bool PassesThroughIntangibles => _intangible != null && _intangible.IsPhased;

    // GetComponentInParent por si el collider está en un hijo del Intangible.
    private static bool IsPhasedIntangible(Collider coll)
    {
        var intangible = coll.GetComponentInParent<Intangible>();
        return intangible != null && intangible.IsPhased;
    }

    private void CheckCommonBehaviour()
    {
        var hit = CheckRaycast(out var origin, out var dir, out var didHit);

        if (_isToggling && _model.IsTransitioning)
        {
            _model.ClearReceptor();
            _model.UpdateRaycastDistance();
            _view.SetLaserPositions(origin, origin + dir * _model.CurrentDist);
            _view.StopHitEffect();
            return;
        }

        _isToggling = false;
        FinishPendingLightOff();
        _model.SetInstant(_isInitialized ? (didHit ? hit.distance : _maxDist) : 0f);

        if (!_isInitialized)
        {
            _view.SetTrailEmitting(false);
            return;
        }

        // Any change in the beam length (something entering or leaving the beam) turns the trail off;
        // it comes back on the first frame the length repeats. No hit = no trail: it lives under the
        // hit effect, which StopHitEffect deactivates.
        var dist = didHit ? hit.distance : _maxDist;
        var lengthIsStable = Mathf.Approximately(dist, _lastDist);
        _lastDist = dist;

        if (didHit) _model.ProcessReceptor(hit, _ownReceptor, EmitterState);
        else _model.ClearReceptor();

        _model.UpdateRaycastDistance();

        var end = origin + dir * _model.CurrentDist;
        
        _view.SetLaserPositions(origin, end);

        if (didHit)
            _view.ShowHitEffect(hit.point, hit.normal);
        else
            _view.StopHitEffect();

        // After ShowHitEffect, so the trail is already at the hit point when it is cleared and turned on.
        _view.SetTrailEmitting(didHit && lengthIsStable);

        _view.PlayAudio();
    }

    // Every eased length change turns the trail off; CheckCommonBehaviour turns it back on once the
    // transition is over and the length is stable.
    private void StartLengthTransition(float length)
    {
        _model.SetLaserLength(length);
        _lastDist = NoDistance;
        _view.SetTrailEmitting(false);
    }

    public void LaserReceived()
    {
        if (_startsInitialized) return;

        StartLaser();
    }

    // Light up first, beam on completion. From any point of a turn-off: if the beam is still
    // retracting the light is already on, so the beam comes back right away.
    private void StartLaser()
    {
        _lightOffPending = false;

        // A glitched laser on the move keeps its beam and light off: IdleBehaviour turns the light on
        // when it reintegrates.
        if (_isTransitioning)
        {
            ActivateBeam();
            return;
        }

        if (TryTweenLight(_lightMaxIntensity))
            _lightTween.OnComplete(this, laser => laser.ActivateBeam());
        else
            ActivateBeam();
    }

    private void ActivateBeam()
    {
        _isInitialized = true;
        _isToggling = true;

        var hit = CheckRaycast(out var origin, out var dir, out var didHit);

        StartLengthTransition(didHit ? hit.distance : _maxDist);
    }

    public void LaserNotReceived()
    {
        if (_startsInitialized) return;

        // The beam is out (or still retracting) unless the light was coming up for it.
        var beamIsOut = (_isInitialized && !IsLightRampingUp) || _lightOffPending;

        _isInitialized = false;
        _isToggling = true;
        _idleSetupDone = false;
        _model.ClearReceptor();
        StartLengthTransition(0f);
        _view.StopAudio();

        // Beam first, light after: the light waits for the retraction. With no beam out, it goes now.
        _lightOffPending = beamIsOut;
        if (!beamIsOut) TryTweenLight(LightOffIntensity);
    }

    private void FinishPendingLightOff()
    {
        if (!_lightOffPending) return;

        _lightOffPending = false;
        TryTweenLight(LightOffIntensity);
    }

    private bool IsLightOn => _light == null || Mathf.Approximately(_light.intensity, _lightMaxIntensity);

    private bool IsLightRampingUp => _lightTween.isAlive && Mathf.Approximately(_lightTarget, _lightMaxIntensity);

    // Tweens the light from its current intensity, so an interrupted ramp reverses from where it is.
    // The duration scales with the distance left. Returns false when there is nothing to tween (no
    // light, or already there): the caller continues right away.
    private bool TryTweenLight(float target)
    {
        _lightTween.Stop();
        _lightTarget = target;

        if (_light == null) return false;

        var range = Mathf.Abs(_lightMaxIntensity - LightOffIntensity);
        var remaining = Mathf.Abs(target - _light.intensity);

        if (range <= 0f || _lightEaseTime <= 0f || Mathf.Approximately(remaining, 0f))
        {
            _light.intensity = target;
            return false;
        }

        var duration = _lightEaseTime * Mathf.Clamp01(remaining / range);
        _lightTween = Tween.LightIntensity(_light, target, duration, _lightEaseType);
        return true;
    }

    private Vector3 GetLaserOrigin()
    {
        return transform.position + transform.forward * _offsetZ;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _gizmosColor;
        
        var maxDist = _model?.CurrentDist ?? _maxDist;

        var origin = transform.position + transform.forward * _offsetZ;
        var dir = transform.forward;
        var end = origin + dir * maxDist;

        Gizmos.DrawLine(origin, end);
        Gizmos.DrawSphere(origin, 0.05f);
    }
}
