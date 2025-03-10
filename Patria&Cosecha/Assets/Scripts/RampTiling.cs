using UnityEngine;

public class RampTiling : MonoBehaviour
{
    private Renderer _renderer;
    private Vector4 _tiling = new Vector4(3, 3, 0, 0);

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.material.mainTextureScale = _tiling;
    }
}
