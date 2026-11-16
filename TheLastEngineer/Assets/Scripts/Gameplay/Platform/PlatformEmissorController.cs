using UnityEngine;

public class PlatformEmissorController : MonoBehaviour
{
    [SerializeField] private Connection _connection;
    
    private Renderer _renderer;

    private void Awake()
    {
        // _connection.OnNodeConnected += OnNodeConnected;
        _renderer = GetComponent<Renderer>();

        var color = GlitchPalette.Default.ColorFor(_connection.RequiredLevel);
        _renderer.material.SetColor("_EmissionColor", color);
    }

    // private void OnNodeConnected(GlitchState state, bool connected)
    // {
    //     var color = connected ? GlitchPalette.Default.ColorFor(state) : Color.black;
    //     _renderer.material.SetColor("_EmissionColor", color);
    // }
}
