using UnityEngine;

public class MainTM : TaskManager
{
    [SerializeField] private ParticleSystem _ps;
    [SerializeField] private Light _light;

    [Header("Level")]
    [SerializeField] private LevelChanger _lvlChanger;
    [SerializeField] private DoorLights _doorLights;
    [SerializeField] private AudioSource _winAS;
    [SerializeField] private ConnectionModuleController _moduleController;

    public static MainTM Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        OnAwake();
        Cursor.visible = false;
    }

    private void Start()
    {
        OnStart();
        SetUp();
        _ps.Stop();
        _light.intensity = 0;
    }

    private void Update()
    {
        if (_running && _light.intensity < 30) _light.intensity += 5 * Time.deltaTime;
    }

    protected override void OnAllNodesConnected()
    {
        _ps.Play();
        _source.Play();
        _winAS.Play();
    }

    protected override void SetUp()
    {
        foreach (var door in _doors) door.SetMainTM(this);
        foreach (var c in connections) c.SetMainTM(this);
    }
}
