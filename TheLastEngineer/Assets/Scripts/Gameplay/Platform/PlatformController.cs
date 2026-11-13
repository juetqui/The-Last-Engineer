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

    private bool[] _stationList;
    private Transform[] _positions;
    public Dictionary<Vector3, bool> myDictionary;
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

    private void Awake()
    {
        myDictionary = new Dictionary<Vector3, bool>();
        
        for(int i = 0; i < _positions2.Length; i++)
        {
            if (!myDictionary.ContainsKey(_positions2[i].Position.position))
                myDictionary.Add(_positions2[i].Position.position, _positions2[i].IsStation);
        }
        
        Route = new RouteManager(myDictionary.Keys.ToArray(),this);
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
        }
        else if(!(_fsm.Current==_fsm.Waiting)&& !(_fsm.Current == _fsm.Inactive))
            _fsm.ToStop();
        else
        {
            _fsm.ToInactive();
        }
        //_fsm.ToReturning();
    }

    /* -------------------- API interna usada por los estados -------------------- */
    #region
    public void AdvanceRoute()
    {
        Route.Advance();
    }

    public void AdvanceRouteAndWait()
    {
        if (Route.HasToWait(myDictionary[CurrentTarget]))
            _fsm.ToWaiting();

        AdvanceRoute();
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
        if (_player != null)
        {
            _player.OnPlatformMoving(Vector3.zero);

            if (isVertical)
                _player.SetVerticalPlatform(false);

            _player.OnDied -= ReleasePassenger;
        }

        _player = null;
        _passenger = null;
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
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out IMovablePassenger passenger) && _passenger == passenger)
            ReleasePassenger();
    }
}