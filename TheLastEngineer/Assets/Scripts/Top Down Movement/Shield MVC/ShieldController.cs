using System.Collections;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    [SerializeField] private PlayerTDController _player;
    [SerializeField] private float _shieldDuration = default, _shieldCD = default;
    [SerializeField] private AudioClip _chargedFX;

    private Renderer _renderer;
    private SphereCollider _collider;
    private AudioSource _audioSource;

    private float _durationCounter = 0f;
    private bool _isActive = false, _canUse = true;
    private Coroutine _keepShieldActive = null;

    //private ShieldModel _shieldModel = default;
    private ShieldView _shieldView = default;

    private void Awake()
    {

        _renderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<SphereCollider>();
        _audioSource = GetComponent<AudioSource>();
        _player.onShieldActive += ActivateShield;
        
        //_shieldModel = new ShieldModel();
        _shieldView = new ShieldView(_renderer, _collider, _audioSource, _chargedFX);
    }

    private void Start()
    {
        _renderer.enabled = false;
        _collider.enabled = false;
    }

    void Update()
    {
        if (_isActive && _canUse && _keepShieldActive == null)
        {
            _keepShieldActive = StartCoroutine(KeepShieldActive());
        }
        else if (!_isActive && _keepShieldActive != null)
        {
            StopCoroutine(_keepShieldActive);
            _shieldView.SetActive(false);
            StartCoroutine(ShieldCD());
        }
    }

    public void ActivateShield(bool isActive)
    {
        _isActive = isActive;
    }

    private IEnumerator KeepShieldActive()
    {
        _shieldView.SetActive(true);
        _durationCounter = 0f;

        while (_durationCounter < _shieldDuration)
        {
            _durationCounter += Time.deltaTime;
            yield return null;
        }

        if (_isActive)
        {
            _isActive = false;
            _shieldView.SetActive(false);

            StartCoroutine(ShieldCD());
        }
    }

    private IEnumerator ShieldCD()
    {
        _keepShieldActive = null;
        _canUse = false;
        yield return new WaitForSeconds(_shieldCD);
        _shieldView.PlayChargedFX();
        _canUse = true;
    }
}
