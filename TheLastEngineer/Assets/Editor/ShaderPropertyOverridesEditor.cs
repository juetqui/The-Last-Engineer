using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Inspector de ShaderPropertyOverrides: lista las propiedades del shader del material para
/// agregarlas como override, y dibuja cada una con el campo que corresponde a su tipo.
/// Todo se edita via SerializedProperty para tener Undo y dirty de escena/prefab gratis.
/// </summary>
[CustomEditor(typeof(ShaderPropertyOverrides))]
public class ShaderPropertyOverridesEditor : UnityEditor.Editor
{
    private SerializedProperty _materialIndexProp;
    private SerializedProperty _overridesProp;

    private ShaderPropertyOverrides Target => (ShaderPropertyOverrides)target;

    private void OnEnable()
    {
        _materialIndexProp = serializedObject.FindProperty("_materialIndex");
        _overridesProp = serializedObject.FindProperty("_overrides");
        // OnValidate no siempre corre tras un undo de un cambio hecho desde este inspector.
        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnDisable() => Undo.undoRedoPerformed -= OnUndoRedo;

    private void OnUndoRedo()
    {
        if (target == null) return;
        Target.Apply();
        SceneView.RepaintAll();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        if (!Target.HasTarget)
        {
            EditorGUILayout.HelpBox("Requiere un Renderer o un DecalProjector (URP) en el mismo objeto.", MessageType.Error);
            ApplyIfChanged();
            return;
        }

        bool isDecal = Target.IsDecal;
        // Si al DecalProjector le asignaron otro material a mano, Apply lo toma como nuevo original.
        if (isDecal && Target.isActiveAndEnabled && !Target.IsDecalInstanceAssigned) Target.Apply();

        int materialCount = isDecal ? 1 : Target.Renderer.sharedMaterials.Length;
        if (!isDecal)
        {
            EditorGUILayout.IntSlider(_materialIndexProp, -1, Mathf.Max(0, materialCount - 1),
                new GUIContent("Material Index", "-1 = todos los submateriales."));
        }

        Material material = Target.GetSourceMaterial();
        if (material == null || material.shader == null)
        {
            EditorGUILayout.HelpBox(isDecal ? "El DecalProjector no tiene material." : "El renderer no tiene material en ese indice.",
                MessageType.Warning);
            ApplyIfChanged();
            return;
        }

        if (!isDecal && _materialIndexProp.intValue < 0 && materialCount > 1)
            EditorGUILayout.HelpBox("Con varios materiales, las propiedades se listan del primero.", MessageType.None);

        DrawToolbar(material);
        EditorGUILayout.HelpBox(isDecal
                ? $"Decal: usa una copia en memoria de '{material.name}' (en disco siempre queda el original). " +
                  "No apliques al prefab el override del material del DecalProjector."
                : "Solo propiedades expuestas. Las keywords (Boolean/Enum Keyword de Shader Graph) no se pueden variar por objeto.",
            MessageType.Info);

        EditorGUILayout.Space();
        DrawOverrides(material);

        ApplyIfChanged();
    }

    private void ApplyIfChanged()
    {
        if (!EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            return;
        }
        serializedObject.ApplyModifiedProperties();
        Target.Apply();
        SceneView.RepaintAll();
    }

    private void DrawToolbar(Material material)
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Agregar propiedad")) ShowAddMenu(material);
        if (GUILayout.Button("Agregar todas"))
        {
            Shader shader = material.shader;
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                string name = shader.GetPropertyName(i);
                if (IsEditable(shader, i) && FindOverride(name) < 0) AddOverride(material, i);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Resetear todas al material"))
        {
            for (int i = 0; i < _overridesProp.arraySize; i++)
                CopyFromMaterial(_overridesProp.GetArrayElementAtIndex(i), material);
        }
        if (GUILayout.Button("Limpiar")) _overridesProp.ClearArray();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawOverrides(Material material)
    {
        Shader shader = material.shader;
        for (int i = 0; i < _overridesProp.arraySize; i++)
        {
            SerializedProperty element = _overridesProp.GetArrayElementAtIndex(i);
            string name = element.FindPropertyRelative("name").stringValue;
            int shaderIndex = shader.FindPropertyIndex(name);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            SerializedProperty enabledProp = element.FindPropertyRelative("enabled");
            enabledProp.boolValue = EditorGUILayout.Toggle(enabledProp.boolValue, GUILayout.Width(16));
            string label = shaderIndex >= 0 ? $"{shader.GetPropertyDescription(shaderIndex)}  ({name})" : name;
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(shaderIndex < 0))
            {
                if (GUILayout.Button("Reset", GUILayout.Width(50))) CopyFromMaterial(element, material);
            }
            bool remove = GUILayout.Button("✕", GUILayout.Width(22));
            EditorGUILayout.EndHorizontal();

            if (shaderIndex < 0)
                EditorGUILayout.HelpBox($"'{name}' no existe en el shader actual ({shader.name}).", MessageType.Warning);

            using (new EditorGUI.DisabledScope(!enabledProp.boolValue))
            {
                DrawValue(element, shader, shaderIndex);
            }
            EditorGUILayout.EndVertical();

            if (remove)
            {
                _overridesProp.DeleteArrayElementAtIndex(i);
                break;
            }
        }
    }

    private static void DrawValue(SerializedProperty element, Shader shader, int shaderIndex)
    {
        var type = (ShaderPropertyOverrides.OverrideType)element.FindPropertyRelative("type").enumValueIndex;
        GUIContent valueLabel = new GUIContent("Valor");

        switch (type)
        {
            case ShaderPropertyOverrides.OverrideType.Range when shaderIndex >= 0:
                Vector2 limits = shader.GetPropertyRangeLimits(shaderIndex);
                EditorGUILayout.Slider(element.FindPropertyRelative("floatValue"), limits.x, limits.y, valueLabel);
                break;
            case ShaderPropertyOverrides.OverrideType.Float:
            case ShaderPropertyOverrides.OverrideType.Range:
                EditorGUILayout.PropertyField(element.FindPropertyRelative("floatValue"), valueLabel);
                break;
            case ShaderPropertyOverrides.OverrideType.Color:
                SerializedProperty colorProp = element.FindPropertyRelative("colorValue");
                bool hdr = shaderIndex >= 0 &&
                           (shader.GetPropertyFlags(shaderIndex) & ShaderPropertyFlags.HDR) != 0;
                colorProp.colorValue = EditorGUILayout.ColorField(valueLabel, colorProp.colorValue, true, true, hdr);
                break;
            case ShaderPropertyOverrides.OverrideType.Vector:
                SerializedProperty vectorProp = element.FindPropertyRelative("vectorValue");
                vectorProp.vector4Value = EditorGUILayout.Vector4Field(valueLabel, vectorProp.vector4Value);
                break;
            case ShaderPropertyOverrides.OverrideType.Texture:
                EditorGUILayout.ObjectField(element.FindPropertyRelative("textureValue"), typeof(Texture), valueLabel);
                break;
            case ShaderPropertyOverrides.OverrideType.Int:
                EditorGUILayout.PropertyField(element.FindPropertyRelative("intValue"), valueLabel);
                break;
        }
    }

    private void ShowAddMenu(Material material)
    {
        Shader shader = material.shader;
        var menu = new GenericMenu();
        for (int i = 0; i < shader.GetPropertyCount(); i++)
        {
            if (!IsEditable(shader, i)) continue;
            string name = shader.GetPropertyName(i);
            // "/" en el texto del GenericMenu crea submenus: se reemplaza por una barra similar.
            var content = new GUIContent($"{shader.GetPropertyDescription(i)}  ({name})".Replace('/', '∕'));
            if (FindOverride(name) >= 0)
            {
                menu.AddDisabledItem(content, true);
                continue;
            }
            int propertyIndex = i;
            menu.AddItem(content, false, () =>
            {
                // El callback corre fuera de OnInspectorGUI: hay que sincronizar y aplicar a mano.
                serializedObject.Update();
                AddOverride(material, propertyIndex);
                serializedObject.ApplyModifiedProperties();
                Target.Apply();
                SceneView.RepaintAll();
            });
        }
        if (menu.GetItemCount() == 0) menu.AddDisabledItem(new GUIContent("El shader no tiene propiedades editables"));
        menu.ShowAsContext();
    }

    private static bool IsEditable(Shader shader, int index)
    {
        const ShaderPropertyFlags hidden = ShaderPropertyFlags.HideInInspector |
                                           ShaderPropertyFlags.PerRendererData |
                                           ShaderPropertyFlags.NonModifiableTextureData;
        return (shader.GetPropertyFlags(index) & hidden) == 0;
    }

    private int FindOverride(string name)
    {
        for (int i = 0; i < _overridesProp.arraySize; i++)
        {
            if (_overridesProp.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == name)
                return i;
        }
        return -1;
    }

    /// <summary>Agrega el override tomando el valor actual del material como punto de partida.</summary>
    private void AddOverride(Material material, int shaderIndex)
    {
        Shader shader = material.shader;
        int index = _overridesProp.arraySize;
        _overridesProp.InsertArrayElementAtIndex(index);
        SerializedProperty element = _overridesProp.GetArrayElementAtIndex(index);

        element.FindPropertyRelative("name").stringValue = shader.GetPropertyName(shaderIndex);
        element.FindPropertyRelative("type").enumValueIndex = (int)ToOverrideType(shader.GetPropertyType(shaderIndex));
        element.FindPropertyRelative("enabled").boolValue = true;
        CopyFromMaterial(element, material);
    }

    private static void CopyFromMaterial(SerializedProperty element, Material material)
    {
        string name = element.FindPropertyRelative("name").stringValue;
        if (!material.HasProperty(name)) return;

        var type = (ShaderPropertyOverrides.OverrideType)element.FindPropertyRelative("type").enumValueIndex;
        switch (type)
        {
            case ShaderPropertyOverrides.OverrideType.Float:
            case ShaderPropertyOverrides.OverrideType.Range:
                element.FindPropertyRelative("floatValue").floatValue = material.GetFloat(name);
                break;
            case ShaderPropertyOverrides.OverrideType.Color:
                element.FindPropertyRelative("colorValue").colorValue = material.GetColor(name);
                break;
            case ShaderPropertyOverrides.OverrideType.Vector:
                element.FindPropertyRelative("vectorValue").vector4Value = material.GetVector(name);
                break;
            case ShaderPropertyOverrides.OverrideType.Texture:
                element.FindPropertyRelative("textureValue").objectReferenceValue = material.GetTexture(name);
                break;
            case ShaderPropertyOverrides.OverrideType.Int:
                element.FindPropertyRelative("intValue").intValue = material.GetInteger(name);
                break;
        }
    }

    private static ShaderPropertyOverrides.OverrideType ToOverrideType(ShaderPropertyType type)
    {
        switch (type)
        {
            case ShaderPropertyType.Color: return ShaderPropertyOverrides.OverrideType.Color;
            case ShaderPropertyType.Vector: return ShaderPropertyOverrides.OverrideType.Vector;
            case ShaderPropertyType.Range: return ShaderPropertyOverrides.OverrideType.Range;
            case ShaderPropertyType.Texture: return ShaderPropertyOverrides.OverrideType.Texture;
            case ShaderPropertyType.Int: return ShaderPropertyOverrides.OverrideType.Int;
            default: return ShaderPropertyOverrides.OverrideType.Float;
        }
    }
}
