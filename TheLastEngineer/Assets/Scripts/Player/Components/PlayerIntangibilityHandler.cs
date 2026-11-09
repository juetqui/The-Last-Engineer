using UnityEngine;

/// <summary>
/// Intangibility on the player side: with an Intangible node in hand, the CharacterController and
/// CapsuleCollider exclude PlayerData.intangibleExcludeLayers, where Intangible objects sit while in
/// their Intangible state. Those objects stay solid (and registered in PlayerInteractionDetector),
/// so Take can still target them. Needing a second Intangible node to pass is the intended puzzle.
/// </summary>
public class PlayerIntangibilityHandler
{
    private const float OverlapSkin = 0.02f;

    private readonly CharacterController _cc;
    private readonly CapsuleCollider _capsule;
    private readonly PlayerNodeHandler _nodeHandler;
    private readonly PlayerData _data;

    private bool _wantsIntangible = false;
    private bool _pendingRestore = false;

    public PlayerIntangibilityHandler(CharacterController cc, CapsuleCollider capsule, PlayerNodeHandler nodeHandler, PlayerData data)
    {
        _cc = cc;
        _capsule = capsule;
        _nodeHandler = nodeHandler;
        _data = data;
    }

    // Por evento y no por pull: OnNodeGrabbed se emite al agarrar, al soltar y en cada cambio de
    // nivel del nodo en mano, que son exactamente los momentos en que la exclusión puede cambiar.
    public void Enable()
    {
        if (_nodeHandler == null) return;

        _nodeHandler.OnNodeGrabbed += OnNodeGrabbed;
        OnNodeGrabbed(_nodeHandler.HasNode, _nodeHandler.CurrentLevel);
    }

    public void Disable()
    {
        if (_nodeHandler != null) _nodeHandler.OnNodeGrabbed -= OnNodeGrabbed;
    }

    public void Tick()
    {
        if (_pendingRestore) Apply();
    }

    private void OnNodeGrabbed(bool hasNode, GlitchState level)
    {
        _wantsIntangible = hasNode && level == GlitchState.Intangible;
        Apply();
    }

    private void Apply()
    {
        if (_wantsIntangible)
        {
            _pendingRestore = false;
            SetExcludeLayers(_data.intangibleExcludeLayers);
            return;
        }

        // Volver a colisionar con la cápsula adentro de un objeto lo dejaría trabado o lo
        // escupiría: se difiere y Tick reintenta cada frame hasta que salga.
        if (IsInsideExcluded())
        {
            _pendingRestore = true;
            return;
        }

        _pendingRestore = false;
        SetExcludeLayers(0);
    }

    private void SetExcludeLayers(LayerMask mask)
    {
        if (_cc != null) _cc.excludeLayers = mask;
        if (_capsule != null) _capsule.excludeLayers = mask;
    }

    private bool IsInsideExcluded()
    {
        if (_cc == null || _data.intangibleExcludeLayers.value == 0) return false;

        var t = _cc.transform;
        var scale = t.lossyScale;

        var radius = _cc.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
        var height = Mathf.Max(_cc.height * Mathf.Abs(scale.y), radius * 2f);
        var center = t.TransformPoint(_cc.center);
        var offset = t.up * (height * 0.5f - radius);

        // El margen evita que estar apoyado contra una cara cuente como estar adentro.
        return Physics.CheckCapsule(center + offset, center - offset, Mathf.Max(0.01f, radius - OverlapSkin),
            _data.intangibleExcludeLayers, QueryTriggerInteraction.Ignore);
    }
}
