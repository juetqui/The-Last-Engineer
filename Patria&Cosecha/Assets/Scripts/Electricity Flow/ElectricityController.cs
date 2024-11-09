using UnityEngine;
using UnityEngine.Splines;

public class ElectricityController : MonoBehaviour
{
    [SerializeField] private SplineContainer[] _splines;
    [SerializeField] private ConnectionNode[] _connections;

    private TrailController _trailController = default;
    private MainTM _mainTM = default;

    private void Awake()
    {
        SplineAnimate animator = GetComponent<SplineAnimate>();
        ISplinePathProvider splinePathProvider = new SplinePP(_splines);

        _trailController = new TrailController(animator, splinePathProvider);
    }

    private void Start()
    {
        _trailController.OnStart();
    }

    public void MoveSpline()
    {
        Debug.Log(_connections[_trailController.CurrentIndex].IsWorking);
        if (_connections[_trailController.CurrentIndex].IsWorking)
        {
            _trailController.MoveToNextSpline();
        }
        else return;
    }

    public void SetTM(MainTM mainTM)
    {
        _mainTM = mainTM;
    }
}
