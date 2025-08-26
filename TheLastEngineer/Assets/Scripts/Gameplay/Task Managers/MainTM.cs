using UnityEngine;

public class MainTM : TaskManager
{
    [SerializeField] private ParticleSystem _ps;
    [SerializeField] private Light _light;

    [Header("Level")]
    [SerializeField] private AudioSource _winAS;

    private Animator _animator = default;

    public static MainTM Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        OnAwake();
        //Cursor.visible = false;

        _animator = GetComponent<Animator>();
        onRunning += OpenDoor;
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
        foreach (var c in connections) c.SetMainTM();
    }

    private void OpenDoor(bool isRunning)
    {
        _animator.SetBool("Open", isRunning);
    }
}
