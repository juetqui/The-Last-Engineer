using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Reads the analog-movement tuning straight from the shipped PlayerData asset, so the tests check
/// the values the game uses today. Read through SerializedObject because the test assembly can't
/// reference Assembly-CSharp, where PlayerData lives.
/// </summary>
public readonly struct PlayerDataValues
{
    public const string AssetPath = "Assets/Scripts/Scriptable Objects/PlayerData.asset";

    public readonly AnimationCurve MoveSpeedByTilt;
    public readonly float MinMoveSpeedFraction;
    public readonly float MoveAnimBlendDampTime;

    private PlayerDataValues(AnimationCurve curve, float minFraction, float dampTime)
    {
        MoveSpeedByTilt = curve;
        MinMoveSpeedFraction = minFraction;
        MoveAnimBlendDampTime = dampTime;
    }

    public static PlayerDataValues Load()
    {
        var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetPath);
        Assert.IsNotNull(asset, $"PlayerData not found at {AssetPath}");

        var so = new SerializedObject(asset);
        return new PlayerDataValues(
            Find(so, "_moveSpeedByTilt").animationCurveValue,
            Find(so, "_minMoveSpeedFraction").floatValue,
            Find(so, "_moveAnimBlendDampTime").floatValue);
    }

    private static SerializedProperty Find(SerializedObject so, string name)
    {
        var property = so.FindProperty(name);
        Assert.IsNotNull(property, $"PlayerData has no serialized field '{name}'");
        return property;
    }
}
