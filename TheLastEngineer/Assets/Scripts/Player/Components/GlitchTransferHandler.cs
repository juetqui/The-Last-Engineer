using System;
using UnityEngine;

/// <summary>
/// Turns the Set and Take inputs into state swaps between the node in hand and the swap target
/// (Glitcheable or Intangible) the player is looking at. Interact is only for physical
/// manipulation, so this is the only way the player moves glitch. Tap only: the swap runs on press.
/// </summary>
[DisallowMultipleComponent]
public class GlitchTransferHandler : MonoBehaviour
{
    private enum TransferDirection
    {
        Set,    // node -> object
        Take    // object -> node
    }

    private PlayerController _player;
    private PlayerNodeHandler _nodeHandler;
    private InputHandler _input;
    private PlayerData _data;

    // Cached so the target filter doesn't allocate a closure per press.
    private Func<IGlitchSwapTarget, bool> _canSetFilter;
    private Func<IGlitchSwapTarget, bool> _canTakeFilter;

    /// <summary>Result of every attempt, so the HUD and audio can react without polling.</summary>
    public Action<GlitchTransferResult, IGlitchSwapTarget> OnTransferResolved;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
        _nodeHandler = GetComponent<PlayerNodeHandler>();
        _input = GetComponent<InputHandler>();

        _canSetFilter = t => GlitchTransferManager.PreviewSet(_nodeHandler.CurrentGlitch, t) == GlitchTransferResult.Swapped;
        _canTakeFilter = t => GlitchTransferManager.PreviewTake(_nodeHandler.CurrentGlitch, t) == GlitchTransferResult.Swapped;
    }

    private void Start()
    {
        _data = _player.Data;
        Hook(true);
    }

    private void OnDestroy() => Hook(false);

    private void Hook(bool enable)
    {
        if (_input == null) return;

        if (enable)
        {
            _input.OnSetStart += OnSetStart;
            _input.OnTakeStart += OnTakeStart;
        }
        else
        {
            _input.OnSetStart -= OnSetStart;
            _input.OnTakeStart -= OnTakeStart;
        }
    }

    private void OnSetStart() => TrySwap(TransferDirection.Set);
    private void OnTakeStart() => TrySwap(TransferDirection.Take);

    private void TrySwap(TransferDirection direction)
    {
        // Same gate as OnInteractPressed: no interaction in the air or mid-dash.
        if (_player.IsDead || !_player.CC.isGrounded) return;

        var node = _nodeHandler.CurrentGlitch;

        if (node == null)
        {
            ReportFailure(GlitchTransferResult.NoSource, null);
            return;
        }

        // The filter keeps a false error away when two objects are in range and the closest one
        // is exactly the one that can't take part in this swap.
        var target = _player.FindTransferTarget(direction == TransferDirection.Set ? _canSetFilter : _canTakeFilter);

        var result = direction == TransferDirection.Set
            ? GlitchTransferManager.Set(node, target)
            : GlitchTransferManager.Take(node, target);

        if (result != GlitchTransferResult.Swapped)
        {
            // With no valid target, report why against the node alone (Clean node on Set, etc.).
            ReportFailure(target == null ? ExplainMissingTarget(direction, node) : result, target);
            return;
        }

        Rumble(_data.transferInitialRumble, _data.rumbleDuration);
        OnTransferResolved?.Invoke(result, target);
    }

    private static GlitchTransferResult ExplainMissingTarget(TransferDirection direction, GlitchComponent node)
    {
        if (direction == TransferDirection.Set && node.CurrentState == GlitchState.Clean) return GlitchTransferResult.NodeClean;
        if (direction == TransferDirection.Take && node.CurrentState != GlitchState.Clean) return GlitchTransferResult.NodeNotClean;

        return GlitchTransferResult.NoTarget;
    }

    private void ReportFailure(GlitchTransferResult result, IGlitchSwapTarget target)
    {
        _player.View.PlayErrorSound(_data.emptyHand);
        Rumble(_data.transferErrorRumble, _data.transferErrorRumbleDuration);

        if (target != null) target.NotifyRejected();

        OnTransferResolved?.Invoke(result, target);
    }

    private void Rumble(Vector2 intensity, float duration)
    {
        InputManager.Instance?.RumblePulse(intensity.x, intensity.y, duration);
    }
}
