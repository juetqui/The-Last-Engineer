using UnityEngine;
using UnityEngine.SceneManagement;
using MaskTransitions;

public class LevelChanger : MonoBehaviour
{
    [SerializeField] private string _lvlName = default;
    [SerializeField] private bool _isMenu = false;

    private void OnTriggerEnter(Collider coll)
    {
        PlayerTDController player = coll.GetComponent<PlayerTDController>();
        AstronautController menuPlayer = coll.GetComponent<AstronautController>();

        if (menuPlayer != null && _isMenu) SceneManager.LoadScene(_lvlName);
        else if (player != null) SwitchScene();

    }

    public void SwitchScene()
    {
        TransitionManager.Instance.LoadLevel(_lvlName);
    }
}
