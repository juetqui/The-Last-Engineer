using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Permite ver la oscilacion de range de LightMovement en el Scene View sin entrar a Play.
/// La preview corre solo mientras esta prendida y siempre restaura el range original al detenerse.
/// </summary>
[CustomEditor(typeof(LightMovement))]
[CanEditMultipleObjects]
public class LightMovementEditor : UnityEditor.Editor
{
    // EditorSceneManager.ClearSceneDirtiness es internal, asi que se resuelve por reflexion.
    // Si Unity la sacara en una version futura, la preview sigue funcionando: solo deja la escena marcada.
    private static readonly MethodInfo ClearSceneDirtinessMethod = typeof(EditorSceneManager).GetMethod(
        "ClearSceneDirtiness",
        BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
        null,
        new[] { typeof(Scene) },
        null);

    private readonly Dictionary<LightMovement, float> _originalRanges = new Dictionary<LightMovement, float>();
    private readonly List<Scene> _scenesToKeepClean = new List<Scene>();
    private bool _isPreviewing;

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        AssemblyReloadEvents.beforeAssemblyReload += StopPreview;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        AssemblyReloadEvents.beforeAssemblyReload -= StopPreview;
        StopPreview();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
        {
            string label = _isPreviewing ? "Detener preview" : "Preview en Scene View";

            if (GUILayout.Button(label, GUILayout.Height(26f)))
            {
                if (_isPreviewing)
                    StopPreview();
                else
                    StartPreview();
            }

            if (_isPreviewing)
                DrawPreviewReadout();
            else
                DrawRangeShortcuts();
        }
    }

    private void DrawPreviewReadout()
    {
        var movement = target as LightMovement;
        var light = movement != null ? movement.GetComponent<Light>() : null;

        if (light != null)
        {
            float t = Mathf.InverseLerp(movement.MinRange, movement.MaxRange, light.range);
            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.ProgressBar(rect, t, "Range actual: " + light.range.ToString("F2"));
        }

        EditorGUILayout.HelpBox("El range original se restaura al detener la preview.", MessageType.None);
    }

    private void DrawRangeShortcuts()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Ver Min"))
                ApplyExtreme(useMin: true);

            if (GUILayout.Button("Ver Max"))
                ApplyExtreme(useMin: false);
        }
    }

    private void ApplyExtreme(bool useMin)
    {
        foreach (Object obj in targets)
        {
            var movement = obj as LightMovement;
            var light = movement != null ? movement.GetComponent<Light>() : null;
            if (light == null) continue;

            Undo.RecordObject(light, useMin ? "Ver Min Range" : "Ver Max Range");
            light.range = useMin ? movement.MinRange : movement.MaxRange;
            EditorUtility.SetDirty(light);
        }

        SceneView.RepaintAll();
    }

    private void StartPreview()
    {
        _originalRanges.Clear();
        _scenesToKeepClean.Clear();

        foreach (Object obj in targets)
        {
            var movement = obj as LightMovement;
            // Los prefabs abiertos desde el Project window no se previsualizan: escribirles el range
            // ensuciaria el asset. Se previsualizan desde la escena o desde el Prefab Mode.
            if (movement == null || EditorUtility.IsPersistent(movement)) continue;

            var light = movement.GetComponent<Light>();
            if (light == null) continue;

            _originalRanges[movement] = light.range;
            movement.InitializeCycle();
            RememberSceneCleanliness(movement.gameObject.scene);
        }

        if (_originalRanges.Count == 0) return;

        _isPreviewing = true;
        EditorApplication.update += OnEditorUpdate;
    }

    private void StopPreview()
    {
        if (_isPreviewing)
        {
            _isPreviewing = false;
            EditorApplication.update -= OnEditorUpdate;
        }

        foreach (KeyValuePair<LightMovement, float> entry in _originalRanges)
        {
            if (entry.Key == null) continue;

            var light = entry.Key.GetComponent<Light>();
            if (light != null)
                light.range = entry.Value;
        }

        // La preview no deberia dejar la escena marcada como modificada si estaba guardada.
        if (ClearSceneDirtinessMethod != null)
        {
            foreach (Scene scene in _scenesToKeepClean)
            {
                if (scene.IsValid())
                    ClearSceneDirtinessMethod.Invoke(null, new object[] { scene });
            }
        }

        _originalRanges.Clear();
        _scenesToKeepClean.Clear();
        SceneView.RepaintAll();
    }

    private void OnEditorUpdate()
    {
        float time = (float)EditorApplication.timeSinceStartup;
        bool anyAlive = false;

        foreach (KeyValuePair<LightMovement, float> entry in _originalRanges)
        {
            if (entry.Key == null) continue;

            entry.Key.ApplyRange(time);
            anyAlive = true;
        }

        if (!anyAlive)
        {
            StopPreview();
            return;
        }

        SceneView.RepaintAll();
        Repaint();
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            StopPreview();
    }

    private void RememberSceneCleanliness(Scene scene)
    {
        if (!scene.IsValid() || scene.isDirty) return;
        if (_scenesToKeepClean.Contains(scene)) return;

        _scenesToKeepClean.Add(scene);
    }
}
