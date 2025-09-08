using UnityEngine;

public class TimerController : MonoBehaviour
{
    [SerializeField] private float _transparencyDuration = 1f;
    [SerializeField] private float _moveDuration = 0.5f;

    public float TransparencyDuration { get; private set; }
    public float MoveDuration { get; private set; }

    private float _baseTransparency;
    private float _baseMove;

    private void Awake()
    {
        _baseTransparency = Mathf.Max(0.0001f, _transparencyDuration);
        _baseMove = Mathf.Max(0.0001f, _moveDuration);

        TransparencyDuration = _baseTransparency;
        MoveDuration = _baseMove;
    }

    public void ResetToBaseline()
    {
        TransparencyDuration = _baseTransparency;
        MoveDuration = _baseMove;
    }
}
