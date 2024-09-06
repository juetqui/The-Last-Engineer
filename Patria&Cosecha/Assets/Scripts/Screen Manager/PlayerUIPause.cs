using UnityEngine;
using UnityEngine.UI;

public class PlayerUIPause : MonoBehaviour, IScreen
{
    Button[] _buttons;

    string _result;

    private void Awake()
    {
        _buttons = GetComponentsInChildren<Button>();
    }

    public void BTN_Pause()
    {
        _result = "Pause Button";
        
        var screenPause = Instantiate(Resources.Load<ScreenPause>("Pause_Menu"));
        ScreenManager.Instance.Push(screenPause);
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
