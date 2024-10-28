using UnityEngine;

public class CombinerController : MonoBehaviour
{
    [SerializeField] private CombineMachine _combineMachine;
    [SerializeField] private OpenDoor _openDoor;

    [SerializeField] private Renderer _renderer;
    [SerializeField] private Renderer _shield;

    [ColorUsage(true, true)]
    [SerializeField] private Color _noColor, _onColor, _offColor;

    private CombinerView _view = default;

    private void Awake()
    {
        _view = new CombinerView(_renderer, _shield, _noColor, _onColor, _offColor);
    }

    private void Start()
    {
        _view.OnStart();
    }

    private void Update()
    {
        _view.Enabled(_combineMachine.IsActive);
    }

    public void ActivateCombineMachine()
    {
        if (!_combineMachine.IsActive) return;

        _combineMachine.CombineNodes();
        _openDoor.Open();
    }
}
