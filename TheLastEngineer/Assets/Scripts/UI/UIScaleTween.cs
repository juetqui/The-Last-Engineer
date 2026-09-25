using System;
using PrimeTween;
using UnityEngine;

/// <summary>
/// Transicion de escala para mostrar y ocultar elementos de HUD sin que aparezcan ni desaparezcan
/// de golpe. Es una clase plana para que la compartan UpdatePosToTarget (un solo visual) y
/// UIActionController (el panel de glitch y sus dos hijos) con la misma curva.
/// </summary>
public class UIScaleTween
{
    private readonly float _minScale;
    private readonly float _maxScale;
    private readonly float _duration;
    private readonly Ease _ease;

    public float MinScale => _minScale;

    public UIScaleTween(float minScale, float maxScale, float duration, Ease ease)
    {
        _minScale = minScale;
        _maxScale = maxScale;
        _duration = duration;
        _ease = ease;
    }

    public void Show(RectTransform rect)
    {
        if (rect == null) return;

        Stop(rect);

        // PrimeTween no corta los tweens cuando el target se desactiva, asi que forzamos el
        // punto de partida para que un show pegado a un hide no arranque de una escala media.
        rect.localScale = Vector3.one * _minScale;

        Tween.Scale(rect, _maxScale, _duration, _ease);
    }

    /// <summary>
    /// onHidden corre recien cuando termino de achicarse. Si un Show interrumpe la transicion no
    /// corre: StopAll no completa los tweens.
    /// </summary>
    public void Hide(RectTransform rect, Action onHidden = null)
    {
        if (rect == null) return;

        Stop(rect);

        var tween = Tween.Scale(rect, _minScale, _duration, _ease);

        if (onHidden != null) tween.OnComplete(onHidden, warnIfTargetDestroyed: false);
    }

    /// <summary>Deja el rect oculto sin transicion, para el estado inicial.</summary>
    public void SnapHidden(RectTransform rect)
    {
        if (rect == null) return;

        Stop(rect);
        rect.localScale = Vector3.one * _minScale;
    }

    public void Stop(RectTransform rect)
    {
        // Ojo: StopAll con un target null frena TODOS los tweens del juego, de ahi el guard.
        if (rect != null) Tween.StopAll(onTarget: rect);
    }
}
