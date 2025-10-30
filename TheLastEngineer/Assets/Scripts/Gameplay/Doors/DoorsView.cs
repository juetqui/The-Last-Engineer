using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.VisualScripting;

public class DoorsView : MonoBehaviour
{
    [SerializeField] private bool _isBroken = false;
    private Animator _animator = default;
    private ParticleSystem _particulas = default;
    private AudioSource _openDoor = default;

    [SerializeField] private List<Renderer> _doorLight;

    [ColorUsageAttribute(true, true)]
    private Color _doorOpen = Color.green;
    [ColorUsageAttribute(true, true)]
    private Color _doorClosed = Color.red;

    public Action<bool> OnOpen;

    public void Initialize()
    {
        if (_doorLight != null)
            for (int i = 0; i < _doorLight.Count; i++)
                _doorLight[i].material.SetColor("_EmissiveColor", _doorClosed);

        _animator = GetComponent<Animator>();
        _openDoor = GetComponent<AudioSource>();
        _particulas = GetComponentInChildren<ParticleSystem>();
        if (_isBroken) _animator.SetBool("IsBroken", true);

    }

    private void Start()
    {
    }
    
    public void OpenDoor(bool isRunning)
    {
        if (_isBroken)
            return;

        _animator.SetBool("DoorActivated", isRunning);
        OnOpen?.Invoke(isRunning);

        if (isRunning)
        {
            if (_doorLight != null)
                for (int i = 0; i < _doorLight.Count; i++)
                    _doorLight[i].material.SetColor("_EmissiveColor", _doorOpen);

            _particulas.Play();
            _openDoor.Play();
        }
        else
        {
            _particulas.Stop();
            if (_doorLight != null)
                for (int i = 0; i < _doorLight.Count; i++)
                    _doorLight[i].material.SetColor("_EmissiveColor", _doorClosed);
        }
    }
}
