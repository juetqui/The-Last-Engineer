using UnityEngine;
using UnityEngine.SceneManagement;

public class Config : MonoBehaviour
{
    ScreenPause screenPause = default;
    string menuName = default;

    private void Awake()
    {
        menuName = SceneManager.GetActiveScene().name;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && menuName != "Menu")
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
