using UnityEngine;
using UnityEngine.UI;

public class UpdateCrosshair : UpdatePosToTarget
{
    [SerializeField] Image _circleImage;

    private Animator _myAnim;
    private Glitcheable _currentTarget;

    protected override void Awake()
    {
        base.Awake();

        _myAnim = GetComponent<Animator>();
        _myAnim.speed = 1f;
        ResetPos();
    }

    // El crosshair solo señala glitcheables: antes le llegaban como fallback de OnInteractableDetected,
    // ahora tienen su propia señal. Se fija por código para no depender del enum del prefab.
    protected override UITargetSource Source => UITargetSource.Glitcheable;

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
        // Se muestra si Set o Take pueden mover carga: misma regla que usa el HUD de acciones
        // (PlayerController.ResolveGlitchHudTarget), para que los dos no se contradigan. Sin nodo
        // en mano CurrentGlitch es null y Preview devuelve NoSource/NoTarget.
        var node = PlayerNodeHandler.Instance.CurrentGlitch;
        var obj = glitcheable.Glitch;

        var canTransfer =
            GlitchTransferManager.Preview(node, obj) == GlitchTransferResult.Transferred ||
            GlitchTransferManager.Preview(obj, node) == GlitchTransferResult.Transferred;

        SetVisual(canTransfer);
        _circleImage.color = GlitchPalette.Default.ColorFor(glitcheable.Level);
    }

    public void SetUpdateAnim()
    {
        _myAnim.SetBool("IsActivated", true);
    }
}
