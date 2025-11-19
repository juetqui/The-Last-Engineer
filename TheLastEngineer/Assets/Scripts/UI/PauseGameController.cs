using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseGameController : MonoBehaviour
{
    [SerializeField] private GameObject _resumeBtn;

    private Canvas _canvas = default;
    private bool _isPaused = false;

    private void Start()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.enabled = false;

        EventSystem.current.firstSelectedGameObject = null;
        InputManager.Instance.pauseInput.started += PauseGame;
    }

    private void PauseGame(InputAction.CallbackContext context)
    {
        _isPaused = !_isPaused;
        SetTimescale(_isPaused);
    }

    public void ResumeGame()
    {
        _isPaused = false;
        SetTimescale(_isPaused);
    }

    public void RestartLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        GoToScene(currentScene);
    }

    public void GoToScene(string scene)
    {
        SetTimescale(false);
        SceneManager.LoadScene(scene);
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
            InputManager.Instance.UpdateActionMap(ActionMaps.PauseUI);
        }
        else
        {
            EventSystem.current.firstSelectedGameObject = null;
            InputManager.Instance.UpdateToLastActionMap();
        }
    }
}
