using UnityEngine;
using UnityEngine.VFX;

public class ParticlesColorChanger : MonoBehaviour
{
    [SerializeField] private TaskManager _tm;   // asignalo en el Inspector si querés
    private VisualEffect _vfx;

    void Awake()
    {
        _vfx = GetComponent<VisualEffect>();
        if (_tm == null) _tm = FindObjectOfType<TaskManager>();
        if (_tm != null)
        {
            _tm.RunningChanged += OnRunningChanged;   // si usás onRunning, cambiar esta
            // _tm.onRunning += OnRunningChanged;
        }
    }

    void OnDestroy()
    {
        if (_tm != null)
        {
            _tm.RunningChanged -= OnRunningChanged;
            // _tm.onRunning -= OnRunningChanged;
        }
    }

    private void OnRunningChanged(bool isRunning)
    {
        if (_vfx != null)
            _vfx.SetBool("IsOpened", isRunning);
    }
}
