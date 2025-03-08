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
        gameObject.SetActive(false);
        _menuCamera.PlayAnimationCredits();
    }
    public void Exit()
    {
        gameObject.SetActive(false);
        _astronautController.SetTarget(true);
        _menuCamera.PlayAnimationExit();
    }

    public void Back()
    {
        gameObject.SetActive(true);
        SceneManager.LoadScene("Menu");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger de salir app");
        Application.Quit();
    }
}
