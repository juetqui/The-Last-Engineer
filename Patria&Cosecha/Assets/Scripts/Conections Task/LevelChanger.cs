using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    [SerializeField] private TaskManager _taskManager = default;

    private void OnTriggerEnter(Collider coll)
    {
        PlayerTDController player = coll.GetComponent<PlayerTDController>();

        if (player != null && _taskManager.Running && SceneManager.GetActiveScene().name == "Lvl 1")
            SceneManager.LoadScene("Lvl 2");
        else if (SceneManager.GetActiveScene().name == "Lvl 2") SceneManager.LoadScene("Lvl 3");
    }
}
