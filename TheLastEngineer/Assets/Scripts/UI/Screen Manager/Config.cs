using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Config : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenuPrefab;

    ScreenPause screenPause = default;
    string menuName = default;

    private void Awake()
    {
        menuName = SceneManager.GetActiveScene().name;
        InputManager.Instance.pauseInput.performed += PauseMenu;
    }
    void PauseMenu(InputAction.CallbackContext context)
    {
        if (menuName == "Menu") return;

        if (screenPause == null)
        {
            screenPause = Instantiate(_pauseMenuPrefab).GetComponent<ScreenPause>();
            ScreenManager.Instance.Push(screenPause);
            return;
        }

        Cursor.visible = false;
        ScreenManager.Instance.Pop();
    }
}
