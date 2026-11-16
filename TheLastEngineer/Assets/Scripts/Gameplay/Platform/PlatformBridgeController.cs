using UnityEngine;

public class PlatformBridgeController : MonoBehaviour
{
    [SerializeField] private Connection _connection;
    
    private Animator _animator;

    private void Awake()
    {
        _connection.OnNodeConnected += OnNodeConnected;
        _animator = GetComponent<Animator>();
    }

    private void OnNodeConnected(GlitchState state, bool connected)
    {
        if (connected) _animator.SetTrigger("Set");
        else _animator.SetTrigger("UnSet");
    }
}
