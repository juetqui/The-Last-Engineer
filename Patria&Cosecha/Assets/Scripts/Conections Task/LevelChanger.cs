using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    [SerializeField] private string _lvlName = default;

    private MainTM _mainTM = default;

    public void SetTM(MainTM mainTM)
    {
        _mainTM = mainTM;
    }

    private void OnTriggerEnter(Collider coll)
    {
        PlayerTDController player = coll.GetComponent<PlayerTDController>();

        if (player != null && _mainTM.Running) SceneManager.LoadScene(_lvlName);
    }
}
