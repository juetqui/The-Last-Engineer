using System.Collections.Generic;
using UnityEngine;

public class ConnectionNegativeFeedback : MonoBehaviour
{
    [SerializeField] private Connection _connection;

    private List<ParticleSystem> _errorPS = new List<ParticleSystem>();

    private void Start()
    {
        _errorPS = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
        _connection.OnNodeConnected += playPS;
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
