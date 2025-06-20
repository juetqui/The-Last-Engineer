using UnityEngine;
using MaskTransitions;

public class LevelChanger : MonoBehaviour
{
    [SerializeField] private string _lvlName = default;

    private BoxCollider _collider;


    private void Start()
    {
        _collider = GetComponent<BoxCollider>();

        if (MainTM.Instance != null)
            MainTM.Instance.onRunning += EnableCollider;

        EnableCollider(false);
    }

    public void EnableCollider(bool isRunning)
    {
        _collider.enabled = isRunning;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerTDController player))
            SwitchScene();
    }

    public void SwitchScene()
    {
        TransitionManager.Instance.LoadLevel(_lvlName);
    }
}
