using UnityEngine;

public class SpecificConnectionView
{
    private NodeType _type;
    private MeshRenderer _renderer;
    private ParticleSystem _ps;
    private AudioSource _source;
    private Collider _triggerCollider;
    private Color _color, _secColor, _fresnelColor;

    public SpecificConnectionView(NodeType type, MeshRenderer renderer, Collider triggerCollider, Color color, Color secColor, Color fresnelColor, ParticleSystem ps, AudioSource source)
    {
        _type = type;
        _renderer = renderer;
        _triggerCollider = triggerCollider;
        _color = color;
        _secColor = secColor;
        _fresnelColor = fresnelColor;
        _ps = ps;
        _source = source;
    }

    public void OnStart()
    {
        SetTexture();
        _renderer.material.SetColor("_Color", _color);
        _renderer.material.SetColor("_SecColor", _secColor);
        _renderer.material.SetColor("_FresnelColor", _fresnelColor);
        Enable(true);
        PlayEffect(false);
    }

    public void Enable(bool value) => _renderer.enabled = value;
    public void EnableTrigger(bool value) => _triggerCollider.enabled = value;
    public void ChangeColor(Color color) => _renderer.material.SetColor("Color", color);
    public void PlayEffect(bool turnOnOff) => _ps.gameObject.SetActive(turnOnOff);

    public void PlayClip(AudioClip audio, float speed)
    {
        _source.clip = audio;
        _source.pitch = speed;
        _source.Play();
    }

    private void SetTexture()
    {
        string textureName = _type == NodeType.Corrupted ? "T_PurpleNode" : "T_YellowNode";
        _renderer.material.SetTexture("_Texture2D", Resources.Load<Texture2D>($"Textures/{textureName}"));
    }
}
