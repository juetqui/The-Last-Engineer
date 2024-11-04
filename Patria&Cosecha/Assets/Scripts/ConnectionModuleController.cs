using UnityEngine;
using UnityEngine.Splines;

public class ConnectionModuleController : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] _ps;
    [SerializeField] private SplineAnimate _animator;

    private MainTM _mainTM = default;

    void Update()
    {
        if (_mainTM.Running)
        {
            foreach (var ps in _ps)
            {
                ps.Play();
            }
            
            _animator.Play();
        }
    }

    public void SetTM(MainTM mainTM)
    {
        _mainTM = mainTM;
    }
}
