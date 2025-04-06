using UnityEngine;

public class CombinerController : MonoBehaviour, IInteractable
{
    public InteractablePriority Priority => InteractablePriority.Low;
    public Transform Transform => transform;

    [SerializeField] private CombineMachine _combineMachine;

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

    public bool CanInteract(PlayerTDController player)
    {
        return !player.HasNode() && _combineMachine.IsActive;
    }

    public void Interact(PlayerTDController player, out bool succededInteraction)
    {
        if (CanInteract(player))
        {
            _combineMachine.CombineNodes();
            succededInteraction = true;
        }
        else
        {
            succededInteraction = false;
        }

    }

    //public void ActivateCombineMachine()
    //{
    //    if (!_combineMachine.IsActive) return;

    //    _combineMachine.CombineNodes();
    //}
}
