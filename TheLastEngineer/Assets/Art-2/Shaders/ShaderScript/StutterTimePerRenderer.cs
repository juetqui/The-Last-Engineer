using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StutterTimePerRenderer : MonoBehaviour
{
    [Min(1)] public int framesPorPaso = 6;
    [Min(1f)] public float pasosPorSegundo = 8f;
    public string shaderFloatName = "_SteppedTime";

    int _frameCount;
    float _steppedTime;
    Renderer _rend;
    MaterialPropertyBlock _mpb;

    void Awake()
    {
        _rend = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
    }

    void Update()
    {
        if (_frameCount % framesPorPaso == 0)
        {
            _steppedTime = Mathf.Floor(Time.time * pasosPorSegundo) / pasosPorSegundo;
            _rend.GetPropertyBlock(_mpb);
            _mpb.SetFloat(shaderFloatName, _steppedTime);
            _rend.SetPropertyBlock(_mpb);
        }
        _frameCount++;
    }
}
