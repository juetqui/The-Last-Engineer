using System;
using Tools.FolderTool.FolderAttribute;
using UnityEditor;
using UnityEngine;

namespace Tools.FolderTool
{
    [DisallowMultipleComponent, OnlyScript]
    public class HierarchyFolder : MonoBehaviour
    {
        public Color folderColor;
        public Color textColor;
        
        private void Reset()
        {
#if UNITY_EDITOR
            var icon = EditorGUIUtility.IconContent("d_Folder Icon").image as Texture2D;
            EditorGUIUtility.SetIconForObject(gameObject, icon);
#endif
        }
    }
}