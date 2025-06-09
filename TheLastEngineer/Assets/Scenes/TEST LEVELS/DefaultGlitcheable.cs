public class DefaultGlitcheable : Glitcheable
{
    private void Awake()
    {
        OnAwake();
    }

    void Start()
    {
        PlayerTDController.Instance.OnNodeGrabed += CheckNode;
        GlitchActive.Instance.OnStopObject += StopObject;
    }

    void Update()
    {
        if (_canMove && !_isStopped)
        {
            StartCoroutine(StartTimer());
        }
    }
}
