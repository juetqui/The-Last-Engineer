#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class PrefabPainterSceneTool
{
    public enum SnapMode { Grid, Chain }

    public struct Settings
    {
        public SnapMode snapMode;
        public float gridSize;
        public bool alignToSurfaceNormal;
        public bool randomYaw;
        public float yOffset;
        public bool livePreview;
    }

    private GameObject _currentPrefab;
    private GameObject _ghost;
    private Transform _lastPlaced;
    private Settings _settings;

    public void SetSettings(Settings s) => _settings = s;
    public void SetCurrentPrefab(GameObject prefab)
    {
        _currentPrefab = prefab;
        RefreshGhost();
    }

    public void ClearLastPlaced() => _lastPlaced = null;

    public void Dispose()
    {
        if (_ghost) Object.DestroyImmediate(_ghost);
    }

    private void RefreshGhost()
    {
        if (_ghost) Object.DestroyImmediate(_ghost);
        if (_currentPrefab == null) return;
        _ghost = (GameObject)PrefabUtility.InstantiatePrefab(_currentPrefab);
        foreach (var r in _ghost.GetComponentsInChildren<Renderer>())
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.4f));
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            r.sharedMaterial = mat;
        }
        SetHierarchyStatic(_ghost, true);
        _ghost.name = "__GhostPreview";
        _ghost.hideFlags = HideFlags.HideAndDontSave;
    }

    private static void SetHierarchyStatic(GameObject go, bool isStatic)
    {
        foreach (Transform t in go.GetComponentsInChildren<Transform>())
            GameObjectUtility.SetStaticEditorFlags(t.gameObject, isStatic ? StaticEditorFlags.BatchingStatic : 0);
    }

    public void DuringSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        if (_currentPrefab == null) return;


        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out var hit, 5000f))
        {
            Vector3 placePos = hit.point + Vector3.up * _settings.yOffset;
            Quaternion rot = Quaternion.identity;


            if (_settings.alignToSurfaceNormal)
                rot = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // Snap según modo
            if (_settings.snapMode == SnapMode.Grid)
            {
                float g = Mathf.Max(0.01f, _settings.gridSize);
                placePos = new Vector3(
                Mathf.Round(placePos.x / g) * g,
                Mathf.Round(placePos.y / g) * g,
                Mathf.Round(placePos.z / g) * g
                );
            }
            else if (_settings.snapMode == SnapMode.Chain && _lastPlaced)
            {
                Bounds lastB = GetHierarchyBounds(_lastPlaced.gameObject);
                Bounds curB = GetPrefabBounds(_currentPrefab);


                // Dirección: hacia la cámara en plano XZ para armar "cadena"
                Vector3 camFwd = sceneView.camera.transform.forward;
                camFwd.y = 0f; camFwd.Normalize();
                Vector3 dir = camFwd.sqrMagnitude < 0.001f ? Vector3.forward : camFwd;


                float step = GetSnapStep(lastB, curB, dir);
                placePos = _lastPlaced.position + dir * step;
            }

            if (_settings.randomYaw)
            {
                // Yaw aleatorio pero estable por celda (grid hash) para repetibilidad
                int hash = Mathf.RoundToInt(placePos.x * 13 + placePos.z * 17);
                float yaw = (hash % 360);
                rot = Quaternion.Euler(0f, yaw, 0f) * rot;
            }


            if (_settings.livePreview && _ghost)
            {
                _ghost.transform.SetPositionAndRotation(placePos, rot);
                SceneView.RepaintAll();
            }
            // Interacciones
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));


            if (e.type == EventType.ScrollWheel && e.modifiers == EventModifiers.Shift)
            {
                // Rotación con rueda + Shift
                float delta = e.delta.y > 0 ? 15f : -15f;
                _ghost.transform.rotation = Quaternion.Euler(0, delta, 0) * _ghost.transform.rotation;
                e.Use();
            }


            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                Place(placePos, rot);
                e.Use();
            }
            else if (e.type == EventType.MouseDown && e.button == 0 && e.control)
            {
                // Borrar objeto bajo el cursor
                if (hit.transform)
                {
                    Undo.DestroyObjectImmediate(hit.transform.gameObject);
                    if (_lastPlaced == hit.transform) _lastPlaced = null;
                }
                e.Use();
            }
        }
    }

    private void Place(Vector3 pos, Quaternion rot)
    {
        if (_currentPrefab == null) return;
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(_currentPrefab);
        Undo.RegisterCreatedObjectUndo(go, "Paint Prefab");
        go.transform.SetPositionAndRotation(pos, rot);
        Selection.activeObject = go;
        _lastPlaced = go.transform;
    }

    private static Bounds GetHierarchyBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);
        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        return b;
    }

    private static Bounds GetPrefabBounds(GameObject prefab)
    {
        var tmp = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Bounds b = GetHierarchyBounds(tmp);
        Object.DestroyImmediate(tmp);
        return b;
    }

    private static float GetSnapStep(Bounds last, Bounds cur, Vector3 dir)
    {
        // Proyectar extents en la dirección y sumar para dejarlos "besándose" sin solaparse
        Vector3 d = dir.normalized;
        float lastExtent = Mathf.Abs(Vector3.Dot(last.extents, new Vector3(Mathf.Abs(d.x), Mathf.Abs(d.y), Mathf.Abs(d.z))));
        float curExtent = Mathf.Abs(Vector3.Dot(cur.extents, new Vector3(Mathf.Abs(d.x), Mathf.Abs(d.y), Mathf.Abs(d.z))));
        return lastExtent + curExtent;
    }
}
#endif