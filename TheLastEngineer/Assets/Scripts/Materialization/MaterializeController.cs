using System;
using UnityEngine;

public class MaterializeController : MonoBehaviour
{
    [SerializeField] private PlayerTDController _player;
    [SerializeField] private Material _material;

    public static MaterializeController Instance;
    public Action<bool, Material> OnMaterialize;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        _player.OnNodeGrabed += Materialize;
        OnMaterialize?.Invoke(false, _material);
    }

    private void Materialize(bool materialize)
    {
        OnMaterialize?.Invoke(materialize, _material);
    }
}
