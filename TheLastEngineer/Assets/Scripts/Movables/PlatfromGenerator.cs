using UnityEngine;

public class PlatfromGenerator : MonoBehaviour
{
    [SerializeField] Material _ActiveMat;
    [SerializeField] Material _DisabledMat;

    private SecondaryTM _secTM = default;
    private MeshRenderer _renderer = default;
    private BoxCollider _collider = default;

    void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<BoxCollider>();
        _secTM = GetComponent<SecondaryTM>();

        _secTM.onRunning += GeneratePlatform;
        GeneratePlatform(false);
    }

    private void GeneratePlatform(bool isRunning)
    {
        if (isRunning)
            _renderer.material = _ActiveMat;
        else _renderer.material = _DisabledMat;

        _collider.enabled = isRunning;
    }
}
