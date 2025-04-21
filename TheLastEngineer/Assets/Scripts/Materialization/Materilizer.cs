using UnityEngine;

public class Materilizer : MonoBehaviour, IMaterializable
{
    private Collider _collider = default;
    private MeshRenderer _renderer = default;
    private Material _originalMat = default, _newMat = default, _currentMat = default;

    private void Start()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<MeshRenderer>();
        _originalMat = _renderer.material;

        MaterializeController.Instance.OnMaterialize += Materialize;
    }

    public void Materialize(bool materialize, Material material)
    {
        _collider.enabled = materialize;


        if (!materialize)
        {
            if (_newMat == null)
            {
                _newMat = material;
            }

            _currentMat = _newMat;
        }
        else
        {
            _currentMat = _originalMat;
        }

        _renderer.material = _currentMat;
    }
}
