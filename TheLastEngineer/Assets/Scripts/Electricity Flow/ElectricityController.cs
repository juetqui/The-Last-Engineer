using UnityEngine;
using UnityEngine.Splines;

public class ElectricityController : MonoBehaviour
{
    [SerializeField] private SplineContainer[] _splines;
    [SerializeField] private SpecificConnectionController[] _connections;

    private TrailController _trailController = default;

    [SerializeField] private int _index = default;

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

    private void Update()
    {
        _index = _trailController.CurrentIndex;
    }

    public void MoveSpline()
    {
        if (_trailController.CurrentIndex < _connections.Length && _connections[_trailController.CurrentIndex].IsWorking)
            _trailController.MoveToNextSpline();
        else return;
    }
}
