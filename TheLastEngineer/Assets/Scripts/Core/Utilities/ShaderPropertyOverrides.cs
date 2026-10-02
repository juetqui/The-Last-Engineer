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
/// Los overrides se agrupan en sets, uno por submaterial: un set con indice -1 aplica a todos los
/// materiales del objeto y uno con indice >= 0 solo a ese material, pisando al general.
///
/// Funciona sobre un Renderer o sobre un DecalProjector de URP, con dos tecnicas distintas:
/// - Renderer: MaterialPropertyBlock por submaterial. No instancia materiales. Limitaciones: no
///   varia keywords, el renderer sale del SRP Batcher y un bloque por material ignora el MPB
///   general del renderer (ej. el de TubeLight) en ese material.
/// - DecalProjector: no es un Renderer y URP agrupa los decals por material con un MPB interno
///   compartido, asi que la unica forma de variar uno solo es darle una copia del material.
///   La copia vive solo en memoria (HideFlags.DontSave) y al guardar la escena/prefab se le
///   devuelve al projector el material original, para que en disco nunca quede la copia.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class ShaderPropertyOverrides : MonoBehaviour, ISerializationCallbackReceiver
{
    /// <summary>Indice de set que aplica a todos los submateriales.</summary>
    public const int AllMaterials = -1;

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

    /// <summary>Overrides de un submaterial (o de todos, con indice -1).</summary>
    [Serializable]
    public class MaterialOverrideSet
    {
        public int materialIndex = AllMaterials;
        public List<ShaderOverride> overrides = new List<ShaderOverride>();

        /// <summary>Escribe los overrides habilitados; devuelve true si escribio alguno.</summary>
        public bool WriteTo(MaterialPropertyBlock mpb)
        {
            bool wrote = false;
            foreach (ShaderOverride o in overrides)
            {
                if (!IsActive(o)) continue;
                o.WriteTo(mpb);
                wrote = true;
            }
            return wrote;
        }

        public void WriteTo(Material material)
        {
            foreach (ShaderOverride o in overrides)
            {
                if (IsActive(o)) o.WriteTo(material);
            }
        }

        private static bool IsActive(ShaderOverride o) => o != null && o.enabled && !string.IsNullOrEmpty(o.name);
    }

    [SerializeField] private List<MaterialOverrideSet> _materialSets = new List<MaterialOverrideSet>();
    // Legacy: formato anterior a los sets (una sola lista para un indice). Solo se leen para
    // migrar escenas viejas en OnAfterDeserialize; quedan vacios al guardar de nuevo.
    [SerializeField, HideInInspector] private int _materialIndex = AllMaterials;
    [SerializeField, HideInInspector] private List<ShaderOverride> _overrides = new List<ShaderOverride>();
    // Solo decals: el material original del projector. Se serializa porque mientras el componente
    // esta activo el projector apunta a la copia, y un duplicado del objeto hereda esa copia ajena.
    [SerializeField, HideInInspector] private Material _decalSourceMaterial;

    private Renderer _renderer;
    private DecalProjector _decal;
    private MaterialPropertyBlock _mpb;
    private Material _decalInstance;
    // Submateriales que tienen un bloque nuestro, para limpiarlos cuando dejan de tener overrides.
    private readonly HashSet<int> _appliedIndices = new HashSet<int>();

    public List<MaterialOverrideSet> MaterialSets => _materialSets;
    public Renderer Renderer => _renderer != null ? _renderer : (_renderer = GetComponent<Renderer>());
    public DecalProjector Decal => _decal != null ? _decal : (_decal = GetComponent<DecalProjector>());
    // El Renderer tiene prioridad: un DecalProjector nunca convive con uno en el mismo objeto.
    public bool IsDecal => Renderer == null && Decal != null;
    public bool HasTarget => Renderer != null || Decal != null;
    /// <summary>False si el projector ya no apunta a la copia (le cambiaron el material a mano).</summary>
    public bool IsDecalInstanceAssigned => _decalInstance != null && Decal != null && _decal.material == _decalInstance;

    private void OnEnable()
    {
#if UNITY_EDITOR
        // Al salir de Play la escena se recarga desde el backup, donde el projector quedo con el
        // material en null (la copia es DontSave) y URP ya le puso su default. Aplicar aca tomaria
        // ese default como fuente: se posterga a EnteredEditMode (ver OnPlayModeStateChanged).
        if (IsDecal && !Application.isPlaying && SessionState.GetBool(ReturningFromPlayKey, false)) return;
#endif
        Apply();
    }

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
    /// Renderer: el sharedMaterial de ese indice (el primero si es -1). Decal: el original.
    /// </summary>
    public Material GetSourceMaterial(int materialIndex)
    {
        if (Renderer != null)
        {
            Material[] materials = _renderer.sharedMaterials;
            int index = Mathf.Max(0, materialIndex);
            return index < materials.Length ? materials[index] : null;
        }
        if (Decal == null) return null;
        Material current = _decal.material;
        return current != null && !IsMemoryInstance(current) ? current : _decalSourceMaterial;
    }

    /// <summary>Primer set con ese indice, o null. El inspector evita que haya dos iguales.</summary>
    public MaterialOverrideSet FindSet(int materialIndex)
    {
        foreach (MaterialOverrideSet set in _materialSets)
        {
            if (set != null && set.materialIndex == materialIndex) return set;
        }
        return null;
    }

    #region Renderer

    private void ApplyToRenderer()
    {
        _mpb ??= new MaterialPropertyBlock();
        int materialCount = _renderer.sharedMaterials.Length;
        MaterialOverrideSet allSet = FindSet(AllMaterials);

        // Siempre un bloque por submaterial, ya combinado: un bloque por material no se suma al
        // general del renderer, lo reemplaza, asi que no se pueden usar los dos a la vez.
        for (int i = 0; i < materialCount; i++)
        {
            // Clear en vez de Get: asi un override desactivado o borrado vuelve al valor del material.
            _mpb.Clear();
            bool wrote = allSet != null && allSet.WriteTo(_mpb);
            // El set especifico va despues para pisar al general en las propiedades que comparten.
            MaterialOverrideSet ownSet = FindSet(i);
            if (ownSet != null) wrote |= ownSet.WriteTo(_mpb);

            if (wrote)
            {
                _renderer.SetPropertyBlock(_mpb, i);
                _appliedIndices.Add(i);
            }
            else if (_appliedIndices.Remove(i))
            {
                _renderer.SetPropertyBlock(_mpb, i);
            }
        }
        // Si le sacaron materiales al renderer, esos indices ya no tienen bloque que limpiar.
        _appliedIndices.RemoveWhere(i => i >= materialCount);
    }

    private void ClearRendererBlock()
    {
        if (Renderer == null || _appliedIndices.Count == 0) return;

        _mpb ??= new MaterialPropertyBlock();
        _mpb.Clear();
        int materialCount = _renderer.sharedMaterials.Length;
        foreach (int i in _appliedIndices)
        {
            if (i < materialCount) _renderer.SetPropertyBlock(_mpb, i);
        }
        _appliedIndices.Clear();
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

        // El projector tiene un solo material: valen el set general y el del indice 0, en ese orden.
        FindSet(AllMaterials)?.WriteTo(_decalInstance);
        FindSet(0)?.WriteTo(_decalInstance);

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

    #region Serializacion

    public void OnBeforeSerialize() { }

    // Escenas/prefabs guardados antes de los sets: la lista vieja pasa a un set con su indice.
    // Corre en cada carga hasta que se guarde de nuevo, y es idempotente.
    public void OnAfterDeserialize()
    {
        if (_overrides == null || _overrides.Count == 0) return;
        _materialSets ??= new List<MaterialOverrideSet>();
        if (_materialSets.Count == 0)
            _materialSets.Add(new MaterialOverrideSet { materialIndex = _materialIndex, overrides = _overrides });
        _overrides = new List<ShaderOverride>();
        _materialIndex = AllMaterials;
    }

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
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    // SessionState sobrevive al domain reload que hay entre salir de Play y volver a Edit Mode.
    private const string ReturningFromPlayKey = "ShaderPropertyOverrides.ReturningFromPlay";

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            SessionState.SetBool(ReturningFromPlayKey, true);
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            SessionState.EraseBool(ReturningFromPlayKey);
            // Con la escena ya restaurada, se le devuelve al projector el original serializado
            // (descartando el default de URP) y recien ahi se reconstruye la copia.
            foreach (ShaderPropertyOverrides o in FindObjectsByType<ShaderPropertyOverrides>(FindObjectsSortMode.None))
            {
                if (!o.isActiveAndEnabled || !o.IsDecal) continue;
                if (o._decalSourceMaterial != null && o._decal.material != o._decalInstance)
                    o._decal.material = o._decalSourceMaterial;
                o.Apply();
            }
        }
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
