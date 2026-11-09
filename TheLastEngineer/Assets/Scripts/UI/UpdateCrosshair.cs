using UnityEngine;
using UnityEngine.UI;

public class UpdateCrosshair : UpdatePosToTarget
{
    [SerializeField] Image _circleImage;

    private Animator _myAnim;
    private IGlitchSwapTarget _currentTarget;

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
        _currentTarget = target as IGlitchSwapTarget;

        _myAnim.SetBool("IsActivated", false);
        _myAnim.SetBool("HasTarget", _currentTarget != null);

        if (_currentTarget == null || !PlayerNodeHandler.Instance.HasNode)
            ResetPos();
    }

    protected override void OnPositionUpdated(Vector3 screenPosition)
    {
        // Through Object so a destroyed target also counts as null (the interface skips Unity's ==).
        if (_currentTarget as Object == null) return;

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

    private void CompareGlitchWithPlayerNode(IGlitchSwapTarget target)
    {
        // Shown when Set or Take can swap: same rule as the action HUD
        // (PlayerController.ResolveSwapHudTarget), so both never disagree. With no node in hand
        // CurrentGlitch is null and the previews return NoSource.
        var node = PlayerNodeHandler.Instance.CurrentGlitch;

        var canSwap =
            GlitchTransferManager.PreviewSet(node, target) == GlitchTransferResult.Swapped ||
            GlitchTransferManager.PreviewTake(node, target) == GlitchTransferResult.Swapped;

        SetVisual(canSwap);
        if (target.Glitch != null) _circleImage.color = GlitchPalette.Default.ColorFor(target.Glitch.CurrentState);
    }

    public void SetUpdateAnim()
    {
        _myAnim.SetBool("IsActivated", true);
    }
}
