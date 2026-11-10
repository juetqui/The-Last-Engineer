using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Dropdown with only the states listed in [GlitchStateOptions]. Goes through SerializedProperty
/// (BeginProperty) so prefab overrides, revert and multi-edit keep working, like GlitchComponentEditor.
/// </summary>
[CustomPropertyDrawer(typeof(GlitchStateOptionsAttribute))]
public class GlitchStateOptionsDrawer : PropertyDrawer
{
    private const float HelpBoxHeight = 32f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (IsInvalid(property)) height += EditorGUIUtility.standardVerticalSpacing + HelpBoxHeight;

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Enum)
        {
            EditorGUI.LabelField(position, label.text, "[GlitchStateOptions] only works on a GlitchState field.");
            return;
        }

        var options = ((GlitchStateOptionsAttribute)attribute).Options;
        var current = (GlitchState)property.intValue;
        int index = Array.IndexOf(options, current);

        var lineRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        label = EditorGUI.BeginProperty(lineRect, label, property);
        EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
        EditorGUI.BeginChangeCheck();

        int picked = EditorGUI.Popup(lineRect, label.text, Mathf.Max(index, 0), Names(options));

        if (EditorGUI.EndChangeCheck())
            property.intValue = (int)options[picked];

        EditorGUI.showMixedValue = false;
        EditorGUI.EndProperty();

        if (IsInvalid(property))
        {
            var helpRect = new Rect(position.x, lineRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                                    position.width, HelpBoxHeight);
            EditorGUI.HelpBox(helpRect, $"{current} is not allowed here: it falls back to Clean at runtime. Pick a state above.", MessageType.Error);
        }
    }

    private bool IsInvalid(SerializedProperty property)
    {
        if (property.propertyType != SerializedPropertyType.Enum || property.hasMultipleDifferentValues) return false;

        var options = ((GlitchStateOptionsAttribute)attribute).Options;
        return Array.IndexOf(options, (GlitchState)property.intValue) < 0;
    }

    private static string[] Names(GlitchState[] states)
    {
        var names = new string[states.Length];
        for (int i = 0; i < states.Length; i++) names[i] = states[i].ToString();

        return names;
    }
}
