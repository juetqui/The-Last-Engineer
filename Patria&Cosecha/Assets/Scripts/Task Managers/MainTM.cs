using UnityEngine;

public class MainTM : TaskManager
{
    [SerializeField] private ParticleSystem _ps;
    [SerializeField] private Light _light;

    [Header("Level")]
    [SerializeField] private LevelChanger _lvlChanger;
    [SerializeField] private AudioSource _winAS;

    private void Awake()
    {
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
        if (_running) if (_light.intensity < 100) _light.intensity += 25 * Time.deltaTime;
    }

    protected override void OnAllNodesConnected()
    {
        _ps.Play();
        _source.Play();
        _winAS.Play();
    }

    protected override void SetUp()
    {
        foreach (var connection in connections) connection.SetMainTM(this);
        foreach (var door in _doors) door.SetMainTM(this);

        _lvlChanger.SetTM(this);
    }
}
