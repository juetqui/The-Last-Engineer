using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

/// <summary>
/// Sobrescribe propiedades del shader SOLO para este objeto, sin tocar el .mat compartido.
/// Corre tambien en Edit Mode, asi se pueden comparar configuraciones del mismo material en
/// varios objetos a la vez sin entrar a Play. Los valores quedan serializados en la escena/prefab
/// y se mantienen en Play y en build.
///
/// Funciona sobre un Renderer o sobre un DecalProjector de URP, con dos tecnicas distintas:
/// - Renderer: MaterialPropertyBlock. No instancia materiales. Limitaciones: no varia keywords,
///   el renderer sale del SRP Batcher y pisa cualquier otro MPB del mismo renderer (ej. TubeLight).
/// - DecalProjector: no es un Renderer y URP agrupa los decals por material con un MPB interno
///   compartido, asi que la unica forma de variar uno solo es darle una copia del material.
///   La copia vive solo en memoria (HideFlags.DontSave) y al guardar la escena/prefab se le
///   devuelve al projector el material original, para que en disco nunca quede la copia.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class ShaderPropertyOverrides : MonoBehaviour
{
    public enum OverrideType { Float, Range, Color, Vector, Texture, Int }

    [Serializable]
    public class ShaderOverride
    {
        public string name;
        public OverrideType type;
        public bool enabled = true;
        public float floatValue;
        [ColorUsage(true, true)] public Color colorValue = Color.white;
        public Vector4 vectorValue;
        public Texture textureValue;
        public int intValue;

        // El id se cachea en runtime; si se renombra la propiedad se recalcula.
        [NonSerialized] private string _cachedName;
        [NonSerialized] private int _id;

        public int Id
        {
            get
            {
                if (_cachedName != name)
                {
                    _cachedName = name;
                    _id = Shader.PropertyToID(name);
                }
                return _id;
            }
        }

        public void WriteTo(MaterialPropertyBlock mpb)
        {
            switch (type)
            {
                case OverrideType.Float:
                case OverrideType.Range: mpb.SetFloat(Id, floatValue); break;
                case OverrideType.Color: mpb.SetColor(Id, colorValue); break;
                case OverrideType.Vector: mpb.SetVector(Id, vectorValue); break;
                // Un MPB no acepta textura null: sin textura se deja la del material.
                case OverrideType.Texture: if (textureValue != null) mpb.SetTexture(Id, textureValue); break;
                case OverrideType.Int: mpb.SetInteger(Id, intValue); break;
            }
        }

        public void WriteTo(Material material)
        {
            switch (type)
            {
                case OverrideType.Float:
                case OverrideType.Range: material.SetFloat(Id, floatValue); break;
                case OverrideType.Color: material.SetColor(Id, colorValue); break;
                case OverrideType.Vector: material.SetVector(Id, vectorValue); break;
                // Mismo criterio que el MPB: sin textura se deja la del material.
                case OverrideType.Texture: if (textureValue != null) material.SetTexture(Id, textureValue); break;
                case OverrideType.Int: material.SetInteger(Id, intValue); break;
            }
        }
    }

    [Tooltip("Solo Renderer. -1 = aplica a todos los submateriales. >= 0 = solo al material de ese indice.")]
    [SerializeField] private int _materialIndex = -1;
    [SerializeField] private List<ShaderOverride> _overrides = new List<ShaderOverride>();
    // Solo decals: el material original del projector. Se serializa porque mientras el componente
    // esta activo el projector apunta a la copia, y un duplicado del objeto hereda esa copia ajena.
    [SerializeField, HideInInspector] private Material _decalSourceMaterial;

    private Renderer _renderer;
    private DecalProjector _decal;
    private MaterialPropertyBlock _mpb;
    private Material _decalInstance;
    // Indice sobre el que se aplico el ultimo bloque, para limpiarlo si cambia _materialIndex.
    private int _appliedIndex = int.MinValue;

    public int MaterialIndex => _materialIndex;
    public List<ShaderOverride> Overrides => _overrides;
    public Renderer Renderer => _renderer != null ? _renderer : (_renderer = GetComponent<Renderer>());
    public DecalProjector Decal => _decal != null ? _decal : (_decal = GetComponent<DecalProjector>());
    // El Renderer tiene prioridad: un DecalProjector nunca convive con uno en el mismo objeto.
    public bool IsDecal => Renderer == null && Decal != null;
    public bool HasTarget => Renderer != null || Decal != null;
    /// <summary>False si el projector ya no apunta a la copia (le cambiaron el material a mano).</summary>
    public bool IsDecalInstanceAssigned => _decalInstance != null && Decal != null && _decal.material == _decalInstance;

    private void OnEnable() => Apply();

    // Apagar o quitar el componente devuelve el objeto al look de su material.
    private void OnDisable() => ClearOverrides();

    // Cubre ediciones desde el inspector, undo/redo y cambios hechos por animacion en editor.
    private void OnValidate()
    {
        if (!isActiveAndEnabled) return;
#if UNITY_EDITOR
        // Con decals Apply puede crear/destruir la copia del material, y en OnValidate Unity no
        // permite DestroyImmediate: se posterga al siguiente tick del editor.
        if (IsDecal && !Application.isPlaying)
        {
            EditorApplication.delayCall -= DelayedApply;
            EditorApplication.delayCall += DelayedApply;
            return;
        }
#endif
        Apply();
    }

    /// <summary>Vuelve a escribir todos los overrides habilitados en el objeto.</summary>
    public void Apply()
    {
        if (Renderer != null) ApplyToRenderer();
        else if (Decal != null) ApplyToDecal();
    }

    /// <summary>Saca los overrides, restaurando los valores del material.</summary>
    public void ClearOverrides()
    {
        ClearRendererBlock();
        ReleaseDecalInstance();
    }

    /// <summary>
    /// Material del que se leen las propiedades disponibles y los valores por defecto.
    /// Renderer: el sharedMaterial del indice elegido (el primero si es -1). Decal: el original.
    /// </summary>
    public Material GetSourceMaterial()
    {
        if (Renderer != null)
        {
            Material[] materials = _renderer.sharedMaterials;
            int index = Mathf.Max(0, _materialIndex);
            return index < materials.Length ? materials[index] : null;
        }
        if (Decal == null) return null;
        Material current = _decal.material;
        return current != null && !IsMemoryInstance(current) ? current : _decalSourceMaterial;
    }

    #region Renderer

    private void ApplyToRenderer()
    {
        _mpb ??= new MaterialPropertyBlock();

        if (_appliedIndex != int.MinValue && _appliedIndex != _materialIndex) ClearRendererBlock();

        // Clear en vez de Get: asi un override desactivado o borrado vuelve al valor del material.
        _mpb.Clear();
        foreach (ShaderOverride o in _overrides)
        {
            if (o == null || !o.enabled || string.IsNullOrEmpty(o.name)) continue;
            o.WriteTo(_mpb);
        }

        if (_materialIndex < 0) _renderer.SetPropertyBlock(_mpb);
        else _renderer.SetPropertyBlock(_mpb, _materialIndex);
        _appliedIndex = _materialIndex;
    }

    private void ClearRendererBlock()
    {
        if (Renderer == null || _appliedIndex == int.MinValue) return;

        _mpb ??= new MaterialPropertyBlock();
        _mpb.Clear();
        if (_appliedIndex < 0) _renderer.SetPropertyBlock(null);
        else _renderer.SetPropertyBlock(_mpb, _appliedIndex);
        _appliedIndex = int.MinValue;
    }

    #endregion

    #region Decal

    private void ApplyToDecal()
    {
        Material current = _decal.material;

        // Si el projector no apunta a nuestra copia: o le asignaron otro material a mano (nueva
        // fuente), o es un duplicado que heredo la copia de otro objeto (se conserva la fuente).
        if (current != _decalInstance)
        {
            if (current != null && !IsMemoryInstance(current)) _decalSourceMaterial = current;
            DestroyDecalInstance();
        }
        if (_decalSourceMaterial == null) return;

        if (_decalInstance == null)
        {
            _decalInstance = new Material(_decalSourceMaterial)
            {
                name = _decalSourceMaterial.name + " (Overrides)",
                hideFlags = HideFlags.DontSave
            };
        }
        else
        {
            // Partir siempre del original: un override desactivado o borrado vuelve a su valor.
            _decalInstance.shader = _decalSourceMaterial.shader;
            _decalInstance.CopyPropertiesFromMaterial(_decalSourceMaterial);
        }

        foreach (ShaderOverride o in _overrides)
        {
            if (o == null || !o.enabled || string.IsNullOrEmpty(o.name)) continue;
            o.WriteTo(_decalInstance);
        }

        if (_decal.material != _decalInstance) _decal.material = _decalInstance;
#if UNITY_EDITOR
        DecalsWithInstance.Add(this);
#endif
    }

    /// <summary>Devuelve al projector su material original y destruye la copia.</summary>
    private void ReleaseDecalInstance()
    {
        if (_decalInstance == null) return;
        RestoreDecalSource();
        DestroyDecalInstance();
    }

    private void RestoreDecalSource()
    {
        if (Decal != null && _decalInstance != null && _decal.material == _decalInstance)
            _decal.material = _decalSourceMaterial;
    }

    private void DestroyDecalInstance()
    {
#if UNITY_EDITOR
        DecalsWithInstance.Remove(this);
#endif
        if (_decalInstance == null) return;
        if (Application.isPlaying) Destroy(_decalInstance);
        else DestroyImmediate(_decalInstance);
        _decalInstance = null;
    }

    private static bool IsMemoryInstance(Material material) => (material.hideFlags & HideFlags.DontSave) != 0;

    #endregion

#if UNITY_EDITOR
    // Decals con copia activa, para devolverles el material original antes de guardar.
    private static readonly HashSet<ShaderPropertyOverrides> DecalsWithInstance = new HashSet<ShaderPropertyOverrides>();

    private void DelayedApply()
    {
        EditorApplication.delayCall -= DelayedApply;
        if (this != null && isActiveAndEnabled) Apply();
    }

    // Si la copia se serializara, la escena/prefab quedaria con el material en null (DontSave).
    // Por eso se guarda con el original y justo despues se vuelve a poner la copia.
    [InitializeOnLoadMethod]
    private static void RegisterSaveHooks()
    {
        EditorSceneManager.sceneSaving += (scene, _) => ForEachDecalIn(scene, o => o.RestoreDecalSource());
        EditorSceneManager.sceneSaved += scene => ForEachDecalIn(scene, o => o.Apply());
        PrefabStage.prefabSaving += root => ForEachDecalIn(root.scene, o => o.RestoreDecalSource());
        PrefabStage.prefabSaved += root => ForEachDecalIn(root.scene, o => o.Apply());
    }

    private static void ForEachDecalIn(Scene scene, Action<ShaderPropertyOverrides> action)
    {
        // Copia: Apply puede modificar el set mientras se recorre.
        foreach (ShaderPropertyOverrides o in new List<ShaderPropertyOverrides>(DecalsWithInstance))
        {
            if (o != null && o.gameObject.scene == scene) action(o);
        }
    }
#endif
}
