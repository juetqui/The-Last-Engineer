using UnityEngine;
public class DoorsView : MonoBehaviour
{
    private Animator _animator = default;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    public void OpenDoor(bool isRunning)
    {
        _animator.SetBool("Open", isRunning);
    }
}
