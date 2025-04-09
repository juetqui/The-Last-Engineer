using UnityEngine;

public class PressuredDoor : MonoBehaviour
{
    [SerializeField] private Material _commonMat;
    [SerializeField] private Material _successMat;
    [SerializeField] private PressurePlate[] _plates;

    private MeshRenderer _renderer;
    private int _pressedPlates = 0;

    void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        _renderer.material = _commonMat;

        foreach (var plate in _plates)
        {
            plate.OnPlayerPressed += CheckPlates;
        }
    }

    void Update()
    {
        if (_pressedPlates == _plates.Length)
        {
            _renderer.material = _successMat;
        }
        else if (_pressedPlates == 0)
        {
            _renderer.material = _commonMat;
        }
    }

    private void CheckPlates(bool pressed)
    {
        if (pressed)
        {
            _pressedPlates++;
        }
        else
        {
            _pressedPlates--;
        }
    }
}
