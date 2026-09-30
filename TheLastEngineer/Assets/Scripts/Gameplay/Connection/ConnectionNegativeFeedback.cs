using System.Collections.Generic;
using UnityEngine;

public class ConnectionNegativeFeedback : MonoBehaviour
{
    [SerializeField] private Connection _connection;

    private List<ParticleSystem> _errorPS = new List<ParticleSystem>();

    // Awake y no Start: el evento del nodo pre-asignado sale de Connection.Start.
    private void Awake()
    {
        _errorPS = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
        _connection.OnNodeConnected += playPS;
    }

    private void OnDestroy()
    {
        if (_connection != null)
            _connection.OnNodeConnected -= playPS;
    }

    void playPS(GlitchState nodeType, bool connected)
    {
        if(connected && nodeType != _connection.RequiredLevel)
        {
            foreach (var ps in _errorPS)
            {
                ps.Play();
            }
        }
        else
        {
            foreach (var ps in _errorPS)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
