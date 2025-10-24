using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tools/Prefab Painter/Palette", fileName = "NewPrefabPalette")]
public class PrefabPainterPalette : ScriptableObject
{
    public List<GameObject> prefabs = new List<GameObject>();
}