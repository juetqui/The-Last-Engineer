using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class FloorDitherManager : MonoBehaviour
{
    public static FloorDitherManager Instance;

    private static readonly int OpacityId = Shader.PropertyToID("_Opacity");

    [Header("Materiales con dither (acomodar array en el orden desde el piso mas bajo hasta el mas alto.)")]
    [SerializeField] private Material[] materials;
    [SerializeField] private Ease ease;
    [SerializeField] private float easeDuration;

    [Header("Opacidad segun piso")]
    [SerializeField] private float targetLowerFloorOpacity;
    [SerializeField] private float targetUpperFloorOpacity;

    [Header("Oclusion para el outline del jugador (PlayerXRayFeature)")]
    [Tooltip("Rendering Layer que se pone en los pisos con opacidad reducida para que no cuenten como pared.")]
    [SerializeField] private RenderingLayerMask _ditheredOccluderLayer;
    [Tooltip("Por debajo de esta opacidad el piso deja de tapar al jugador.")]
    [SerializeField] private float _occluderOpacityThreshold = 0.99f;

    public event Action<FloorHeight> OnFloorChanged;
    public FloorHeight CurrentFloor { get; private set; }

    private List<Renderer>[] _floorRenderers;
    private Tween[] _floorTweens;
    private bool[] _floorIgnored;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _floorRenderers = new List<Renderer>[materials.Length];
        _floorTweens = new Tween[materials.Length];
        _floorIgnored = new bool[materials.Length];
        for (var i = 0; i < materials.Length; i++)
            _floorRenderers[i] = new List<Renderer>();

        CollectFloorRenderers();
    }

    private void Start()
    {
        DitherFloor(FloorHeight.Base);
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        Instance = null;
        if (_floorTweens != null)
            foreach (var t in _floorTweens) t.Stop();
    }

    public void DitherFloor(FloorHeight fh)
    {
        CurrentFloor = fh;
        var currentHeight = (int)fh;

        for (var i = 0; i < materials.Length; i++)
        {
            float targetOpacity;

            if (i == currentHeight)
                targetOpacity = 1f;
            else if (i == currentHeight - 1)
                targetOpacity = targetLowerFloorOpacity;
            else if (i == currentHeight + 1)
                targetOpacity = targetUpperFloorOpacity;
            else
                targetOpacity = 0f;

            AnimateFloor(i, targetOpacity);
        }

        OnFloorChanged?.Invoke(fh);
    }

    /// <summary>
    /// Para renderers que aparecen despues del Awake (carga aditiva, instanciados): los suma al piso
    /// y les aplica el estado de oclusion actual.
    /// </summary>
    public void RegisterFloorRenderer(Renderer r, FloorHeight fh)
    {
        var floor = (int)fh;
        if (r == null || floor < 0 || floor >= materials.Length || _floorRenderers[floor].Contains(r)) return;

        _floorRenderers[floor].Add(r);
        SetRendererIgnored(r, _floorIgnored[floor]);
    }

    public void UnregisterFloorRenderer(Renderer r, FloorHeight fh)
    {
        var floor = (int)fh;
        if (r == null || floor < 0 || floor >= materials.Length) return;

        if (_floorRenderers[floor].Remove(r))
            SetRendererIgnored(r, false);
    }

    private void AnimateFloor(int floor, float targetOpacity)
    {
        var mat = materials[floor];

        // Un tween por piso: si el jugador cruza dos FloorChanger seguidos, el anterior se corta en
        // vez de pelear por el mismo material, y su OnComplete no pisa el estado nuevo.
        _floorTweens[floor].Stop();

        var from = mat.GetFloat(OpacityId);
        var ignoredAsOccluder = targetOpacity < _occluderOpacityThreshold;

        // Asimetrico a proposito: el piso que se desvanece deja de tapar ya, y el que vuelve a ser
        // opaco recien tapa al terminar. Mientras esta a medio dither escribe depth "de a puntitos"
        // y el outline del jugador saldria punteado.
        if (ignoredAsOccluder)
            SetFloorIgnored(floor, true);

        mat.SetFloat(OpacityId, from);
        _floorTweens[floor] = Tween.Custom(gameObject, from, targetOpacity, easeDuration,
            (_, v) => mat.SetFloat(OpacityId, v), ease);

        if (!ignoredAsOccluder)
            _floorTweens[floor].OnComplete(() => SetFloorIgnored(floor, false));
    }

    private void CollectFloorRenderers()
    {
        var renderers = FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var r in renderers)
        {
            foreach (var shared in r.sharedMaterials)
            {
                var floor = Array.IndexOf(materials, shared);
                if (floor < 0) continue;

                _floorRenderers[floor].Add(r);
                break;
            }
        }
    }

    private void SetFloorIgnored(int floor, bool ignored)
    {
        _floorIgnored[floor] = ignored;

        foreach (var r in _floorRenderers[floor])
            SetRendererIgnored(r, ignored);
    }

    // Se agrega/quita solo el bit propio, sin pisar Default/Receive decals ni otros.
    private void SetRendererIgnored(Renderer r, bool ignored)
    {
        if (r == null) return;

        var mask = _ditheredOccluderLayer.value;
        r.renderingLayerMask = ignored ? r.renderingLayerMask | mask : r.renderingLayerMask & ~mask;
    }
}

public enum FloorHeight
{
    Underground = 0,
    Base = 1,
    First = 2,
    Second = 3
}
