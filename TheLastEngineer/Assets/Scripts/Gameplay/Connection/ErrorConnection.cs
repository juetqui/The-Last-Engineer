using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorConnection : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> _errorPS = new List<ParticleSystem>();
    [SerializeField] private GenericConnectionController _puertaRequirement;

    private void Start()
    {

        _errorPS = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
        _puertaRequirement = GetComponentInParent<GenericConnectionController>();
        _puertaRequirement.OnNodeConnected += playPS;
    }

    void playPS(NodeType nodeType, bool connected)
    {
        if(connected && nodeType == NodeType.Corrupted)
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
