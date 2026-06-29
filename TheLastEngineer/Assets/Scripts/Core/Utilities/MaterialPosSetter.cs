using UnityEngine;

public class MaterialPosSetter : MonoBehaviour
{
    [SerializeField] private Vector2 targetMinMaxPos;

    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.material.SetVector("_minMaxPos", targetMinMaxPos);
    }
}
