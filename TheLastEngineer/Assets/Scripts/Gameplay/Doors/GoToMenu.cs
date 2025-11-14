using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMenu : MonoBehaviour
{
    [SerializeField] private DoorsView _door;
    [SerializeField] private string _scene = "MainMenu";

    private Collider _coll;

    private void Awake()
    {
        _coll = GetComponent<Collider>();
        _door.OnOpen += EnableCollider;

        EnableCollider(false);
    }

    private void EnableCollider(bool isOpen) => _coll.enabled = isOpen;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerController player))
            SceneManager.LoadScene(_scene);
    }
}
