using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tools.FolderTool.Editor
{
    public class FolderSceneProcessor : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        private readonly Queue<Transform> _transformsToRemoveParent = new();

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var folders = Object.FindObjectsByType<FolderTool.HierarchyFolder>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .ToList();

            while (folders.Count > 0)
            {
                var actualFolder = folders[0];
                folders.RemoveAt(0);

                foreach (Transform child in actualFolder.transform)
                {
                    _transformsToRemoveParent.Enqueue(child.transform);
                }

                while (_transformsToRemoveParent.Count > 0)
                {
                    _transformsToRemoveParent.Dequeue().parent = null;
                }

                Object.DestroyImmediate(actualFolder.gameObject);
            }
        }
    }
}