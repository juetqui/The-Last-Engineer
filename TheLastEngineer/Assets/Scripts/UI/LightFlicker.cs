using System.Collections;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [SerializeField] private float _minOnTimer = 0.125f;
    [SerializeField] private float _maxOnTimer = 1f;
    [SerializeField] private float _minOffTimer = 0.125f;
    [SerializeField] private float _maxOffTimer = 0.25f;

    private Light _light = default;

    void Start()
    {
        _light = GetComponent<Light>();
        StartCoroutine(FlickLight());
    }

    private IEnumerator FlickLight()
    {
        float firstTimer = Random.Range(_minOnTimer, _maxOnTimer);
        float secTimer = Random.Range(_minOffTimer, _maxOffTimer);
        
        _light.enabled = true;
        yield return new WaitForSeconds(firstTimer);
        _light.enabled = false;
        yield return new WaitForSeconds(secTimer);
        StartCoroutine(FlickLight());
    }
}
