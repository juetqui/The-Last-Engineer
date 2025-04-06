using UnityEngine;

public class PlatformActivator : MonoBehaviour
{
    public delegate void OnActivated(bool isActive);
    public event OnActivated onActivated;

    private bool _isActive = false;

    void Start()
    {
        onActivated?.Invoke(_isActive);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _isActive = !_isActive;
            onActivated?.Invoke(_isActive);
        }
    }
}
