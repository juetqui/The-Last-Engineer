using UnityEngine;

public class DetectPlayerToOpen : MonoBehaviour
{
    private Animator _animator = default;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player))
            _animator.SetBool("IsPlayerNear", true);
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player))
            _animator.SetBool("IsPlayerNear", false);
    }
}
