using UnityEngine;
using UnityEngine.UI;

public class ScreenControlsTutorial : MonoBehaviour, IScreen
{
    Button[] _buttons;

    string _result;

    private void Awake()
    {
        _buttons = GetComponentsInChildren<Button>();
    }

    public void BTN_Close()
    {
        _result = "Close Button";

        ScreenManager.Instance.Pop();
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

        return _result;
    }
}
