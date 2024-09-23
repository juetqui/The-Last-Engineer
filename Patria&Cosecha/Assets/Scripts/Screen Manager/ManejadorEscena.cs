using UnityEngine;
using UnityEngine.SceneManagement;

public class ManejadorEscena : MonoBehaviour
{
    public void Menu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void Game()
    {
        SceneManager.LoadScene("Level1");
    }
    public void Credits()
    {
        SceneManager.LoadScene("Credits");
    }
    public void Exit()
    {
        Application.Quit();
    }
}
