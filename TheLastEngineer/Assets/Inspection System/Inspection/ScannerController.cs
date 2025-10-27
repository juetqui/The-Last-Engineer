using System;
using UnityEngine;

public class ScannerController : MonoBehaviour
{
    public static ScannerController Instance = null;

    [SerializeField] private float _targetScale = 6f;
    [SerializeField] private float _targetTime = 1f;

    private CorruptionGenerator _corruptionGenerator = default;

    private float _timer = 0f;
    private bool _scan = false;

    public Action OnScanFinished = delegate { };

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null;
        }

        Instance = this;
    }

    void Start()
    {
        InspectorController.Instance.OnTargetEnabled += ListenToTarget;
    }

    void Update()
    {
        if (_scan) StartScanning();
    }

    private void ListenToTarget(UIInspectionable inspectionable)
    {
        _corruptionGenerator = inspectionable.CorruptionGenerator;
        _corruptionGenerator.OnObjectCleaned += SetScan;
    }

    private void SetScan(CorruptionGenerator corruptionGenerator)
    {
        if (corruptionGenerator != _corruptionGenerator) return;

        InspectionSystem.Instance.OnResetRot += SetUpScanning;
        GamepadCursor.Instance.CenterCursor();
        InspectionSystem.Instance.ResetRot();

        _corruptionGenerator.OnObjectCleaned -= SetScan;
        _corruptionGenerator = null;
    }

    private void SetUpScanning()
    {
        _scan = true;
    }

    private void StartScanning()
    {
        _timer += Time.deltaTime;
        
        Vector3 newScale = Vector3.one * (_targetScale * _timer);

        transform.localScale = newScale;

        if (_timer >= _targetTime)
        {
            InspectionSystem.Instance.OnResetRot -= SetUpScanning;

            _scan = false;
            transform.localScale = Vector3.one;
            OnScanFinished?.Invoke();
        }
    }
}
