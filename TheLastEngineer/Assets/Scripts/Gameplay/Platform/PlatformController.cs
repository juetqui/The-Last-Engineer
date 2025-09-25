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
    [SerializeField] private float _corruptedMoveSpeed = 0f;
    [SerializeField] private float _waitCD = 1f;
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

    public bool isStopped;
    public bool isReversed;
    
    public float CurrentSpeed { get; private set; }
    public float MoveSpeed => _moveSpeed;
    public float CorruptedMoveSpeed => (_corruptedMoveSpeed > 0f) ? _corruptedMoveSpeed : _moveSpeed * 0.5f;
    public float WaitCD => _waitCD;
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
        
        if (_corruptedMoveSpeed <= 0f) _corruptedMoveSpeed = _moveSpeed / 2f;
    }

    private void Start()
    {
        _connection.OnNodeConnected += OnConnectionChanged;
        
         if (PlayerNodeHandler.Instance != null)
            PlayerNodeHandler.Instance.OnNodeGrabbed += OnNodeGrabbed;

        _fsm = new PlatformStateMachine(this, _connection.StartsConnected);

        SetPositiveFeedback(_connection.StartsConnected);
    }

    private void OnDestroy()
    {
        if (_connection != null)
            _connection.OnNodeConnected -= OnConnectionChanged;

        if (PlayerNodeHandler.Instance != null)
            PlayerNodeHandler.Instance.OnNodeGrabbed -= OnNodeGrabbed;
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

    private void OnNodeGrabbed(bool hasNode, NodeType nodeType)
    {
        CurrentSpeed = (hasNode && nodeType == NodeType.Corrupted) ? CorruptedMoveSpeed : MoveSpeed;
    }

    /* -------------------- API interna usada por los estados -------------------- */

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
        return Motor.InTarget(CurrentTarget);
    }
    public bool CheckStop()
    {
        return myDictionary[CurrentTarget];
    }

    public void MoveStep()
    {
        Motor.MoveTowards(CurrentTarget, CurrentSpeed, _passenger);
    }

    private void CleanPlayerReferences()
    {
        if (_player != null)
            _player.OnDied -= CleanPlayerReferences;

    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.TryGetComponent(out IMovablePassenger passenger))
        {
            _passenger = passenger;

            if (_passenger is PlayerController)
            {
                _player = (PlayerController) _passenger;
                _player.OnDied += CleanPlayerReferences;
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.TryGetComponent(out IMovablePassenger passenger) && _passenger == passenger)
        {
            _passenger.OnPlatformMoving(Vector3.zero);
            _passenger = null;
        }
    }
}