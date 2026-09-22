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

        if (_currentTarget == null || !PlayerNodeHandler.Instance.HasNode)
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
        // El gate de HasNode es el que antes hacía NodeType.None: sin nodo en mano, Clean haría
        // pasar la segunda condición y el crosshair se mostraría como compatible.
        var compatible = PlayerNodeHandler.Instance.HasNode &&
            ((PlayerNodeHandler.Instance.CurrentLevel == GlitchState.Glitched && glitcheable.IsCorrupted) ||
             (PlayerNodeHandler.Instance.CurrentLevel == GlitchState.Clean && !glitcheable.IsCorrupted));

        SetVisual(!compatible);
        _circleImage.color = glitcheable.IsCorrupted ? glitchColor : defaultColor;
    }

    public void SetUpdateAnim()
    {
        _myAnim.SetBool("IsActivated", true);
    }
}
