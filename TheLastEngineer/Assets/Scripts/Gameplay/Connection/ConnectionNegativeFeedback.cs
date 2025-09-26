using System.Collections.Generic;
using UnityEngine;

public class ConnectionNegativeFeedback : MonoBehaviour
{
    private List<ParticleSystem> _errorPS = new List<ParticleSystem>();
    private Connection _connection;

    private void Start()
    {
        _errorPS = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
        _connection = GetComponentInParent<Connection>();
        _connection.OnNodeConnected += playPS;
    }

    void playPS(NodeType nodeType, bool connected)
    {
        if(connected && nodeType != _connection.RequiredType)
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
