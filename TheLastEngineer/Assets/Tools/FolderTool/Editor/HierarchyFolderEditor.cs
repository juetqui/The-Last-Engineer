using UnityEditor;
using UnityEngine;

namespace Tools.FolderTool.Editor
{
    [InitializeOnLoad]
    public class HierarchyFolderEditor : UnityEditor.Editor
    {
        [MenuItem("GameObject/Hierarchy Folder", false, 10)]
        public static void CreateSpawner()
        {
            var go = new GameObject("Hierarchy Folder");
            go.AddComponent<HierarchyFolder>();

            if (Selection.activeTransform != null)
                go.transform.SetParent(Selection.activeTransform, worldPositionStays:false);

            Undo.RegisterCreatedObjectUndo(go, "Create Hierarchy Folder");
            Selection.activeGameObject = go;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        [MenuItem("GameObject/Hierarchy Folder", true)]
        private static bool ValidateCreateSpawner()
        {
            return !Application.isPlaying;
        }
        
        static HierarchyFolderEditor()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            EditorApplication.delayCall += ApplyIcons;
        }

        private static void ApplyIcons()
        {
            var folders = FindObjectsByType<HierarchyFolder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var tex = EditorGUIUtility.IconContent("d_Folder Icon").image as Texture2D; // try "d_Folder Icon" or "Folder Icon"
            foreach (var f in folders)
            {
                if (f) EditorGUIUtility.SetIconForObject(f.gameObject, tex);
            }
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null) return;

            // Only apply to objects with HierarchyFolder component
            if (!obj.TryGetComponent<HierarchyFolder>(out var folder)) return;

            var folderC = folder.folderColor;
            folderC.a = 1; 
            // Background highlight
            EditorGUI.DrawRect(selectionRect, folderC);

            var textC = folder.textColor;
            textC.a = 1;
            // Custom label
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                normal =
                {
                    textColor = textC
                }
            };

            EditorGUI.LabelField(selectionRect, $"📁 {obj.name}", style);
        }
    }
}
