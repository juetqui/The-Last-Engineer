using UnityEngine;
public class DoorsView : MonoBehaviour
{
    [SerializeField] private bool _isBroken = false;
    private Animator _animator = default;
    private ParticleSystem _particulas = default;
    private AudioSource _openDoor = default;

    public void Initialize()
    {
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
            _particulas.Play();
            _openDoor.Play();
        }
        else _particulas.Stop();
    }
}
