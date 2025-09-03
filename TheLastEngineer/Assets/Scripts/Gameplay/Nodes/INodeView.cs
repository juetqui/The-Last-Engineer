using UnityEngine;
public interface INodeView
{
    void Initialize(Color outlineColor);
    void EnableCollider(bool on);
    void SetIdle();
    void SetCollected();
    void SetInRange();
    void SetOutlineColor(Color c);
    void PlayProximityFX(bool play); // partículas/FX cercanía

    // Disintegrate shader
    void StartDisintegrate(Shader shader, Color startColor, Vector2 minMax, float alpha = 1f);
    void SetDisintegrateAlpha(float alpha);
    void StopDisintegrate(Shader originalShader);
}
