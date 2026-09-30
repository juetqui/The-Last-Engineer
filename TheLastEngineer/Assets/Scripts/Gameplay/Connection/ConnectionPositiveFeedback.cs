using System.Collections.Generic;
using UnityEngine;

public class ConnectionPositiveFeedback : MonoBehaviour
{
    [SerializeField] private Connection _connection;

    // El tono sale de GlitchPalette; solo la transparencia es propia de estas particulas.
    [Range(0f, 1f)]
    [SerializeField] private float _alpha = 0.64f;

    private List<ParticleSystem> _positivePS = new List<ParticleSystem>();

    private void Awake()
    {
        _positivePS = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
        _connection.OnNodeConnected += playPS;
    }

    private void OnDestroy()
    {
        if (_connection != null)
            _connection.OnNodeConnected -= playPS;
    }

    void playPS(GlitchState nodeType, bool connected)
    {
        if (connected && nodeType == _connection.RequiredLevel)
        {
            Color color = GlitchPalette.Default.ColorFor(nodeType);
            color.a = _alpha;

            foreach (var ps in _positivePS)
            {
                ParticleSystem.MainModule module = ps.main;
                module.startColor = color;

                ps.Play();
            }
        }
        else
        {
            foreach (var ps in _positivePS)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
