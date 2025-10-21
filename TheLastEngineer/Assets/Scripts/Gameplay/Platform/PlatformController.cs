using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class StationsStops
{
    public Transform Position;
    public bool IsStation;
}

public class PlatformController : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _accelTime = 1f;
    [SerializeField] private float _decelTime = 1f;
    [SerializeField] private float _waitCD = 1f;
    [SerializeField] private LeanTweenType _accelEase = LeanTweenType.easeOutSine;
    [SerializeField] private LeanTweenType _decelEase = LeanTweenType.easeInSine;
    [SerializeField] private NodeType _requiredNode = NodeType.Corrupted;
    [SerializeField] private Connection _connection = default;
    [SerializeField] private StationsStops[] _positions2;
    
    private bool[] _stationList;
    private Transform[] _positions;
    public Dictionary<Vector3, bool> myDictionary;
    private IMovablePassenger _passenger;
    private IPlatformState _state;
    private PlatformStateMachine _fsm;
    private Coroutine _changingColor = null;
    private PlayerController _player = default;
    private LTDescr _tween = default;

    public bool isStopped;
    public bool isReversed;
    
    public float CurrentSpeed { get; private set; }
    public float MoveSpeed => _moveSpeed;
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
        Motor = new PlatformMotor(transform, null);
        CurrentSpeed = _moveSpeed;

        if (_connection != null)
            _connection.OnInitialized += Initialize;
    }

    private void Initialize()
    {
        _connection.OnNodeConnected += OnConnectionChanged;
        _fsm = new PlatformStateMachine(this, _connection.StartsConnected);
        SetPositiveFeedback(_connection.StartsConnected);
    }

    private void OnDestroy()
    {
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
    private void OnConnectionChanged(NodeType type, bool active)
    {
        bool canMove = (type == _requiredNode) && active;
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
    public void AdvanceRouteAndWait()
    {
        if (Route.HasToWait(myDictionary[CurrentTarget]))
            _fsm.ToWaiting();

        Route.Advance();
    }

    public void BeginWait()
    {
        WaitTimer = _waitCD;
    }

    public void StopPassenger()
    {
        if (_passenger != null) return;

        Motor.Stop(_passenger);
    }

    public bool ReachedTarget()
    {
        return Motor.InTarget(CurrentTarget, CurrentSpeed);
    }

    public bool CheckStop()
    {
        return myDictionary[CurrentTarget];
    }

    public void MoveStep()
    {
        Motor.MoveTowards(CurrentTarget, CurrentSpeed, _passenger);
    }

    public void CancelSpeedTween()
    {
        if (_tween != null) LeanTween.cancel(gameObject);
        _tween = null;
    }

    public void StartAccelerationToMax()
    {
        CancelSpeedTween();
        CurrentSpeed = 0f;
        _tween = LeanTween.value(gameObject, 0f, _moveSpeed, _accelTime)
            .setEase(_accelEase)
            .setOnUpdate(v => CurrentSpeed = v);
    }

    public void StartDeceleration(float duration)
    {
        CancelSpeedTween();
        float v0 = CurrentSpeed;
        _tween = LeanTween.value(gameObject, v0, 0f, Mathf.Max(0.01f, duration))
            .setEase(_decelEase)
            .setOnUpdate(v => CurrentSpeed = v);
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

    private void CleanPlayerReferences()
    {
        if (_player == null) return;
        
        _player.OnDied -= CleanPlayerReferences;
        _player = null;
        _passenger = null;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out IMovablePassenger passenger))
        {
            if (passenger is not PlayerController player || player.IsDead)
            {
                CleanPlayerReferences();
                return;
            }

            _passenger = passenger;
            _player = player;
            _player.OnDied += CleanPlayerReferences;
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out IMovablePassenger passenger) && _passenger == passenger)
        {
            _passenger.OnPlatformMoving(Vector3.zero);
            _passenger = null;
        }
    }
}