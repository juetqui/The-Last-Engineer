using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// The initial-state dropdown only offers what the owner accepts (GlitchSwapRules): Glitcheable
/// Clean/Glitched, Intangible Clean/Intangible, node all three. Goes through SerializedProperty so
/// prefab overrides, revert and multi-edit keep working.
/// </summary>
[CustomEditor(typeof(GlitchComponent))]
[CanEditMultipleObjects]
public class GlitchComponentEditor : Editor
{
    private const string StateField = "_currentLevel";

    private static readonly GlitchState[] AllStates = { GlitchState.Clean, GlitchState.Intangible, GlitchState.Glitched };

    private SerializedProperty _state;

    private void OnEnable() => _state = serializedObject.FindProperty(StateField);

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var kind = ((GlitchComponent)target).ResolveKind() ?? GlitchContainerKind.Node;
        var allowed = AllowedStates(kind);
        var current = (GlitchState)_state.intValue;

        var label = new GUIContent("Initial State", $"States a {kind} accepts.");
        var rect = EditorGUILayout.GetControlRect();

        EditorGUI.BeginProperty(rect, label, _state);
        EditorGUI.showMixedValue = _state.hasMultipleDifferentValues;
        EditorGUI.BeginChangeCheck();

        int index = allowed.IndexOf(current);
        int picked = EditorGUI.Popup(rect, label.text, Mathf.Max(index, 0), Names(allowed));

        if (EditorGUI.EndChangeCheck())
            _state.intValue = (int)allowed[picked];

        EditorGUI.showMixedValue = false;
        EditorGUI.EndProperty();

        if (index < 0 && !_state.hasMultipleDifferentValues)
            EditorGUILayout.HelpBox($"{current} is not valid on a {kind}: Play Mode will refuse to start. Pick a state above.", MessageType.Error);

        serializedObject.ApplyModifiedProperties();
    }

    private static List<GlitchState> AllowedStates(GlitchContainerKind kind)
    {
        var allowed = new List<GlitchState>(AllStates.Length);

        foreach (var state in AllStates)
            if (GlitchSwapRules.IsValidState(kind, state)) allowed.Add(state);

        return allowed;
    }

    private static string[] Names(List<GlitchState> states)
    {
        var names = new string[states.Count];
        for (int i = 0; i < states.Count; i++) names[i] = states[i].ToString();

        return names;
    }
}
