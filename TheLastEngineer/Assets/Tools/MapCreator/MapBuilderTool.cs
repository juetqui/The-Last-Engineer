using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITYEDITOR
public class MapBuilderTool : EditorWindow
{
    // Referencias principales
    private Grid targetGrid;
    private MapBuilderPalette paletteAsset;
    private GameObject prefabToPaint; // Mantener compatibilidad con prefab único
    
    // Estado de la herramienta
    private bool isPainting = false;
    private bool isDeletingMode = false;
    private bool enableCollisionValidation = true;
    private int currentPrefabIndex = 0;
    private float currentYRotation = 0f;
    
    // Ghost preview
    private GameObject ghostObject;
    private Vector3Int lastGhostCell = Vector3Int.zero;
    private bool lastCellWasValid = true;

    // Configuración modular
    private Vector3 moduleSize = new Vector3(1f, 1f, 1f);
    private Vector3 pivotOffsetMultiplier = new Vector3(0.5f, 0.5f, 0f);
    private bool useAutoCentering = true;
    
    // Preview
    private Editor prefabPreviewEditor;
    private GameObject lastPreviewedPrefab;
    
    // EditorPrefs Keys
    private const string PREF_MODULE_SIZE_X = "MapBuilderTool_ModuleSizeX";
    private const string PREF_MODULE_SIZE_Y = "MapBuilderTool_ModuleSizeY";
    private const string PREF_MODULE_SIZE_Z = "MapBuilderTool_ModuleSizeZ";
    private const string PREF_PIVOT_OFFSET_X = "MapBuilderTool_PivotOffsetX";
    private const string PREF_PIVOT_OFFSET_Y = "MapBuilderTool_PivotOffsetY";
    private const string PREF_PIVOT_OFFSET_Z = "MapBuilderTool_PivotOffsetZ";
    private const string PREF_ROTATION = "MapBuilderTool_Rotation";
    private const string PREF_COLLISION_VALIDATION = "MapBuilderTool_CollisionValidation";
    private const string PREF_DELETING_MODE = "MapBuilderTool_DeletingMode";
    private const string PREF_AUTO_CENTERING = "MapBuilderTool_AutoCentering";
    
    // UI
    private Vector2 scrollPosition;

    [MenuItem("Tools/Map Builder")]
    public static void ShowWindow()
    {
        GetWindow<MapBuilderTool>("Map Builder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Configuración de la Grilla y Módulos", EditorStyles.boldLabel);
        
        targetGrid = (Grid)EditorGUILayout.ObjectField("Grilla Destino", targetGrid, typeof(Grid), true);

        EditorGUILayout.Space();
        
        // === PALETA Y PREFABS ===
        GUILayout.Label("Prefabs", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        paletteAsset = (MapBuilderPalette)EditorGUILayout.ObjectField("Paleta", paletteAsset, typeof(MapBuilderPalette), false);
        if (EditorGUI.EndChangeCheck())
        {
            RefreshGhost();
        }
        
        // Botón para crear nueva paleta
        if (GUILayout.Button("Crear Nueva Paleta"))
        {
            CreateNewPalette();
        }
        
        EditorGUILayout.Space();
        
        // Mostrar prefabs de la paleta o prefab individual
        if (paletteAsset != null && paletteAsset.prefabs != null && paletteAsset.prefabs.Count > 0)
        {
            GUILayout.Label($"Prefabs en Paleta: {paletteAsset.prefabs.Count}", EditorStyles.miniLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
            
            for (int i = 0; i < paletteAsset.prefabs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                // Highlight del prefab seleccionado
                GUI.backgroundColor = (i == currentPrefabIndex) ? Color.green : Color.white;
                
                if (GUILayout.Button($"{i + 1}", GUILayout.Width(30)))
                {
                    currentPrefabIndex = i;
                    RefreshGhost();
                }
                
                GUI.backgroundColor = Color.white;
                
                EditorGUI.BeginChangeCheck();
                paletteAsset.prefabs[i] = (GameObject)EditorGUILayout.ObjectField(paletteAsset.prefabs[i], typeof(GameObject), false);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(paletteAsset);
                    if (i == currentPrefabIndex)
                    {
                        RefreshGhost();
                    }
                }
                
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    paletteAsset.prefabs.RemoveAt(i);
                    EditorUtility.SetDirty(paletteAsset);
                    if (currentPrefabIndex >= paletteAsset.prefabs.Count)
                    {
                        currentPrefabIndex = Mathf.Max(0, paletteAsset.prefabs.Count - 1);
                    }
                    RefreshGhost();
                    break;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            
            if (GUILayout.Button("Agregar Prefab a Paleta"))
            {
                paletteAsset.prefabs.Add(null);
                EditorUtility.SetDirty(paletteAsset);
            }
            
            EditorGUILayout.Space();
            
            // === PREVIEW DEL PREFAB SELECCIONADO ===
            GUILayout.Label("Preview del Prefab", EditorStyles.boldLabel);
            GameObject currentPrefab = GetCurrentPrefab();
            
            if (currentPrefab != null)
            {
                UpdatePrefabPreview(currentPrefab);
                
                if (prefabPreviewEditor != null)
                {
                    GUILayout.Box("", GUILayout.Height(200), GUILayout.Width(200));
                    Rect previewRect = GUILayoutUtility.GetLastRect();
                    prefabPreviewEditor.OnInteractivePreviewGUI(previewRect, EditorStyles.helpBox);
                }
                else
                {
                    GUILayout.Box("No hay preview disponible", GUILayout.Height(200), GUILayout.Width(200));
                }
            }
            else
            {
                GUILayout.Box("Selecciona un prefab", GUILayout.Height(200), GUILayout.Width(200));
            }
        }
        else
        {
            // Fallback a prefab único si no hay paleta
            EditorGUI.BeginChangeCheck();
            prefabToPaint = (GameObject)EditorGUILayout.ObjectField("Prefab Individual", prefabToPaint, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck())
            {
                RefreshGhost();
            }
            
            EditorGUILayout.Space();
            
            // === PREVIEW DEL PREFAB INDIVIDUAL ===
            GUILayout.Label("Preview del Prefab", EditorStyles.boldLabel);
            
            if (prefabToPaint != null)
            {
                UpdatePrefabPreview(prefabToPaint);
                
                if (prefabPreviewEditor != null)
                {
                    GUILayout.Box("", GUILayout.Height(200), GUILayout.Width(200));
                    Rect previewRect = GUILayoutUtility.GetLastRect();
                    prefabPreviewEditor.OnInteractivePreviewGUI(previewRect, EditorStyles.helpBox);
                }
                else
                {
                    GUILayout.Box("No hay preview disponible", GUILayout.Height(200), GUILayout.Width(200));
                }
            }
            else
            {
                GUILayout.Box("Selecciona un prefab", GUILayout.Height(200), GUILayout.Width(200));
            }
        }
        
        EditorGUILayout.Space();
        
        // === ROTACIÓN ===
        GUILayout.Label("Rotación (Y Axis)", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Label($"Rotación actual: {currentYRotation}°", EditorStyles.miniLabel);
        
        EditorGUILayout.Space();
        
        // === CONFIGURACIÓN MODULAR ===
        GUILayout.Label("Configuración Modular", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        moduleSize = EditorGUILayout.Vector3Field("Tamaño del Módulo", moduleSize);
        if (EditorGUI.EndChangeCheck())
        {
            SavePreferences();
        }
        
        EditorGUI.BeginChangeCheck();
        useAutoCentering = EditorGUILayout.Toggle("Auto-Centrar Prefabs", useAutoCentering);
        if (EditorGUI.EndChangeCheck())
        {
            SavePreferences();
        }
        
        if (!useAutoCentering)
        {
            EditorGUI.BeginChangeCheck();
            pivotOffsetMultiplier = EditorGUILayout.Vector3Field("Offset de Pivote (Multiplicador)", pivotOffsetMultiplier);
            if (EditorGUI.EndChangeCheck())
            {
                SavePreferences();
            }
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Vector3Field("Offset de Pivote (Multiplicador)", pivotOffsetMultiplier);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.HelpBox("Auto-centrado activado: El prefab se centrará automáticamente en la celda según sus bounds.", MessageType.Info);
        }

        EditorGUILayout.Space();

        // Actualizar tamaño de celda del grid
        if (targetGrid != null)
        {
            targetGrid.cellSize = moduleSize;
        }
        
        // === OPCIONES ===
        GUILayout.Label("Opciones", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        enableCollisionValidation = EditorGUILayout.Toggle("Validar Superposición", enableCollisionValidation);
        if (EditorGUI.EndChangeCheck())
        {
            SavePreferences();
        }
        
        EditorGUI.BeginChangeCheck();
        isDeletingMode = EditorGUILayout.Toggle("Modo Borrado", isDeletingMode);
        if (EditorGUI.EndChangeCheck())
        {
            SavePreferences();
        }
        
        EditorGUILayout.Space();
        
        // === MODO PINTAR ===
        GUI.color = isPainting ? Color.green : Color.white;
        if (GUILayout.Button(isPainting ? "Modo Pintar: ACTIVADO" : "Modo Pintar: DESACTIVADO", GUILayout.Height(40)))
        {
            isPainting = !isPainting;
        }
        GUI.color = Color.white;
        
        EditorGUILayout.Space();
        
        // === ATAJOS DE TECLADO ===
        GUILayout.Label("Atajos de Teclado", EditorStyles.boldLabel);
        GUILayout.Label("Q: Rotar -90° | E: Rotar +90° | R: Reset Rotación", EditorStyles.miniLabel);
        GUILayout.Label("1-9: Seleccionar prefab de paleta", EditorStyles.miniLabel);
        GUILayout.Label("Shift + Click: Borrado rápido", EditorStyles.miniLabel);
        GUILayout.Label("Click: Colocar/Borrar prefab", EditorStyles.miniLabel);
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        LoadPreferences();
        RefreshGhost();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        DestroyGhost();
        DestroyPrefabPreview();
    }
    
    private void LoadPreferences()
    {
        moduleSize = new Vector3(
            EditorPrefs.GetFloat(PREF_MODULE_SIZE_X, 1f),
            EditorPrefs.GetFloat(PREF_MODULE_SIZE_Y, 1f),
            EditorPrefs.GetFloat(PREF_MODULE_SIZE_Z, 1f)
        );
        
        pivotOffsetMultiplier = new Vector3(
            EditorPrefs.GetFloat(PREF_PIVOT_OFFSET_X, 0.5f),
            EditorPrefs.GetFloat(PREF_PIVOT_OFFSET_Y, 0.5f),
            EditorPrefs.GetFloat(PREF_PIVOT_OFFSET_Z, 0f)
        );
        
        currentYRotation = EditorPrefs.GetFloat(PREF_ROTATION, 0f);
        enableCollisionValidation = EditorPrefs.GetBool(PREF_COLLISION_VALIDATION, true);
        isDeletingMode = EditorPrefs.GetBool(PREF_DELETING_MODE, false);
        useAutoCentering = EditorPrefs.GetBool(PREF_AUTO_CENTERING, true);
    }
    
    private void SavePreferences()
    {
        EditorPrefs.SetFloat(PREF_MODULE_SIZE_X, moduleSize.x);
        EditorPrefs.SetFloat(PREF_MODULE_SIZE_Y, moduleSize.y);
        EditorPrefs.SetFloat(PREF_MODULE_SIZE_Z, moduleSize.z);
        
        EditorPrefs.SetFloat(PREF_PIVOT_OFFSET_X, pivotOffsetMultiplier.x);
        EditorPrefs.SetFloat(PREF_PIVOT_OFFSET_Y, pivotOffsetMultiplier.y);
        EditorPrefs.SetFloat(PREF_PIVOT_OFFSET_Z, pivotOffsetMultiplier.z);
        
        EditorPrefs.SetFloat(PREF_ROTATION, currentYRotation);
        EditorPrefs.SetBool(PREF_COLLISION_VALIDATION, enableCollisionValidation);
        EditorPrefs.SetBool(PREF_DELETING_MODE, isDeletingMode);
        EditorPrefs.SetBool(PREF_AUTO_CENTERING, useAutoCentering);
    }
    
    private void CreateNewPalette()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Crear Nueva Paleta", 
            "NewMapBuilderPalette", 
            "asset", 
            "Elige la ubicación para la nueva paleta"
        );
        
        if (string.IsNullOrEmpty(path)) return;
        
        MapBuilderPalette newPalette = CreateInstance<MapBuilderPalette>();
        AssetDatabase.CreateAsset(newPalette, path);
        AssetDatabase.SaveAssets();
        paletteAsset = newPalette;
        RefreshGhost();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isPainting || targetGrid == null) return;

        GameObject currentPrefab = GetCurrentPrefab();
        if (currentPrefab == null) return;

        Event e = Event.current;
        
        // === KEYBOARD SHORTCUTS ===
        if (e.type == EventType.KeyDown)
        {
            bool handled = false;
            
            // Rotación
            if (e.keyCode == KeyCode.Q)
            {
                currentYRotation -= 90f;
                if (currentYRotation < 0) currentYRotation += 360f;
                SavePreferences();
                RefreshGhost();
                handled = true;
            }
            else if (e.keyCode == KeyCode.E)
            {
                currentYRotation += 90f;
                if (currentYRotation >= 360f) currentYRotation -= 360f;
                SavePreferences();
                RefreshGhost();
                handled = true;
            }
            else if (e.keyCode == KeyCode.R)
            {
                currentYRotation = 0f;
                SavePreferences();
                RefreshGhost();
                handled = true;
            }
            // Selección de prefabs (1-9)
            else if (paletteAsset != null && paletteAsset.prefabs.Count > 0)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (e.keyCode == KeyCode.Alpha1 + i)
                    {
                        if (i < paletteAsset.prefabs.Count)
                        {
                            currentPrefabIndex = i;
                            RefreshGhost();
                            handled = true;
                        }
                        break;
                    }
                }
            }
            
            if (handled)
            {
                e.Use();
                Repaint();
                SceneView.RepaintAll();
            }
        }

        // === RAYCAST Y POSICIONAMIENTO ===
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, targetGrid.transform.position);
        
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3Int cellPosition = targetGrid.WorldToCell(hitPoint);
            
            // Usar método auxiliar para obtener posición con offset
            Vector3 finalPosition = GetCellWorldPositionWithOffset(cellPosition);
            Quaternion finalRotation = Quaternion.Euler(0, currentYRotation, 0);
            
            // Verificar si la celda está ocupada
            bool cellOccupied = IsCellOccupied(cellPosition);
            
            // === GHOST PREVIEW ===
            if (ghostObject != null)
            {
                ghostObject.transform.SetPositionAndRotation(finalPosition, finalRotation);
                
                // Cambiar color del ghost según validez
                bool isValid = !isDeletingMode && (!enableCollisionValidation || !cellOccupied);
                
                if (cellPosition != lastGhostCell || isValid != lastCellWasValid)
                {
                    UpdateGhostColor(isValid);
                    lastGhostCell = cellPosition;
                    lastCellWasValid = isValid;
                }
                
                ghostObject.SetActive(!isDeletingMode);
                SceneView.RepaintAll();
            }
            
            // === VISUAL FEEDBACK ===
            // Dibujar wireframe de la celda actual (en la posición con offset)
            Color cellColor = isDeletingMode ? Color.red : (cellOccupied ? Color.red : Color.green);
            cellColor.a = 0.5f;
            Handles.color = cellColor;
            // Handles.DrawWireCube(finalPosition, moduleSize);
            
            Vector3 cellCenter = targetGrid.GetCellCenterWorld(cellPosition);
            Handles.DrawWireCube(cellCenter, moduleSize);

            // === CONTROLES DE MOUSE ===
            if (e.type == EventType.Layout)
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            
            bool isDeleting = isDeletingMode || e.shift;
            
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                if (isDeleting)
                {
                    DeleteAtCell(cellPosition);
                }
                else
                {
                    if (!enableCollisionValidation || !cellOccupied)
                        PaintCube(finalPosition, finalRotation, cellPosition);
                    else
                        Debug.LogWarning("No se puede colocar el prefab: celda ocupada");
                }
                
                e.Use();
            }
            
            // === HUD DE INFORMACIÓN ===
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Label($"Celda: {cellPosition}", EditorStyles.helpBox);
            GUILayout.Label($"Rotación: {currentYRotation}°", EditorStyles.helpBox);
            GUILayout.Label($"Prefab: {currentPrefab.name}", EditorStyles.helpBox);
            if (paletteAsset != null)
            {
                GUILayout.Label($"Índice: {currentPrefabIndex + 1}/{paletteAsset.prefabs.Count}", EditorStyles.helpBox);
            }
            GUILayout.Label($"Modo: {(isDeleting ? "BORRADO" : "PINTAR")}", EditorStyles.helpBox);
            GUILayout.EndArea();
            Handles.EndGUI();
        }
    }
    
    private GameObject GetCurrentPrefab()
    {
        if (paletteAsset != null && paletteAsset.prefabs != null && paletteAsset.prefabs.Count > 0)
        {
            currentPrefabIndex = Mathf.Clamp(currentPrefabIndex, 0, paletteAsset.prefabs.Count - 1);
            return paletteAsset.prefabs[currentPrefabIndex];
        }
        return prefabToPaint;
    }
    
    private void RefreshGhost()
    {
        DestroyGhost();
        
        GameObject currentPrefab = GetCurrentPrefab();
        if (currentPrefab == null) return;
        
        ghostObject = (GameObject)PrefabUtility.InstantiatePrefab(currentPrefab);
        ghostObject.name = "__MapBuilderGhost";
        ghostObject.hideFlags = HideFlags.HideAndDontSave;
        
        // Aplicar material transparente a todos los renderers
        foreach (var renderer in ghostObject.GetComponentsInChildren<Renderer>())
        {
            Material[] materials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                materials[i].SetFloat("_Surface", 1); // Transparent
                materials[i].SetColor("_BaseColor", new Color(0f, 1f, 0f, 0.4f));
                materials[i].EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                
                // Configurar para transparencia
                materials[i].SetFloat("_AlphaClip", 0);
                materials[i].SetFloat("_Blend", 0);
                materials[i].renderQueue = 3000;
            }
            renderer.sharedMaterials = materials;
        }
        
        // Desactivar colliders del ghost
        foreach (var collider in ghostObject.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
        
        SceneView.RepaintAll();
    }
    
    private void UpdateGhostColor(bool isValid)
    {
        if (ghostObject == null) return;
        
        Color color = isValid ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);
        
        foreach (var renderer in ghostObject.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.sharedMaterials)
            {
                if (mat != null)
                {
                    mat.SetColor("_BaseColor", color);
                }
            }
        }
    }
    
    private void DestroyGhost()
    {
        if (ghostObject != null)
        {
            DestroyImmediate(ghostObject);
            ghostObject = null;
        }
    }
    
    /// <summary>
    /// Calcula la posición mundial de una celda incluyendo el offset del pivote.
    /// </summary>
    private Vector3 GetCellWorldPositionWithOffset(Vector3Int cellPosition)
    {
        if (targetGrid == null) return Vector3.zero;
        
        Vector3 cellCenter = targetGrid.GetCellCenterWorld(cellPosition);
        
        if (useAutoCentering)
        {
            // Usar auto-centrado basado en los bounds del prefab
            GameObject currentPrefab = GetCurrentPrefab();
            if (currentPrefab != null)
            {
                Vector3 autoCenterOffset = GetPrefabCenterOffset(currentPrefab);
                return cellCenter - autoCenterOffset;
            }
        }
        
        // Usar offset manual
        Vector3 actualOffset = new Vector3(
            moduleSize.x * pivotOffsetMultiplier.x,
            moduleSize.y * pivotOffsetMultiplier.y,
            moduleSize.z * pivotOffsetMultiplier.z
        );
        
        return cellCenter + actualOffset;
    }
    
    /// <summary>
    /// Calcula el offset necesario para centrar un prefab en base a sus bounds.
    /// Retorna el offset entre el pivot del prefab y su centro geométrico.
    /// </summary>
    private Vector3 GetPrefabCenterOffset(GameObject prefab)
    {
        if (prefab == null) return Vector3.zero;
        
        // Obtener todos los renderers del prefab (incluyendo hijos)
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
        
        if (renderers.Length == 0)
        {
            // Si no hay renderers, intentar con colliders
            Collider[] colliders = prefab.GetComponentsInChildren<Collider>();
            if (colliders.Length > 0)
            {
                Bounds combinedBounds = colliders[0].bounds;
                for (int i = 1; i < colliders.Length; i++)
                {
                    combinedBounds.Encapsulate(colliders[i].bounds);
                }
                
                // El centro en espacio local considerando la posición del prefab
                return combinedBounds.center - prefab.transform.position;
            }
            
            // No hay geometría visible ni colliders, no hacer offset
            return Vector3.zero;
        }
        
        // Calcular los bounds combinados de todos los renderers
        Bounds combinedBounds2 = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combinedBounds2.Encapsulate(renderers[i].bounds);
        }
        
        // El offset es la diferencia entre el centro de los bounds y el pivot del prefab
        // Nota: bounds.center está en espacio mundial, así que restamos la posición del prefab
        return combinedBounds2.center - prefab.transform.position;
    }
    
    /// <summary>
    /// Actualiza el editor de preview del prefab seleccionado.
    /// </summary>
    private void UpdatePrefabPreview(GameObject prefab)
    {
        if (prefab == lastPreviewedPrefab && prefabPreviewEditor != null)
            return;
        
        DestroyPrefabPreview();
        
        if (prefab != null)
        {
            prefabPreviewEditor = Editor.CreateEditor(prefab);
            lastPreviewedPrefab = prefab;
        }
    }
    
    /// <summary>
    /// Destruye el editor de preview para liberar recursos.
    /// </summary>
    private void DestroyPrefabPreview()
    {
        if (prefabPreviewEditor != null)
        {
            DestroyImmediate(prefabPreviewEditor);
            prefabPreviewEditor = null;
            lastPreviewedPrefab = null;
        }
    }
    
    private bool IsCellOccupied(Vector3Int cellPosition)
    {
        if (targetGrid == null) return false;
    
        Vector3 cellPositionWithOffset = GetCellWorldPositionWithOffset(cellPosition);
        float tolerance = Mathf.Max(moduleSize.magnitude * 0.2f, 0.5f); // Tolerancia más generosa
    
        foreach (Transform child in targetGrid.transform)
        {
            // Verificar solo por distancia con offset (más confiable que WorldToCell con offset)
            if (Vector3.Distance(child.position, cellPositionWithOffset) < tolerance)
            {
                return true;
            }
        }
    
        return false;
    }
    
    private GameObject GetObjectAtCell(Vector3Int cellPosition)
    {
        if (targetGrid == null) return null;
    
        Vector3 cellPositionWithOffset = GetCellWorldPositionWithOffset(cellPosition);
        float tolerance = Mathf.Max(moduleSize.magnitude * 0.2f, 0.5f); // Tolerancia más generosa
    
        foreach (Transform child in targetGrid.transform)
        {
            // Verificar solo por distancia con offset (más confiable que WorldToCell con offset)
            if (Vector3.Distance(child.position, cellPositionWithOffset) < tolerance)
            {
                return child.gameObject;
            }
        }
    
        return null;
    }
    
    private void DeleteAtCell(Vector3Int cellPosition)
    {
        GameObject objToDelete = GetObjectAtCell(cellPosition);
        
        if (objToDelete != null)
        {
            Undo.DestroyObjectImmediate(objToDelete);
            Debug.Log($"Prefab eliminado en celda {cellPosition}");
        }
    }

    private void PaintCube(Vector3 finalPosition, Quaternion finalRotation, Vector3Int cellPosition)
    {
        GameObject currentPrefab = GetCurrentPrefab();
        if (currentPrefab == null) return;
        
        // Instanciar
        GameObject newCube = (GameObject)PrefabUtility.InstantiatePrefab(currentPrefab);
        
        // Configurar transform
        newCube.transform.parent = targetGrid.transform;
        newCube.transform.SetPositionAndRotation(finalPosition, finalRotation);
        
        // Registrar para Undo
        Undo.RegisterCreatedObjectUndo(newCube, "Pintar Módulo");
        
        Debug.Log($"Prefab '{currentPrefab.name}' colocado en celda {cellPosition} con rotación {currentYRotation}°");
    }
}
#endif
