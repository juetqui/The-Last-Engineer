public class PlatesGlitcheable : Glitcheable
{
    public PlateType colorType = default;

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

public enum PlateType
{
    None,
    Green,
    Blue
}
