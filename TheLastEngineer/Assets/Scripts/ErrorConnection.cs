using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorConnection : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> _errorPS = new List<ParticleSystem>();
    [SerializeField] private GenericTM _puertaRequirement;

    private void Start()
    {
        _errorPS = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
        _puertaRequirement = GetComponent<GenericTM>();
    }




}
