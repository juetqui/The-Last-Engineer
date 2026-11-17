using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using PrimeTween;

[System.Serializable]
public class StationsStops
{
    public Transform Position;
    public bool IsStation;
}

[DefaultExecutionOrder(ExecutionOrder.Platform)]
public class PlatformController : MonoBehaviour
{
    [Header("Configuraci�n")]
    [SerializeField] private float _moveTime = 2f;
    [SerializeField] private float _accelTime = 1f;
    [SerializeField] private float _decelTime = 1f;
    [SerializeField] private float _waitCD = 1f;
    [SerializeField] private Ease _accelEase = Ease.OutSine;
    [SerializeField] private Ease _decelEase = Ease.InSine;
    [SerializeField] private Connection _connection = default;
    [SerializeField] private StationsStops[] _positions2;

    public Dictionary<Vector3, bool> myDictionary;
    // Route points without duplicate positions, in route order (aligned with Route's indices).
    private StationsStops[] _stops;
    private bool[] _isStation;
    private Transform _targetStation;
    private IMovablePassenger _passenger;
    private IPlatformState _state;
    private PlatformStateMachine _fsm;
    private GlitchState _requiredLevel = GlitchState.Clean;
    private Coroutine _changingColor = null;
    private PlayerController _player = default;
    private Tween _tween = default;

    public bool isStopped;
    public bool isReversed;
    public bool isVertical;

    /// <summary>A connection with the required level powered the platform on (also on scene load, for connections that start plugged).</summary>
    public event System.Action PoweredOn;

    /// <summary>The platform came to rest unpowered: it entered the Inactive state (after ToStop, or straight from Waiting).</summary>
    public event System.Action BecameInactive;

    /// <summary>The player got on (true) or off (false): trigger exit, death on the platform, or the platform being destroyed.</summary>
    public event System.Action<bool> PassengerChanged;

    /// <summary>The platform started travelling (Moving; stays true while decelerating in ToStop) or came to rest (Waiting/Inactive).</summary>
    public event System.Action<bool> MovingChanged;

    /// <summary>The connection stopped powering the platform (unplugged or wrong level). It may still be travelling to its target.</summary>
    public event System.Action PoweredOff;

    /// <summary>The platform arrived at a station and stopped there while powered.</summary>
    public event System.Action<Transform> StationReached;

    /// <summary>The station the platform is heading to changed (always raised after arriving at a station). Null when the route has none.</summary>
    public event System.Action<Transform> TargetStationChanged;

    public float CurrentSpeed { get; private set; }
    public float SegmentSpeed { get; private set; }
    public float MoveTime => _moveTime;
    public float WaitCD => _waitCD;
    public float DecelTime => _decelTime;
    public float AccelTime => _accelTime;
    public float WaitTimer { get; set; }

    public IMovablePassenger Passenger => _passenger;
    public PlatformMotor Motor { get; private set; }
    public RouteManager Route { get; private set; }
    public Vector3 CurrentTarget => Route.CurrentPoint;

    /// <summary>The route as serialized in the Inspector (available before Awake).</summary>
    public IReadOnlyList<StationsStops> Stops => _positions2;

    /// <summary>The station the platform is heading to, or null when the route has none.</summary>
    public Transform TargetStation => _targetStation;

    private void Awake()
    {
        myDictionary = new Dictionary<Vector3, bool>();
        var uniqueStops = new List<StationsStops>(_positions2.Length);

        for(int i = 0; i < _positions2.Length; i++)
        {
            StationsStops stop = _positions2[i];

            if (stop == null || stop.Position == null)
            {
                Debug.LogWarning($"{nameof(PlatformController)} on '{name}' has an empty route point at {i}; skipping it.", this);
                continue;
            }

            if (myDictionary.ContainsKey(stop.Position.position)) continue;

            myDictionary.Add(stop.Position.position, stop.IsStation);
            uniqueStops.Add(stop);
        }

        _stops = uniqueStops.ToArray();
        _isStation = _stops.Select(s => s.IsStation).ToArray();

        Route = new RouteManager(myDictionary.Keys.ToArray(),this);
        _targetStation = StopAt(Route.NextStation(_isStation));
        Motor = new PlatformMotor(KinematicBody.Ensure(gameObject), transform);
        CurrentSpeed = 0f;

        if (_connection != null)
            _connection.OnInitialized += Initialize;
    }

    private void Initialize()
    {
        _connection.OnNodeConnected += OnConnectionChanged;
        _requiredLevel = _connection.RequiredLevel;
        _fsm = new PlatformStateMachine(this, _connection.StartsConnected);
        SetPositiveFeedback(_connection.StartsConnected);
    }

    private void OnDestroy()
    {
        _tween.Stop();
        ReleasePassenger();

        if (_connection != null)
            _connection.OnNodeConnected -= OnConnectionChanged;
    }


    private void Update()
    {
        if (!Route.IsValid) return;
        _fsm?.Tick(Time.deltaTime);
    }

    public void SetPositiveFeedback(bool Active)
    {
        //refuerzoPositivo.SetActive(Active);

        if (_changingColor != null) StopCoroutine(_changingColor);

        Color targetColor = Active ? Color.cyan : Color.black;
        //_changingColor = StartCoroutine(ChangeColor(targetColor));
    }

    /* -------------------- Eventos externos -------------------- */
    private void OnConnectionChanged(GlitchState type, bool active)
    {
        bool canMove = (type == _requiredLevel) && active;
        
        SetPositiveFeedback(canMove);

        if (canMove) 
        {
            if (isStopped)
                _fsm.ToWaiting();
            else
                _fsm.ToMoving();

            PoweredOn?.Invoke();
        }
        else
        {
            if(!(_fsm.Current==_fsm.Waiting)&& !(_fsm.Current == _fsm.Inactive))
                _fsm.ToStop();
            else
                _fsm.ToInactive();

            PoweredOff?.Invoke();
        }
        //_fsm.ToReturning();
    }

    private Transform StopAt(int index)
    {
        return index >= 0 && index < _stops.Length ? _stops[index].Position : null;
    }

    /* -------------------- API interna usada por los estados -------------------- */
    #region
    /// <param name="notifyTarget">Raise <see cref="TargetStationChanged"/> even if the target is the same station (heading back to it).</param>
    public void AdvanceRoute(bool notifyTarget = false)
    {
        Route.Advance();

        Transform target = StopAt(Route.NextStation(_isStation));
        if (target == _targetStation && !notifyTarget) return;

        _targetStation = target;
        TargetStationChanged?.Invoke(target);
    }

    public void AdvanceRouteAndWait()
    {
        bool isStation = myDictionary[CurrentTarget];

        if (isStation)
            StationReached?.Invoke(StopAt(Route.CurrentIndex));

        if (Route.HasToWait(isStation))
            _fsm.ToWaiting();

        AdvanceRoute(notifyTarget: isStation);
    }

    public void NotifyInactive()
    {
        BecameInactive?.Invoke();
    }

    public void NotifyMoving(bool isMoving)
    {
        MovingChanged?.Invoke(isMoving);
    }

    public void BeginWait()
    {
        WaitTimer = _waitCD;
    }

    public void StopPassenger()
    {
        if (_passenger == null) return;

        Motor.Stop(ActivePassenger);
    }

    public bool CheckStop()
    {
        return myDictionary[CurrentTarget];
    }

    /// <summary>Moves one frame toward the current target. True when it landed exactly on it.</summary>
    public bool MoveStep(out float leftover)
    {
        return Motor.Step(CurrentTarget, CurrentSpeed, ActivePassenger, out leftover);
    }

    /// <summary>Spends the distance left over after reaching a waypoint on the next segment, so the speed doesn't dip.</summary>
    public void MoveDistance(float distance)
    {
        Motor.Travel(CurrentTarget, distance, ActivePassenger, out _);
    }

    // The passenger is always the player; a destroyed player must not be called through the interface.
    private IMovablePassenger ActivePassenger => _player == null ? null : _passenger;

    public void CancelSpeedTween()
    {
        _tween.Stop();
    }

    public void RefreshSegmentSpeed()
    {
        float dist = Vector3.Distance(transform.position, Route.CurrentPoint);
        SegmentSpeed = dist / Mathf.Max(0.01f, _moveTime);
    }

    public void StartAccelerationToMax()
    {
        CancelSpeedTween();
        CurrentSpeed = 0f;
        _tween = Tween.Custom(gameObject, 0f, SegmentSpeed, _accelTime,
            (_, v) => CurrentSpeed = v, _accelEase);
    }

    public void ContinueWithNewSegmentSpeed()
    {
        CancelSpeedTween();
        float from = CurrentSpeed;
        float to = SegmentSpeed;
        if (Mathf.Approximately(from, to)) { CurrentSpeed = to; return; }
        float t = _accelTime * Mathf.Abs(to - from) / Mathf.Max(0.001f, to);
        _tween = Tween.Custom(gameObject, from, to, Mathf.Max(0.01f, t),
            (_, v) => CurrentSpeed = v, _accelEase);
    }

    public void StartDeceleration(float duration)
    {
        CancelSpeedTween();
        float v0 = CurrentSpeed;
        _tween = Tween.Custom(gameObject, v0, 0f, Mathf.Max(0.01f, duration),
            (_, v) => CurrentSpeed = v, _decelEase);
    }

    public void DecelerateToTarget(Vector3 target)
    {
        float d = Vector3.Distance(transform.position, target);
        float v0 = Mathf.Max(CurrentSpeed, 0.01f);
        float t = (2f * d) / v0;
        StartDeceleration(t);
    }

    public void StopImmediate()
    {
        CancelSpeedTween();
        CurrentSpeed = 0f;
    }
    #endregion

    // Releases the rider for every way of getting off: leaving the trigger, dying on the platform, or the
    // platform being destroyed. Restores gravity a vertical platform switched off.
    private void ReleasePassenger()
    {
        // C# null check on the interface: a destroyed player still counts as a rider to release.
        bool hadPassenger = _passenger != null;

        if (_player != null)
        {
            _player.OnPlatformMoving(Vector3.zero);

            if (isVertical)
                _player.SetVerticalPlatform(false);

            _player.OnDied -= ReleasePassenger;
        }

        _player = null;
        _passenger = null;

        if (hadPassenger)
            PassengerChanged?.Invoke(false);
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out IMovablePassenger passenger))
        {
            if (passenger is not PlayerController player || player.IsDead)
            {
                ReleasePassenger();
                return;
            }

            // A second collider of the same player (or of this platform's compound) must not subscribe twice.
            if (player == _player) return;

            ReleasePassenger();
            _passenger = passenger;
            _player = player;
            if (isVertical)
            {
                _player.SetVerticalPlatform(true);
            }
            _player.OnDied += ReleasePassenger;
            PassengerChanged?.Invoke(true);
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out IMovablePassenger passenger) && _passenger == passenger)
            ReleasePassenger();
    }
}