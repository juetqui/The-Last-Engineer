using System;
using UnityEngine;

/// <summary>
/// Traduce los inputs Set y Take en transferencias de carga entre el nodo en mano y el glitcheable
/// que el jugador esté mirando. Interact quedó exclusivo de manipulación física, así que esta es
/// la única vía por la que el jugador mueve glitch.
///
/// Al apretar solo se valida (el error se siente en el momento); la transferencia se decide
/// después. Soltar antes de transferHoldDelay = tap, una carga. Llegar a transferHoldDelay = hold,
/// se mueve de una sola vez el máximo posible entre los dos (1 o 2 niveles), sin paso intermedio.
/// El tiempo se mide acá y no con interactions del Input System, que es el mismo patrón que ya
/// usa el hold de PlayerEmptyState.
/// </summary>
[DisallowMultipleComponent]
public class GlitchTransferHandler : MonoBehaviour
{
    private enum TransferDirection
    {
        Set,    // del nodo al objeto
        Take    // del objeto al nodo
    }

    private PlayerController _player;
    private PlayerNodeHandler _nodeHandler;
    private InputHandler _input;
    private PlayerData _data;

    private bool _active = false;
    private TransferDirection _direction = TransferDirection.Set;
    private Glitcheable _target = default;
    private float _holdTimer = 0f;

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
    private void OnSetCancel() { if (_active && _direction == TransferDirection.Set) ReleaseAsTap(); }
    private void OnTakeCancel() { if (_active && _direction == TransferDirection.Take) ReleaseAsTap(); }

    private void BeginHold(TransferDirection direction)
    {
        // Idempotente a propósito: EnableInputs puede correr más de una vez y dejar el evento
        // suscrito dos veces, y una pulsación no puede arrancar dos holds.
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
        var result = Preview(direction, node, target);

        if (result != GlitchTransferResult.Transferred)
        {
            ReportFailure(result, target);
            return;
        }

        _active = true;
        _direction = direction;
        _target = target;
        _holdTimer = 0f;
    }

    private void Update()
    {
        if (!_active) return;

        // Perder el objetivo o el nodo en medio del hold cancela sin transferir nada.
        if (!StillValid())
        {
            EndHold();
            return;
        }

        _holdTimer += Time.deltaTime;
        if (_holdTimer < _data.transferHoldDelay) return;

        // El máximo se calcula ahora y no al apretar: el objeto pudo cambiar de nivel en el medio.
        var node = _nodeHandler.CurrentGlitch;
        var target = _target;
        var amount = MaxTransferable(_direction, node, target);
        var result = Execute(_direction, node, target, amount);

        // EndHold antes del feedback: deja _active en false para que el canceled que llega al
        // soltar el botón no dispare además un tap.
        EndHold();

        if (result != GlitchTransferResult.Transferred)
        {
            ReportFailure(result, target);
            return;
        }

        Rumble(_data.transferHoldRumble, _data.rumbleDuration);
        OnTransferResolved?.Invoke(result, target);
    }

    private void ReleaseAsTap()
    {
        var target = _target;
        var valid = StillValid();
        EndHold();

        // Si el objetivo se perdió justo en este frame, se cancela en silencio igual que en Update.
        if (!valid) return;

        var result = Execute(_direction, _nodeHandler.CurrentGlitch, target, 1);

        if (result != GlitchTransferResult.Transferred)
        {
            ReportFailure(result, target);
            return;
        }

        Rumble(_data.transferInitialRumble, _data.rumbleDuration);
        OnTransferResolved?.Invoke(result, target);
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

    private GlitchTransferResult Preview(TransferDirection direction, GlitchComponent node, Glitcheable target)
    {
        if (node == null) return GlitchTransferResult.NoSource;
        if (target == null) return GlitchTransferResult.NoTarget;

        return direction == TransferDirection.Set
            ? GlitchTransferManager.Preview(node, target.Glitch)
            : GlitchTransferManager.Preview(target.Glitch, node);
    }

    private int MaxTransferable(TransferDirection direction, GlitchComponent node, Glitcheable target)
    {
        if (node == null || target == null) return 0;

        return direction == TransferDirection.Set
            ? GlitchTransferManager.MaxTransferable(node, target.Glitch)
            : GlitchTransferManager.MaxTransferable(target.Glitch, node);
    }

    private GlitchTransferResult Execute(TransferDirection direction, GlitchComponent node, Glitcheable target, int amount)
    {
        if (node == null) return GlitchTransferResult.NoSource;
        if (target == null) return GlitchTransferResult.NoTarget;

        return direction == TransferDirection.Set
            ? GlitchTransferManager.ExecuteTransfer(node, target.Glitch, amount)
            : GlitchTransferManager.ExecuteTransfer(target.Glitch, node, amount);
    }

    private void EndHold()
    {
        _active = false;
        _target = null;
        _holdTimer = 0f;
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
