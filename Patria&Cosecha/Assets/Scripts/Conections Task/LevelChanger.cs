using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    [SerializeField] private string _lvlName = default;
    [SerializeField] private bool _isMenu = false;

    private MainTM _mainTM = default;

    public void SetTM(MainTM mainTM)
    {
        _mainTM = mainTM;
    }

    private void OnTriggerEnter(Collider coll)
    {
        PlayerTDController player = coll.GetComponent<PlayerTDController>();
        AstronautController menuPlayer = coll.GetComponent<AstronautController>();

        Debug.Log(_isMenu);

        if (menuPlayer != null && _isMenu)
        {
            Debug.Log("Change Lvl");
            SceneManager.LoadScene(_lvlName);
        }
        else if (player != null && _mainTM.Running) SceneManager.LoadScene(_lvlName);

    }
}
