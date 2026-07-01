using UnityEngine;

/// Local easing helpers. Replaces DOTween's DOVirtual.EasedValue so the Glitch
/// states no longer depend on DOTween (the project's tweening is handled by PrimeTween).
public static class EaseUtil
{
    // Equivalent to DOVirtual.EasedValue(0f, 1f, t, Ease.InOutQuad).
    public static float InOutQuad(float t)
    {
        t = Mathf.Clamp01(t);
        return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;
    }
}
