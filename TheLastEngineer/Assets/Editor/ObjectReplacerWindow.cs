using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Herramienta de editor para reemplazar en bloque los objetos seleccionados
/// por instancias de un prefab, conservando posición, rotación, escala y padre.
/// </summary>
public class ObjectReplacerWindow : EditorWindow
{
    // Variable serializada: prefab destino con el que se reemplazan los objetos.
    [SerializeField] private GameObject _prefab;
    // Lista sincronizada en tiempo real con la selección del editor.
    [SerializeField] private List<GameObject> _selected = new List<GameObject>();

    private Vector2 _scroll;

    [MenuItem("Tools/Object Replacer")]
    public static void ShowWindow()
    {
        GetWindow<ObjectReplacerWindow>("Object Replacer");
    }

    private void OnEnable()
    {
        Selection.selectionChanged += RefreshSelection;
        RefreshSelection();
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= RefreshSelection;
    }

    // Unity la llama automáticamente al cambiar la selección mientras la ventana está abierta.
    private void OnSelectionChange()
    {
        RefreshSelection();
    }

    private void OnFocus()
    {
        RefreshSelection();
    }

    /// <summary>
    /// Actualiza la lista con los GameObjects de escena actualmente seleccionados.
    /// </summary>
    private void RefreshSelection()
    {
        _selected.Clear();

        foreach (GameObject go in Selection.gameObjects)
        {
            // Sólo objetos de escena (los assets del Project no aplican para reemplazo).
            if (go != null && go.scene.IsValid())
                _selected.Add(go);
        }

        Repaint();
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab de Reemplazo", EditorStyles.boldLabel);
        _prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), false);

        EditorGUILayout.Space();

        // === LISTA DE OBJETOS SELECCIONADOS (tiempo real) ===
        GUILayout.Label($"Objetos Seleccionados: {_selected.Count}", EditorStyles.boldLabel);

        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(200));
        if (_selected.Count == 0)
        {
            EditorGUILayout.HelpBox("Seleccioná objetos de la escena para reemplazarlos.", MessageType.Info);
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true); // sólo lectura: la lista refleja la selección
            foreach (GameObject go in _selected)
            {
                EditorGUILayout.ObjectField(go, typeof(GameObject), true);
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // === BOTÓN DE REEMPLAZO ===
        EditorGUI.BeginDisabledGroup(_prefab == null || _selected.Count == 0);
        if (GUILayout.Button("Reemplazar seleccionados", GUILayout.Height(40)))
        {
            ReplaceAll();
        }
        EditorGUI.EndDisabledGroup();

        if (_prefab == null)
            EditorGUILayout.HelpBox("Cargá un prefab para habilitar el reemplazo.", MessageType.Warning);
    }

    /// <summary>
    /// Reemplaza cada objeto de la lista por una instancia del prefab, copiando su
    /// transform (posición/rotación world, escala local) y su padre. Todo con soporte de Undo.
    /// </summary>
    private void ReplaceAll()
    {
        if (_prefab == null) return;

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Object Replacer");
        int undoGroup = Undo.GetCurrentGroup();

        // Copia de la lista: la vamos vaciando y no queremos modificarla mientras iteramos.
        List<GameObject> originals = new List<GameObject>(_selected);

        foreach (GameObject original in originals)
        {
            if (original == null) continue;

            Transform originalTransform = original.transform;

            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(_prefab);
            if (inst == null) continue;

            Transform instTransform = inst.transform;

            // Hereda el padre original si lo tiene (si es null queda en la raíz de la escena).
            instTransform.SetParent(originalTransform.parent, false);
            instTransform.SetPositionAndRotation(originalTransform.position, originalTransform.rotation);
            instTransform.localScale = originalTransform.localScale;
            instTransform.SetSiblingIndex(originalTransform.GetSiblingIndex());

            Undo.RegisterCreatedObjectUndo(inst, "Reemplazar objeto");
            Undo.DestroyObjectImmediate(original);
        }

        Undo.CollapseUndoOperations(undoGroup);

        _selected.Clear();
        Repaint();
    }
}
