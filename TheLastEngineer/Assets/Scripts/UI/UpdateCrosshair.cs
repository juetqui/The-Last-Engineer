using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class UpdateCrosshair : UpdatePosToTarget
{
    [SerializeField] Image _circleImage;

    [SerializeField] private Color defaultColor;
    [SerializeField] private Color glitchColor;

    [Header("Crosshair Scale")]
    [SerializeField] private float targetMin = 0.01f;
    [SerializeField] private float targetMax = 1f;
    [SerializeField] private float easeTime = 0.5f;
    [SerializeField] private float easeDelay = 0.25f;
    [SerializeField] private Ease easeType = Ease.OutBack;

    private Animator _myAnim;
    private Glitcheable _currentTarget;

    protected override void Awake()
    {
        base.Awake();

        _myAnim = GetComponent<Animator>();
        _myAnim.speed = 1f;
        ResetPos();
    }

    // El circulo no vive en el mismo objeto que el componente: la base tiene que mover su rect.
    protected override RectTransform ResolveRectToMove() => _circleImage.rectTransform;

    protected override void OnTargetChanged(IInteractable target)
    {
        _currentTarget = target as Glitcheable;

        _myAnim.SetBool("IsActivated", false);
        _myAnim.SetBool("HasTarget", _currentTarget != null);

        if (_currentTarget == null || PlayerNodeHandler.Instance.CurrentType == NodeType.None)
            ResetPos();
    }

    protected override void OnPositionUpdated(Vector3 screenPosition)
    {
        if (_currentTarget == null) return;

        CompareGlitchWithPlayerNode(_currentTarget);
    }

    protected override void OnTargetUnavailable() => ResetPos();

    private void ResetPos()
    {
        SetCircleEnabled(false);
        _circleImage.rectTransform.position = Vector3.zero;

        _myAnim.SetBool("IsActivated", false);
        _myAnim.SetBool("HasTarget", false);
    }

    private void CompareGlitchWithPlayerNode(Glitcheable glitcheable)
    {
        var compatible =
            (PlayerNodeHandler.Instance.CurrentType == NodeType.Corrupted && glitcheable.IsCorrupted) ||
            (PlayerNodeHandler.Instance.CurrentType == NodeType.Default && !glitcheable.IsCorrupted);

        SetCircleEnabled(!compatible);
        _circleImage.color = glitcheable.IsCorrupted ? glitchColor : defaultColor;
    }

    private void SetCircleEnabled(bool value)
    {
        if (_circleImage.enabled == value) return;

        Tween.StopAll(onTarget: _circleImage.rectTransform);

        if (value)
        {
            _circleImage.enabled = true;
            Tween.Scale(_circleImage.rectTransform, targetMax, easeTime, easeType, 1, CycleMode.Restart, easeDelay);
        }
        else
        {
            Tween.Scale(_circleImage.rectTransform, targetMin, easeTime, easeType, 1, CycleMode.Restart, easeDelay)
                .OnComplete(() => _circleImage.enabled = false);
        }
    }

    public void SetUpdateAnim()
    {
        _myAnim.SetBool("IsActivated", true);
    }
}
