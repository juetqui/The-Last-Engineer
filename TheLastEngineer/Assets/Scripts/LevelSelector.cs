using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SceneManager.LoadScene("TESTING");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene("Lvl Inicial");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SceneManager.LoadScene("Lvl 9");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SceneManager.LoadScene("Lvl 10 B");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SceneManager.LoadScene("Lvl 11");
        }
        //else if (Input.GetKeyDown(KeyCode.Alpha4))
        //{
        //    SceneManager.LoadScene("Lvl 12");
        //}
        //else if (Input.GetKeyDown(KeyCode.Alpha5))
        //{
        //    SceneManager.LoadScene("Lvl 5");
        //}
        //else if (Input.GetKeyDown(KeyCode.Alpha6))
        //{
        //    SceneManager.LoadScene("Lvl 6");
        //}
        //else if (Input.GetKeyDown(KeyCode.Alpha7))
        //{
        //    SceneManager.LoadScene("Lvl 7");
        //}
        //else if (Input.GetKeyDown(KeyCode.Alpha8))
        //{
        //    SceneManager.LoadScene("Lvl 8");
        //}
        //else if (Input.GetKeyDown(KeyCode.Alpha9))
        //{
        //    SceneManager.LoadScene("NEW Camera Testing");
        //}
    }
}
