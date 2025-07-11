using System.Collections;
using UnityEngine;

public class CheckPointController : MonoBehaviour
{
    [SerializeField] private GenericTM _taskManager;
    [SerializeField] private float _transitionDuration = 0.5f;
    
    [SerializeField] private Color _enabledMainColor;
    [SerializeField] private Color _enabledSecColor;
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _enabledFresnelColor;

    private SkinnedMeshRenderer _renderer = default;

    private void Awake()
    {
        _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    private IEnumerator SetMaterialsCoroutine()
    {
        float elapsed = 0f;

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
        if (coll.TryGetComponent(out PlayerTDController player))
        {
            GetComponentInChildren<ParticleSystem>().Play();
            //Vector3 checkPointPos = player.transform.position;
            player.SetCheckPointPos(_renderer.transform.position);
            StartCoroutine(SetMaterialsCoroutine());
            
            if (_taskManager != null) _taskManager.CloseDoor();
        }
    }
}
