using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Inspector de ShaderPropertyOverrides: un bloque por material a sobrescribir, cada uno con su
/// selector de material, la lista de propiedades del shader para agregar como override y cada
/// override dibujado con el campo que corresponde a su tipo.
/// Todo se edita via SerializedProperty para tener Undo y dirty de escena/prefab gratis.
/// </summary>
[CustomEditor(typeof(ShaderPropertyOverrides))]
public class ShaderPropertyOverridesEditor : UnityEditor.Editor
{
    private const int AllMaterials = ShaderPropertyOverrides.AllMaterials;

    private SerializedProperty _setsProp;

    private ShaderPropertyOverrides Target => (ShaderPropertyOverrides)target;

    private void OnEnable()
    {
        _setsProp = serializedObject.FindProperty("_materialSets");
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

        Material decalSource = isDecal ? Target.GetSourceMaterial(0) : null;
        EditorGUILayout.HelpBox(isDecal
                ? $"Decal: usa una copia en memoria de '{(decalSource != null ? decalSource.name : "?")}' " +
                  "(en disco siempre queda el original). No apliques al prefab el override del material del DecalProjector."
                : "Solo propiedades expuestas. Las keywords (Boolean/Enum Keyword de Shader Graph) no se pueden variar por objeto. " +
                  "Un material con su propio bloque pisa al de 'Todos' en las propiedades que repiten.",
            MessageType.Info);

        Material[] materials = isDecal ? new[] { decalSource } : Target.Renderer.sharedMaterials;
        if (GUILayout.Button("Agregar material")) ShowAddSetMenu(materials, isDecal);

        for (int i = 0; i < _setsProp.arraySize; i++)
        {
            EditorGUILayout.Space(2);
            if (DrawSet(i, materials, isDecal))
            {
                _setsProp.DeleteArrayElementAtIndex(i);
                break;
            }
        }

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

    #region Sets

    /// <summary>Dibuja un bloque de material; devuelve true si se pidio quitarlo.</summary>
    private bool DrawSet(int setIndex, Material[] materials, bool isDecal)
    {
        SerializedProperty set = _setsProp.GetArrayElementAtIndex(setIndex);
        SerializedProperty indexProp = set.FindPropertyRelative("materialIndex");
        SerializedProperty overridesProp = set.FindPropertyRelative("overrides");
        int materialIndex = indexProp.intValue;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        // isExpanded vive en el editor: plegar un bloque no ensucia la escena.
        // Ancho fijo: EditorGUILayout.Foldout se estira y empujaria el selector de material.
        Rect foldoutRect = GUILayoutUtility.GetRect(36, EditorGUIUtility.singleLineHeight, GUILayout.Width(36));
        set.isExpanded = EditorGUI.Foldout(foldoutRect, set.isExpanded, $"({overridesProp.arraySize})", true);
        if (isDecal) EditorGUILayout.LabelField("Material del decal", EditorStyles.boldLabel);
        else DrawMaterialPopup(indexProp, materials);
        bool remove = GUILayout.Button("✕", GUILayout.Width(22));
        EditorGUILayout.EndHorizontal();

        if (IsDuplicated(setIndex, materialIndex))
            EditorGUILayout.HelpBox("Ya hay otro bloque para este material: solo se aplica el primero.", MessageType.Warning);

        if (set.isExpanded) DrawSetContents(overridesProp, materialIndex, materials, isDecal);

        EditorGUILayout.EndVertical();
        return remove;
    }

    private void DrawSetContents(SerializedProperty overridesProp, int materialIndex, Material[] materials, bool isDecal)
    {
        if (!isDecal && materialIndex >= materials.Length)
        {
            EditorGUILayout.HelpBox($"El renderer no tiene material en el indice {materialIndex}.", MessageType.Warning);
            return;
        }

        Material material = Target.GetSourceMaterial(materialIndex);
        if (material == null || material.shader == null)
        {
            EditorGUILayout.HelpBox(isDecal ? "El DecalProjector no tiene material." : "El renderer no tiene material en ese indice.",
                MessageType.Warning);
            return;
        }

        if (!isDecal && materialIndex == AllMaterials && materials.Length > 1)
            EditorGUILayout.HelpBox($"Las propiedades se listan del primer material ({material.name}).", MessageType.None);

        DrawToolbar(overridesProp, material);
        EditorGUILayout.Space();
        DrawOverrides(overridesProp, material);
    }

    private static void DrawMaterialPopup(SerializedProperty indexProp, Material[] materials)
    {
        var options = new List<string> { "Todos los materiales" };
        for (int i = 0; i < materials.Length; i++) options.Add(MaterialLabel(materials, i));

        // Un indice que ya no existe (le sacaron materiales al renderer) se muestra igual para no perderlo.
        int current = indexProp.intValue;
        if (current >= materials.Length) options.Add($"[{current}] (no existe)");
        int selected = current < 0 ? 0 : current + 1;

        int chosen = EditorGUILayout.Popup(selected, options.ToArray());
        if (chosen != selected) indexProp.intValue = chosen - 1;
    }

    private void ShowAddSetMenu(Material[] materials, bool isDecal)
    {
        var menu = new GenericMenu();
        if (isDecal)
        {
            // El projector tiene un solo material: alcanza con un bloque.
            AddSetMenuItem(menu, "Material del decal", 0, _setsProp.arraySize > 0);
        }
        else
        {
            AddSetMenuItem(menu, "Todos los materiales", AllMaterials, FindSet(AllMaterials) >= 0);
            for (int i = 0; i < materials.Length; i++)
                AddSetMenuItem(menu, MaterialLabel(materials, i).Replace('/', '∕'), i, FindSet(i) >= 0);
        }
        menu.ShowAsContext();
    }

    private void AddSetMenuItem(GenericMenu menu, string label, int materialIndex, bool alreadyUsed)
    {
        var content = new GUIContent(label);
        if (alreadyUsed)
        {
            menu.AddDisabledItem(content, true);
            return;
        }
        menu.AddItem(content, false, () =>
        {
            // El callback corre fuera de OnInspectorGUI: hay que sincronizar y aplicar a mano.
            serializedObject.Update();
            AddSet(materialIndex);
            serializedObject.ApplyModifiedProperties();
        });
    }

    private void AddSet(int materialIndex)
    {
        int index = _setsProp.arraySize;
        _setsProp.InsertArrayElementAtIndex(index);
        // InsertArrayElementAtIndex duplica el ultimo elemento: hay que vaciar la copia.
        SerializedProperty set = _setsProp.GetArrayElementAtIndex(index);
        set.FindPropertyRelative("materialIndex").intValue = materialIndex;
        set.FindPropertyRelative("overrides").ClearArray();
        set.isExpanded = true;
    }

    private int FindSet(int materialIndex)
    {
        for (int i = 0; i < _setsProp.arraySize; i++)
        {
            if (_setsProp.GetArrayElementAtIndex(i).FindPropertyRelative("materialIndex").intValue == materialIndex)
                return i;
        }
        return -1;
    }

    private bool IsDuplicated(int setIndex, int materialIndex)
    {
        int first = FindSet(materialIndex);
        return first >= 0 && first < setIndex;
    }

    private static string MaterialLabel(Material[] materials, int index) =>
        $"[{index}] {(materials[index] != null ? materials[index].name : "(vacio)")}";

    #endregion

    #region Overrides

    private void DrawToolbar(SerializedProperty overridesProp, Material material)
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Agregar propiedad")) ShowAddMenu(overridesProp, material);
        if (GUILayout.Button("Agregar todas"))
        {
            Shader shader = material.shader;
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                string name = shader.GetPropertyName(i);
                if (IsEditable(shader, i) && FindOverride(overridesProp, name) < 0) AddOverride(overridesProp, material, i);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Resetear todas al material"))
        {
            for (int i = 0; i < overridesProp.arraySize; i++)
                CopyFromMaterial(overridesProp.GetArrayElementAtIndex(i), material);
        }
        if (GUILayout.Button("Limpiar")) overridesProp.ClearArray();
        EditorGUILayout.EndHorizontal();
    }

    private static void DrawOverrides(SerializedProperty overridesProp, Material material)
    {
        Shader shader = material.shader;
        for (int i = 0; i < overridesProp.arraySize; i++)
        {
            SerializedProperty element = overridesProp.GetArrayElementAtIndex(i);
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
                overridesProp.DeleteArrayElementAtIndex(i);
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

    private void ShowAddMenu(SerializedProperty overridesProp, Material material)
    {
        Shader shader = material.shader;
        // El SerializedProperty no sobrevive al Update del callback: se lo vuelve a buscar por path.
        string overridesPath = overridesProp.propertyPath;
        var menu = new GenericMenu();
        for (int i = 0; i < shader.GetPropertyCount(); i++)
        {
            if (!IsEditable(shader, i)) continue;
            string name = shader.GetPropertyName(i);
            // "/" en el texto del GenericMenu crea submenus: se reemplaza por una barra similar.
            var content = new GUIContent($"{shader.GetPropertyDescription(i)}  ({name})".Replace('/', '∕'));
            if (FindOverride(overridesProp, name) >= 0)
            {
                menu.AddDisabledItem(content, true);
                continue;
            }
            int propertyIndex = i;
            menu.AddItem(content, false, () =>
            {
                // El callback corre fuera de OnInspectorGUI: hay que sincronizar y aplicar a mano.
                serializedObject.Update();
                SerializedProperty prop = serializedObject.FindProperty(overridesPath);
                if (prop == null) return;
                AddOverride(prop, material, propertyIndex);
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

    private static int FindOverride(SerializedProperty overridesProp, string name)
    {
        for (int i = 0; i < overridesProp.arraySize; i++)
        {
            if (overridesProp.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == name)
                return i;
        }
        return -1;
    }

    /// <summary>Agrega el override tomando el valor actual del material como punto de partida.</summary>
    private static void AddOverride(SerializedProperty overridesProp, Material material, int shaderIndex)
    {
        Shader shader = material.shader;
        int index = overridesProp.arraySize;
        overridesProp.InsertArrayElementAtIndex(index);
        SerializedProperty element = overridesProp.GetArrayElementAtIndex(index);

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

    #endregion
}
