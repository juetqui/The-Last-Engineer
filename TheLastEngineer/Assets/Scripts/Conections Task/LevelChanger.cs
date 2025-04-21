using UnityEngine;
using MaskTransitions;

public class LevelChanger : MonoBehaviour
{
    [SerializeField] private string _lvlName = default;
    [SerializeField] private bool _isMenu = false;

    private BoxCollider _collider;


    private void Start()
    {
        _collider = GetComponent<BoxCollider>();
        MainTM.Instance.onRunning += EnableCollider;
        
        EnableCollider(false);
    }

    private void EnableCollider(bool isRunning)
    {
        _collider.enabled = isRunning;
    }

    private void OnTriggerEnter(Collider coll)
    {
        PlayerTDController player = coll.GetComponent<PlayerTDController>();
        AstronautController menuPlayer = coll.GetComponent<AstronautController>();

        if (menuPlayer != null && _isMenu) SwitchScene();
        else if (player != null) SwitchScene();

    }

    public void SwitchScene()
    {
        TransitionManager.Instance.LoadLevel(_lvlName);
    }
}
