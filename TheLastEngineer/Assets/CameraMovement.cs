using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private InputManager _inputManager = default;

    void Start()
    {
        _inputManager = InputManager.Instance;
    }

    void Update()
    {
        if (_inputManager == null || _inputManager.playerInputs == null || !_inputManager.playerInputs.Player.enabled) return;

        Vector2 moveVector = _inputManager.rotateInput.ReadValue<Vector2>();
        MoveCamera(moveVector);
    }

    private void MoveCamera(Vector2 moveDir)
    {

    }
}
