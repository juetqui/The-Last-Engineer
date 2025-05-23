using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenPause : MonoBehaviour, IScreen
{
    Button[] _buttons;

    string _result;

    private void Awake()
    {
        _buttons = GetComponentsInChildren<Button>();

        foreach (var button in _buttons)
        {
            button.interactable = false;
        }

    }

    public void Start()
    {
        Cursor.visible = false;
    }

    public void BTN_Back()
    {
        _result = "Back Button";

        ScreenManager.Instance.Pop();
    }

    public void BTN_Pause()
    {
        _result = "Pause Button";

        ScreenManager.Instance.Push("Pause_Menu");
    }

    public void BTN_Menu()
    {
        _result = "Menu Button";

        ScreenManager.Instance.Pop();
        SceneManager.LoadScene("Menu");
        Cursor.visible = true;
    }

    public void BTN_ResetLevel()
    {
        _result = "Reset Button";
        ScreenManager.Instance.Pop();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Activate()
    {
        foreach (var button in _buttons)
        {
            button.interactable = true;
        }
    }

    public void Deactivate()
    {
        foreach (var button in _buttons)
        {
            button.interactable = false;
        }
    }

    public string Free()
    {
        Destroy(gameObject);
        Cursor.visible = false;

        return _result;
    }
}
