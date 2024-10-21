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
        _view.Enabled(_combineMachine.IsActive);
    }

    public void ActivateCombineMachine()
    {
        _combineMachine.CombineNodes();
    }
}
