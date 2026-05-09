using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tools/Map Builder/Palette", fileName = "NewMapBuilderPalette")]
public class MapBuilderPalette : ScriptableObject
{
    public List<GameObject> prefabs = new List<GameObject>();
}

