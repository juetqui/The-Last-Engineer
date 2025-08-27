using System;
using Tools.FolderTool.FolderAttribute;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tools.FolderTool.Editor
{
    [InitializeOnLoad]
    public class OnlyScriptProcessor
    {
        static OnlyScriptProcessor()
        {
            EditorApplication.hierarchyChanged += EnforceOnlyScriptRule;
        }

        private static void EnforceOnlyScriptRule()
        {
            var all = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            foreach (var mb in all)
            {
                if (mb == null) continue;
                var type = mb.GetType();
                var hasAttr = Attribute.IsDefined(type, typeof(OnlyScriptAttribute));
                if (!hasAttr) continue;

                var behaviours = mb.gameObject.GetComponents<MonoBehaviour>();
                foreach (var other in behaviours)
                {
                    if (other == null || other == mb) continue;

                    Debug.LogWarning(
                        $"{type.Name} is marked [OnlyScript]. Removing {other.GetType().Name} from {mb.gameObject.name}.",
                        mb);
                    Object.DestroyImmediate(other);
                }
            }
        }
    }
}