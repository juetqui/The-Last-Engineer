using System.Collections;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    [SerializeField] private float _shieldCD = default;
    [SerializeField] private AudioClip _chargedFX;

    PlayerTDController _player = null;
    private Transform _parent = default;
    private Renderer _renderer = default;
    private SphereCollider _collider = default;
    private AudioSource _audioSource = default;

    private bool _isActive = false, _canUse = true;

    //private ShieldModel _shieldModel = default;
    private ShieldView _shieldView = default;

    private void Awake()
    {
        _parent = GetComponentInParent<Transform>();
        _renderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<SphereCollider>();
        _audioSource = GetComponent<AudioSource>();
        
        //_shieldModel = new ShieldModel();
        _shieldView = new ShieldView(_renderer, _collider, _audioSource, _chargedFX);
    }

    private void Start()
    {
        _renderer.enabled = false;
        _collider.enabled = false;
        
        _player = PlayerTDController.Instance;
        _player.OnChangeActiveShield += ActivateShield;
    }

    public void ActivateShield()
    {
        _isActive = !_isActive;

        if (_isActive && _canUse)
        {
            _shieldView.SetActive(true);
        }
        else
        {
            _shieldView.SetActive(false);
            StartCoroutine(ShieldCD());
        }
    }

    private IEnumerator ShieldCD()
    {
        _canUse = false;
        yield return new WaitForSeconds(_shieldCD);
        _shieldView.PlayChargedFX();
        _canUse = true;
    }
}
