using UnityEngine;
using UnityEngine.Splines;

public class ElectricityController : MonoBehaviour
{
    [SerializeField] private SplineContainer[] _splines;
    [SerializeField] private SplineAnimate _animator;

    private MainTM _mainTM = default;
    private int _index = default;

    private void Awake()
    {
        _animator = GetComponent<SplineAnimate>();
    }

    private void Start()
    {
        _index = 0;
        _animator.Container = _splines[0];
    }

    private void Update()
    {
        if (_mainTM.Running)
        {
            _index++;
            _animator.Container = _splines[1];
        }
    }

    public void SetTM(MainTM mainTM)
    {
        _mainTM = mainTM;
    }
}
