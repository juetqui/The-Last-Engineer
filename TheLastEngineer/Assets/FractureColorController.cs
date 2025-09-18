using System.Collections.Generic;
using UnityEngine;

public class FractureColorController : MonoBehaviour
{
    [SerializeField] private Transform _parent;
    [SerializeField] private List<Color> _colors;

    private Renderer _renderer = default;

    void Start()
    {
        _colors = new List<Color>();
        _colors.Add(Color.white);
        _colors.Add(Color.black);
        _colors.Add(Color.magenta);

        _renderer = GetComponent<Renderer>();

        int randomColor = Random.Range(0, _colors.Count);

        _renderer.material.SetColor("_MainColor", _colors[randomColor]);
        _renderer.material.SetVector("_Center", _parent.position);
    }

}
