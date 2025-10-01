using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ConnectionPositiveFeedback : MonoBehaviour
{
    private List<ParticleSystem> _positivePS = new List<ParticleSystem>();
    private Connection _connection;
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _corruptedColor;

    private void Awake()
    {
        _positivePS = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
        _connection = GetComponentInParent<Connection>();
        _connection.OnNodeConnected += playPS;
    }

    void playPS(NodeType nodeType, bool connected)
    {
        if (connected && nodeType == _connection.RequiredType)
        {
            foreach (var ps in _positivePS)
            {
                ParticleSystem.MainModule module = ps.main;

                if(nodeType == NodeType.Default)
                    module.startColor = _defaultColor;
                else
                    module.startColor = _corruptedColor;

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
