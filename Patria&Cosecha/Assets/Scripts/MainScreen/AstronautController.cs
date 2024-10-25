using System;
using System.Collections;
using UnityEngine;

public class AstronautController : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private Array _anims = default;
    private bool _isAnimating = false;

    void Start()
    {
        _anims = Enum.GetValues(typeof(Animations));
    }

    void Update()
    {
        if (!_isAnimating) StartCoroutine(SetRandomAnim());
    }

    private IEnumerator SetRandomAnim()
    {
        _isAnimating = true;

        int randomAnim = UnityEngine.Random.Range(0, _anims.Length);
        
        yield return new WaitForSeconds(7f);

        if (randomAnim == 0) _animator.SetTrigger("Flip");
        else if (randomAnim == 1) _animator.SetTrigger("Jump");
        else if (randomAnim == 2) _animator.SetTrigger("Fall");
        else if (randomAnim == 3) _animator.SetTrigger("Surprise");

        _isAnimating = false;
    }

    private enum Animations
    {
        Flip,
        Jump,
        Fall,
        Surprise
    }
}
