using UnityEngine;

public class Config : MonoBehaviour
{
    private void Awake()
    {
        var ControlsTutorial = Instantiate(Resources.Load<ScreenControlsTutorial>("Controls_Tutorial"));
        ScreenManager.Instance.Push(ControlsTutorial);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            var screenPause = Instantiate(Resources.Load<ScreenPause>("Pause_Menu"));
            ScreenManager.Instance.Push(screenPause);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            ScreenManager.Instance.Pop();
        }
    }
}
