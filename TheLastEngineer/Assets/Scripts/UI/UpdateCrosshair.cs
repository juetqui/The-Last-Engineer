using UnityEngine;
using UnityEngine.UI;

public class UpdateCrosshair : UpdatePosToTarget
{
    [SerializeField] Image _circleImage;

    [SerializeField] private Color defaultColor;
    [SerializeField] private Color glitchColor;

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

    // El reset de posicion espera a que la base termine de achicar el circulo: si lo hicieramos al
    // pedir el ocultado, el circulo saltaria a la esquina de la pantalla mientras se encoge.
    protected override void OnVisualHidden() => _circleImage.rectTransform.position = Vector3.zero;

    private void ResetPos()
    {
        SetVisual(false);

        _myAnim.SetBool("IsActivated", false);
        _myAnim.SetBool("HasTarget", false);
    }

    private void CompareGlitchWithPlayerNode(Glitcheable glitcheable)
    {
        var compatible =
            (PlayerNodeHandler.Instance.CurrentType == NodeType.Corrupted && glitcheable.IsCorrupted) ||
            (PlayerNodeHandler.Instance.CurrentType == NodeType.Default && !glitcheable.IsCorrupted);

        SetVisual(!compatible);
        _circleImage.color = glitcheable.IsCorrupted ? glitchColor : defaultColor;
    }

    public void SetUpdateAnim()
    {
        _myAnim.SetBool("IsActivated", true);
    }
}
