using UnityEngine;

public class CombinerController : MonoBehaviour
{
    [SerializeField] private CombineMachine _combineMachine;

    [SerializeField] private Renderer _renderer;
    [SerializeField] private Color _onColor, _offColor;

    private CombinerView _view = default;

    private void Start()
    {
        _view = new CombinerView(_renderer, _onColor, _offColor);
        _view.OnStart();
    }

    private void Update()
    {
        if (_combineMachine.IsActive) _view.Enabled();
    }

    public void ActivateCombineMachine()
    {
        Debug.Log("Combine");
        _combineMachine.CombineNodes();
    }
}
