using System.Collections;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [SerializeField] private float _minFirstTimer = 0.125f;
    [SerializeField] private float _maxFirstTimer = 1f;
    [SerializeField] private float _minSecTimer = 0.125f;
    [SerializeField] private float _maxSecTimer = 0.25f;

    private Light _light = default;
    private bool _isFlicking = false;

    void Start()
    {
        _light = GetComponent<Light>();
        StartCoroutine(FlickLight());
    }

    private IEnumerator FlickLight()
    {
        float firstTimer = Random.Range(_minFirstTimer, _maxFirstTimer);
        float secTimer = Random.Range(_minSecTimer, _maxSecTimer);
        
        _light.enabled = true;
        yield return new WaitForSeconds(firstTimer);
        _light.enabled = false;
        yield return new WaitForSeconds(secTimer);
        StartCoroutine(FlickLight());
    }
}
