using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    [SerializeField] private TaskManager _taskManager = default;
    [SerializeField] private string _nextLvl;

    private void OnTriggerEnter(Collider coll)
    {
        PlayerTDController player = coll.GetComponent<PlayerTDController>();

        if (player != null && _taskManager.Running) SceneManager.LoadScene(_nextLvl);
    }
}
