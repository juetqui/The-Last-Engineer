using UnityEngine;
using UnityEngine.SceneManagement;

public class ManejadorEscena : MonoBehaviour
{
    [SerializeField] AstronautController _astronautController;
    [SerializeField] MenuCamera _menuCamera;

    public void Menu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void Game()
    {
        gameObject.SetActive(false);
        _astronautController.SetTarget();
        _menuCamera.PlayAnimation();
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
