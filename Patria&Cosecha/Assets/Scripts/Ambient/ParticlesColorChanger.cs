using UnityEngine;
using UnityEngine.VFX;

public class ParticlesColorChanger : MonoBehaviour
{
    private VisualEffect _vfx;

    void Start()
    {
        _vfx = GetComponent<VisualEffect>();
        MainTM.Instance.onRunning += StartLerp;
    }

    private void StartLerp(bool isRunning)
    {
        _vfx.SetBool("IsOpened", isRunning);
    }
}
