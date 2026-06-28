using UnityEngine;

public class GoToMenu : MonoBehaviour
{
    [SerializeField] private DoorsView door;
    [SerializeField] private string scene = "MainMenu";

    private Collider _coll;

    private void Awake()
    {
        _coll = GetComponent<Collider>();
        door.OnOpen += EnableCollider;

        EnableCollider(false);
    }

    private void OnDestroy()
    {
        door.OnOpen -= EnableCollider;
    }

    private void EnableCollider(bool isOpen) => _coll.enabled = isOpen;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerController player))
        {
            LevelLoader.Instance.SetScene(scene);
        }
    }
}
