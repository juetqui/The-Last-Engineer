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
        _menuCamera.PlayAnimationPlay();
    }
    public void Credits()
    {
        //SceneManager.LoadScene("Credits");
        gameObject.SetActive(false);
        _menuCamera.PlayAnimationCredits();
    }
    public void Exit()
    {
        gameObject.SetActive(false);
        _astronautController.SetTarget();
        _menuCamera.PlayAnimationExit();
    }
}
