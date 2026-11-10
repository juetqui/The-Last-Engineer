using UnityEngine;
using Unity.Cinemachine;

// Blends the camera from the pose it was frozen at (death spot) to the pose Cinemachine computes
// now. The composer's own damping is exponential and stops inside its dead zone, so it can neither
// guarantee a fixed duration nor end centered; this extension only overrides the final state while
// a pan is running and leaves every component value untouched. Timing lives in FollowController.
public class RespawnCameraPan : CinemachineExtension
{
    private Vector3 _frozenPosition;
    private Quaternion _frozenOrientation = Quaternion.identity;
    private bool _isPanning;

    // 0 = frozen pose, 1 = pose computed by the pipeline.
    public float Weight { get; set; }

    public void Begin(Vector3 frozenPosition, Quaternion frozenOrientation)
    {
        _frozenPosition = frozenPosition;
        _frozenOrientation = frozenOrientation;
        Weight = 0f;
        _isPanning = true;
    }

    public void End()
    {
        _isPanning = false;
        Weight = 1f;
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (!_isPanning || stage != CinemachineCore.Stage.Finalize) return;

        var weight = Mathf.Clamp01(Weight);
        var position = Vector3.Lerp(_frozenPosition, state.GetFinalPosition(), weight);
        var orientation = Quaternion.Slerp(_frozenOrientation, state.GetFinalOrientation(), weight);

        state.RawPosition = position;
        state.PositionCorrection = Vector3.zero;
        state.RawOrientation = orientation;
        state.OrientationCorrection = Quaternion.identity;
    }
}
