using System.Collections;
using UnityEngine;

public class CheckPointController : MonoBehaviour
{
    [SerializeField] private Renderer _vfxRenderer;
    //[SerializeField] private GenericTM _taskManager;
    [SerializeField] private float _transitionDuration = 0.5f;
    
    [SerializeField] private Color _enabledMainColor;
    [SerializeField] private Color _enabledSecColor;
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _enabledFresnelColor;

    [SerializeField] private GameObject _objectToActivate;

    private SkinnedMeshRenderer _renderer = default;
    private Light _light = default;
    private AudioSource _source = default;
    private bool _enabled = true;

    private void Awake()
    {
        _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        _light = GetComponentInChildren<Light>();
        _source = GetComponent<AudioSource>();
    }

    private IEnumerator SetMaterialsCoroutine()
    {
        _enabled = false;
        _source.Play();

        float elapsed = 0f;

        Color vfxColor = _vfxRenderer.material.GetColor("_Color");

        Color[] startMain = new Color[_renderer.materials.Length];
        Color[] startSec = new Color[_renderer.materials.Length];
        Color[] startFresnel = new Color[_renderer.materials.Length];

        for (int i = 0; i < _renderer.materials.Length; i++)
        {
            startMain[i] = _renderer.materials[i].GetColor("_Color");
            startSec[i] = _renderer.materials[i].GetColor("_SecColor");
            startFresnel[i] = _renderer.materials[i].GetColor("_FresnelColor");
        }

        while (elapsed < _transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _transitionDuration;

            Color vfxNewColor = Color.Lerp(vfxColor, _enabledFresnelColor, t);
            Color newLightColor = Color.Lerp(_light.color, _enabledMainColor, t);

            _vfxRenderer.material.SetColor("_Color", vfxNewColor);
            _light.color = newLightColor;

            foreach (var material in _renderer.materials)
            {
                int idx = System.Array.IndexOf(_renderer.materials, material);

                Color newMainColor = Color.Lerp(startMain[idx], _enabledMainColor, t);
                Color newSecColor = Color.Lerp(startSec[idx], _enabledSecColor, t);
                Color newFresnelColor = Color.Lerp(startFresnel[idx], _enabledFresnelColor, t);

                material.SetColor("_Color", newMainColor);
                material.SetColor("_SecColor", newSecColor);
                material.SetColor("_FresnelColor", newFresnelColor);
            }

            yield return null;
        }

        foreach (var material in _renderer.materials)
        {
            material.SetColor("_Color", _enabledMainColor);
            material.SetColor("_SecColor", _enabledSecColor);
            material.SetColor("_FresnelColor", _enabledFresnelColor);
        }
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player) && _enabled)
        {
            GetComponentInChildren<ParticleSystem>().Play();
            //Vector3 checkPointPos = player.transform.position;
            player.SetCheckPointPos(_renderer.transform.position);
            StartCoroutine(SetMaterialsCoroutine());
            
            //if (_taskManager != null) _taskManager.CloseDoor();

            if (_objectToActivate != null)
                _objectToActivate.SetActive(true);
        }
    }
}
