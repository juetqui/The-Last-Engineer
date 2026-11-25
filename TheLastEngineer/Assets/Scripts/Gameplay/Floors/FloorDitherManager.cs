using System;
using PrimeTween;
using UnityEngine;

/// <summary>
/// Publishes which floors are visible as shader globals read by the floor-dither Sub Graph:
/// <c>_FloorCutHeights</c> (world Y where each floor ends) and <c>_FloorFade</c> (0 = visible, 1 = hidden,
/// one per floor). Any material whose shader includes the Sub Graph reacts, without touching objects.
/// Runs in edit mode only to preview the dither from the Inspector; gameplay logic stays Play-only.
/// </summary>
[ExecuteAlways]
public class FloorDitherManager : MonoBehaviour
{
    public static FloorDitherManager Instance { get; private set; }

    private const int FloorCount = 4;
    private const int CutCount = FloorCount - 1;
    private const float GizmoPlaneSize = 60f;

    private static readonly int CutHeightsId = Shader.PropertyToID("_FloorCutHeights");
    private static readonly int FadeId = Shader.PropertyToID("_FloorFade");

    [Header("Alturas de corte (Y de mundo) entre pisos, de abajo hacia arriba:\nUnderground|Base, Base|First, First|Second.")]
    [SerializeField] private float[] floorCutHeights = new float[CutCount];
    [SerializeField] private FloorHeight startFloor = FloorHeight.Base;

    [Header("Transicion")]
    [SerializeField] private Ease ease;
    [SerializeField, Min(0f)] private float easeDuration;

    [Header("Opacidad segun piso")]
    [SerializeField, Range(0f, 1f)] private float targetLowerFloorOpacity;
    [SerializeField, Range(0f, 1f)] private float targetUpperFloorOpacity;

    [Header("Preview en editor (fuera de Play Mode)")]
    [SerializeField] private bool previewInEditor;
    [SerializeField] private FloorHeight previewFloor = FloorHeight.Base;

    private readonly float[] _fadeBuffer = new float[FloorCount];
    private Vector4 _currentFade;
    private Vector4 _tweenFrom;
    private Vector4 _tweenTo;
    private Tween _fadeTween;

    private void Awake()
    {
        if (!Application.isPlaying) return;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (!Application.isPlaying) return;

        Shader.SetGlobalVector(CutHeightsId, BuildCutHeights());
        SetFade(TargetFade(startFloor));
    }

    private void OnEnable()
    {
        if (!Application.isPlaying) ApplyEditorPreview();
    }

    private void OnDisable()
    {
        _fadeTween.Stop();
        // Leave every floor visible so other scenes (or this one after the manager goes away) are not dithered.
        SetFade(Vector4.zero);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void DitherFloor(FloorHeight floor)
    {
        if (!isActiveAndEnabled) return;

        _fadeTween.Stop();
        var target = TargetFade(floor);

        if (easeDuration <= 0f)
        {
            SetFade(target);
            return;
        }

        _tweenFrom = _currentFade;
        _tweenTo = target;
        _fadeTween = Tween.Custom(this, 0f, 1f, easeDuration,
            (manager, t) => manager.SetFade(Vector4.LerpUnclamped(manager._tweenFrom, manager._tweenTo, t)), ease);
    }

    private Vector4 TargetFade(FloorHeight floor)
    {
        FloorFadeRules.Compute((int)floor, targetLowerFloorOpacity, targetUpperFloorOpacity, _fadeBuffer);
        return new Vector4(_fadeBuffer[0], _fadeBuffer[1], _fadeBuffer[2], _fadeBuffer[3]);
    }

    private void SetFade(Vector4 fade)
    {
        _currentFade = fade;
        Shader.SetGlobalVector(FadeId, fade);
    }

    private Vector4 BuildCutHeights()
    {
        // w is unused: there is no cut above the top floor.
        return new Vector4(floorCutHeights[0], floorCutHeights[1], floorCutHeights[2], float.MaxValue);
    }

    private void OnValidate()
    {
        if (floorCutHeights == null || floorCutHeights.Length != CutCount)
            Array.Resize(ref floorCutHeights, CutCount);

        for (var i = 1; i < CutCount; i++)
        {
            if (floorCutHeights[i] < floorCutHeights[i - 1])
                Debug.LogWarning($"{name}: las alturas de corte deben ir de menor a mayor (indice {i}).", this);
        }

        if (Application.isPlaying)
        {
            if (Instance == this) Shader.SetGlobalVector(CutHeightsId, BuildCutHeights());
        }
        else if (isActiveAndEnabled)
        {
            ApplyEditorPreview();
        }
    }

    // Edit mode only. With the preview off every floor is left visible, as in a scene without a manager.
    // Entering Play Mode disables this instance first (reset), then Start applies startFloor.
    private void ApplyEditorPreview()
    {
        if (previewInEditor)
        {
            Shader.SetGlobalVector(CutHeightsId, BuildCutHeights());
            SetFade(TargetFade(previewFloor));
        }
        else
        {
            SetFade(Vector4.zero);
        }

#if UNITY_EDITOR
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    private void OnDrawGizmosSelected()
    {
        if (floorCutHeights == null) return;

        Gizmos.color = Color.cyan;
        var center = transform.position;
        foreach (var height in floorCutHeights)
            Gizmos.DrawWireCube(new Vector3(center.x, height, center.z), new Vector3(GizmoPlaneSize, 0f, GizmoPlaneSize));
    }
}
