using UnityEngine;
public class DoorsView : MonoBehaviour
{
    [SerializeField] private bool _isBroken = false;
    private Animator _animator = default;
    private ParticleSystem _particulas = default;
    private AudioSource _openDoor = default;

    [SerializeField] private Renderer _doorLight;

    [ColorUsageAttribute(true, true)]
    private Color _doorOpen = Color.green;
    [ColorUsageAttribute(true, true)]
    private Color _doorClosed = Color.red;

    public void Initialize()
    {
        _doorLight.material.color = _doorClosed;
        _animator = GetComponent<Animator>();
        _openDoor = GetComponent<AudioSource>();
        _particulas = GetComponentInChildren<ParticleSystem>();
    }
    
    private void Start()
    {
        if (_isBroken) _animator.SetBool("IsBroken", _isBroken);
    }
    
    public void OpenDoor(bool isRunning)
    {
        _animator.SetBool("DoorActivated", isRunning);

        if (isRunning)
        {
            if (_doorLight != null)
                _doorLight.material.SetColor("_EmissiveColor", _doorOpen);

            _particulas.Play();
            _openDoor.Play();
        }
        else
        {
            _particulas.Stop();
            if (_doorLight != null)
                _doorLight.material.SetColor("_EmissiveColor", _doorClosed);
        }
    }
}
