using System;
using PrimeTween;
using UnityEngine;

public class FloorDitherManager : MonoBehaviour
{
    public static FloorDitherManager Instance;

    [Header("Materiales con dither (acomodar array en el orden desde el piso mas bajo hasta el mas alto.)")]
    [SerializeField] private Material[] materials;
    [SerializeField] private Ease ease;
    [SerializeField] private float easeDuration;
    
    [Header("Opacidad segun piso")]
    [SerializeField] private float targetLowerFloorOpacity;
    [SerializeField] private float targetUpperFloorOpacity;

    private float _targetOpacity = 0f;
    
    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        DitherFloor(FloorHeight.Base);
    }

    public void DitherFloor(FloorHeight fh)
    {
        var targetMat = materials[(int) fh];
        var currentTargetOpacity = targetMat.GetFloat("_Opacity");
        
        TweenFloat(targetMat, "_Opacity", currentTargetOpacity, 1f, easeDuration);

        var currentHeight = (int)fh;

        for (var i = 0; i < materials.Length; i++)
        {
            // if (i <= currentHeight) continue;
            if (i == currentHeight) continue;

            _targetOpacity = i == currentHeight - 1 ? targetLowerFloorOpacity : targetUpperFloorOpacity;

            if (i == currentHeight -1)
                _targetOpacity = targetLowerFloorOpacity;
            else if (i == currentHeight + 1)
            // if (i == currentHeight + 1)
                _targetOpacity = targetUpperFloorOpacity;
            else
                _targetOpacity = 0f;
            
            var currentOpacity = materials[i].GetFloat("_Opacity");
            TweenFloat(materials[i], "_Opacity", currentOpacity, _targetOpacity, easeDuration);
        }
    }
    
    private Tween TweenFloat(Material mat, string prop, float from, float to, float dur)
    {
        mat.SetFloat(prop, from);
        return Tween.Custom(gameObject, from, to, dur,
            (_, v) => mat.SetFloat(prop, v), ease);
    }
}

public enum FloorHeight
{
    Underground = 0,
    Base = 1,
    First = 2,
    Second = 3
}
