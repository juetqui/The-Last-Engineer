using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    [SerializeField] private TaskManager _taskManager = default;

    private void OnTriggerEnter(Collider coll)
    {
        PlayerTDController player = coll.GetComponent<PlayerTDController>();

        if (player != null && _taskManager.Running) SceneManager.LoadScene("Lvl 2");

        Debug.Log(SceneManager.GetActiveScene().name);

    }
}
