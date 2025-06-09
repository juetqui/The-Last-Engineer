public class DefaultGlitcheable : Glitcheable, ICorruptionCanceler
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

    public void CorruptionCancel()
    {
        StopObject(this);
    }
}
