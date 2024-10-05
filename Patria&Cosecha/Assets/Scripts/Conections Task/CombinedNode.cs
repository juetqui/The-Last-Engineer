using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinedNode : MonoBehaviour
{
    [SerializeField] CombinedType _combinedType;

    public CombinedType CombinedType { get { return _combinedType; } }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

public enum CombinedType
{
    CubeCapsule,
    CubeSphere,
    SphereCapsule
}
