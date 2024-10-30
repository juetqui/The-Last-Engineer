using UnityEngine;

public class Config : MonoBehaviour
{
    ScreenPause screenPause = new ScreenPause();
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (screenPause != null)
            {
                Cursor.visible = false;
                ScreenManager.Instance.Pop();
            }
            else
            {
                screenPause = Instantiate(Resources.Load<ScreenPause>("Pause_Menu"));
                ScreenManager.Instance.Push(screenPause);
            }
        }
    }
}
