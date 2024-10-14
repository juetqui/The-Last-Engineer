using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    [SerializeField] private TaskManager _taskManager = default;
<<<<<<< HEAD
    [SerializeField] private string _lvlName = default;
=======
    [SerializeField] private string _nextLvl;
>>>>>>> 0cb619849c36b9039c679361924fa93f85c9410c

    private void OnTriggerEnter(Collider coll)
    {
        PlayerTDController player = coll.GetComponent<PlayerTDController>();

<<<<<<< HEAD
        if (player != null && _taskManager.Running) SceneManager.LoadScene(_lvlName);
=======
        if (player != null && _taskManager.Running) SceneManager.LoadScene(_nextLvl);
>>>>>>> 0cb619849c36b9039c679361924fa93f85c9410c
    }
}
