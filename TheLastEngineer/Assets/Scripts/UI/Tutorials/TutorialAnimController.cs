using UnityEngine;

public class TutorialAnimController : MonoBehaviour
{
    [SerializeField] private Animator _anim;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
            _anim.SetTrigger("Show");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
            _anim.SetTrigger("Hide");
    }
}
