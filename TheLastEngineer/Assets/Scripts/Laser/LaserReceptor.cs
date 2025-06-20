using UnityEngine;
using UnityEngine.Events;

public class LaserReceptor : MonoBehaviour, ILaserReceptor
{
    [SerializeField] UnityEvent OnEndHit;
    [SerializeField] UnityEvent OnHit;
    [SerializeField] UnityEvent OnFill;
    [SerializeField] UnityEvent OnDepleated;
    
    private MeshRenderer _meshRenderer = default;
    private Collider _collider = default;
    
    public bool _isCompleted = false;
    public bool _isFull = false;
    
    public void LaserNotRecived()
    {
        if (!_isCompleted)
            OnEndHit?.Invoke();
    }
    
    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<Collider>();

    }
    
    public void TurnOffObject()
    {
        _meshRenderer.enabled = false;
        _collider.enabled = false;
    }
    
    public void BrotherCompletationChecker()
    {
        _meshRenderer.enabled = false;
        _collider.enabled = false;
    }
    
    public void LaserRecived()
    {
        OnHit?.Invoke();
    }
    
    public void ChargeCompleted()
    {
        OnFill?.Invoke();
    }
    
    public void ChargeFilled()
    {
        _isCompleted = true;
    }
    
    public void ChargeDepleted()
    {
        OnDepleated?.Invoke();
    }
}
