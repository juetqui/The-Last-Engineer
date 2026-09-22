using System;
using UnityEngine;

/// <summary>
/// Traduce los inputs Set y Take en transferencias de carga entre el nodo en mano y el glitcheable
/// que el jugador esté mirando. Interact quedó exclusivo de manipulación física, así que esta es
/// la única vía por la que el jugador mueve glitch.
///
/// Tap = una carga. Hold = esa primera carga y después una cada transferRepeatInterval, mientras
/// las capacidades lo permitan. El tiempo se mide acá y no con interactions del Input System, que
/// es el mismo patrón que ya usa el hold de PlayerEmptyState.
/// </summary>
[DisallowMultipleComponent]
public class GlitchTransferHandler : MonoBehaviour
{
    private enum TransferDirection
    {
        Set,    // del nodo al objeto
        Take    // del objeto al nodo
    }

    private PlayerController _player = default;
    private PlayerNodeHandler _nodeHandler = default;
    private InputHandler _input = default;
    private PlayerData _data = default;

    private bool _active = false;
    private bool _continuous = false;
    private TransferDirection _direction = TransferDirection.Set;
    private Glitcheable _target = default;
    private float _holdTimer = 0f;
    private float _repeatTimer = 0f;

    /// <summary>Resultado de cada intento, para que el HUD y el audio reaccionen sin consultar por frame.</summary>
    public Action<GlitchTransferResult, Glitcheable> OnTransferResolved;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
        _nodeHandler = GetComponent<PlayerNodeHandler>();
        _input = GetComponent<InputHandler>();
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
            _input.OnSetCancel += OnSetCancel;
            _input.OnTakeStart += OnTakeStart;
            _input.OnTakeCancel += OnTakeCancel;
        }
        else
        {
            _input.OnSetStart -= OnSetStart;
            _input.OnSetCancel -= OnSetCancel;
            _input.OnTakeStart -= OnTakeStart;
            _input.OnTakeCancel -= OnTakeCancel;
        }
    }

    private void OnSetStart() => BeginHold(TransferDirection.Set);
    private void OnTakeStart() => BeginHold(TransferDirection.Take);

    // Cada dirección solo cancela lo suyo: si el jugador tiene Set apretado y toca Take, el
    // canceled de Take no tiene que cortar el hold de Set.
    private void OnSetCancel() { if (_active && _direction == TransferDirection.Set) EndHold(); }
    private void OnTakeCancel() { if (_active && _direction == TransferDirection.Take) EndHold(); }

    private void BeginHold(TransferDirection direction)
    {
        // Idempotente a propósito: EnableInputs puede correr más de una vez y dejar el evento
        // suscrito dos veces, y una pulsación no puede mover dos cargas.
        if (_active) return;

        // Mismo gate que OnInteractPressed: en el aire o en pleno dash no se interactúa.
        if (_player.IsDead || !_player.CC.isGrounded) return;

        var node = _nodeHandler.CurrentGlitch;

        if (node == null)
        {
            ReportFailure(GlitchTransferResult.NoSource, null);
            return;
        }

        var target = ResolveTarget(direction);
        var result = Execute(direction, node, target);

        if (result != GlitchTransferResult.Transferred)
        {
            ReportFailure(result, target);
            return;
        }

        _active = true;
        _continuous = false;
        _direction = direction;
        _target = target;
        _holdTimer = 0f;
        _repeatTimer = 0f;

        Rumble(_data.transferInitialRumble, _data.rumbleDuration);
        OnTransferResolved?.Invoke(result, target);
    }

    private void Update()
    {
        if (!_active) return;

        if (!StillValid())
        {
            EndHold();
            return;
        }

        _holdTimer += Time.deltaTime;

        if (!_continuous)
        {
            if (_holdTimer < _data.transferHoldDelay) return;

            _continuous = true;
            _repeatTimer = 0f;
        }

        _repeatTimer += Time.deltaTime;
        if (_repeatTimer < _data.transferRepeatInterval) return;

        _repeatTimer = 0f;

        var result = Execute(_direction, _nodeHandler.CurrentGlitch, _target);

        if (result != GlitchTransferResult.Transferred)
        {
            // Quedarse sin carga (o llenar el destino) es un final limpio del hold, no un error:
            // repetir el feedback de rechazo cada tick sería insoportable.
            EndHold();
            return;
        }

        // Pulso por carga, más corto que el intervalo: con la sobrecarga de RumblePulse que lleva
        // duración, cada llamada arranca su corrutina, y si se superponen la vieja apaga los
        // motores en medio del pulso nuevo.
        Rumble(_data.transferHoldRumble, _data.transferRepeatInterval * 0.5f);
        OnTransferResolved?.Invoke(result, _target);
    }

    /// <summary>
    /// Las condiciones de corte del hold: todas se revalidan por frame porque ninguna avisa.
    /// El objetivo puede irse solo (el glitcheable se mueve), taparse detrás de una pared, o el
    /// nodo puede soltarse o enchufarse en una Connection en medio de la pulsación.
    /// </summary>
    private bool StillValid()
    {
        if (_player.IsDead) return false;
        if (_target == null) return false;
        if (_nodeHandler.CurrentGlitch == null) return false;

        return _player.IsInteractableAvailable(_target);
    }

    private Glitcheable ResolveTarget(TransferDirection direction)
    {
        // El filtro por capacidad es lo que evita el error falso cuando hay dos glitcheables en
        // rango y el más cercano es justo el que no puede dar (o recibir) carga.
        Func<Glitcheable, bool> filter;

        if (direction == TransferDirection.Set)
            filter = g => g.Glitch != null && g.Glitch.CanReceive();
        else
            filter = g => g.Glitch != null && g.Glitch.CanGive();

        return _player.FindTransferTarget(filter);
    }

    private GlitchTransferResult Execute(TransferDirection direction, GlitchComponent node, Glitcheable target)
    {
        if (node == null) return GlitchTransferResult.NoSource;
        if (target == null) return GlitchTransferResult.NoTarget;

        return direction == TransferDirection.Set
            ? GlitchTransferManager.ExecuteTransfer(node, target.Glitch)
            : GlitchTransferManager.ExecuteTransfer(target.Glitch, node);
    }

    private void EndHold()
    {
        _active = false;
        _continuous = false;
        _target = null;
        _holdTimer = 0f;
        _repeatTimer = 0f;
    }

    private void ReportFailure(GlitchTransferResult result, Glitcheable target)
    {
        _player.View.PlayErrorSound(_data.emptyHand);
        Rumble(_data.transferErrorRumble, _data.transferErrorRumbleDuration);

        // Mismo feedback visual que tenía el rechazo de Interact: el orbe del glitcheable rebota.
        if (target != null) target.OnInteractionRejected?.Invoke();

        OnTransferResolved?.Invoke(result, target);
    }

    private void Rumble(Vector2 intensity, float duration)
    {
        InputManager.Instance?.RumblePulse(intensity.x, intensity.y, duration);
    }
}
