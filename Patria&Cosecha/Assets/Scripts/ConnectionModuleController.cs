using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class ConnectionModuleController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _orbitPs;
    [SerializeField] private ParticleSystem _completedPs;
    [SerializeField] private SplineAnimate _animator;

    [Header("Emissive Management")]
    [SerializeField] private float _onSpeed;
    [SerializeField] private float _offSpeed;

    [ColorUsage(true, true)]
    [SerializeField] private Color _onColor;

    [ColorUsage(true, true)]
    [SerializeField] private Color _offColor;

    private MainTM _mainTM = default;
    private Renderer _renderer = default;
    
    private float _currentIntensity = default;
    private bool _canAdd = true;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.material.EnableKeyword("_EMISSION");
    }

    private void Start()
    {
        StopFX();
    }

    private void Update()
    {
        if (_mainTM.Running) PlayFX();
    }

    private void StopFX()
    {
        _orbitPs.Stop();
        _completedPs.Stop();
        EnableEmission(_offColor, _offSpeed);
    }

    private void PlayFX()
    {
        if (!_orbitPs.isPlaying) _orbitPs.Play();
        if (!_completedPs.isPlaying) _completedPs.Play();
        EnableEmission(_onColor, _onSpeed);
        _animator.Play();
    }

    private void EnableEmission(Color color, float emissive)
    {
        if (_currentIntensity < 10 && _canAdd)
        {
            _currentIntensity += emissive * Time.deltaTime;
            StartCoroutine(EmissiveCD());
        }
        
        Color emissiveColor = color * _currentIntensity;

        _renderer.material.SetColor("_EmissionColor", emissiveColor);
    }

    public void SetTM(MainTM mainTM)
    {
        _mainTM = mainTM;
    }

    private IEnumerator EmissiveCD()
    {
        _canAdd = false;
        yield return new WaitForSeconds(0.5f);
        _canAdd = true;
    }
}
