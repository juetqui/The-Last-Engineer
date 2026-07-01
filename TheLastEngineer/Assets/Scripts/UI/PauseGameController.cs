using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseGameController : MonoBehaviour
{
    [SerializeField] private GameObject _resumeBtn;

    private Canvas _canvas = default;
    private CanvasButtonStateController _canvasController = default;

    private bool _isPaused = false;

    private void Start()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.enabled = false;

        // The CanvasButtonStateController has a bool that enables or disables the buttons in the Awake method.
        // In this case the pause menu will never be enabled at the start,
        // so the bool will be set to false in the prefab to avoid calling the method again here.
        _canvasController = GetComponent<CanvasButtonStateController>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        EventSystem.current.firstSelectedGameObject = null;
        InputManager.Instance.pauseInput.started += PauseGame;
    }

    private void PauseGame(InputAction.CallbackContext context)
    {
        _isPaused = !_isPaused;
        SetTimescale(_isPaused);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

    }

    public void ResumeGame()
    {
        _isPaused = false;
        SetTimescale(_isPaused);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    public void RestartLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        GoToScene(currentScene);
    }

    public void GoToScene(string scene)
    {
        SetTimescale(false);
        LevelLoader.Instance.SetScene(scene);
    }

    public void GoToDesktop()
    {
        Application.Quit();
    }

    private void SetTimescale(bool isPaused)
    {
        Time.timeScale = isPaused ? 0.0f : 1.0f;
        _canvas.enabled = isPaused;

        if (isPaused)
        {
            EventSystem.current.firstSelectedGameObject = _resumeBtn;
            EventSystem.current.SetSelectedGameObject(_resumeBtn);
            _canvasController.EnableButtons();
            InputManager.Instance.UpdateActionMap(ActionMaps.PauseUI);
        }
        else
        {
            EventSystem.current.firstSelectedGameObject = null;
            EventSystem.current.SetSelectedGameObject(null);
            _canvasController.DisableButtons();
            InputManager.Instance.UpdateToLastActionMap();
        }
    }
}
