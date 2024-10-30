using UnityEngine;
using UnityEngine.SceneManagement;

public class ManejadorEscena : MonoBehaviour
{
    [SerializeField] AstronautController _astronautController;

    public void Menu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void Game()
    {
        gameObject.SetActive(false);
        _astronautController.SetTarget();
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
