using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInteraction : MonoBehaviour
{
    private PlayerTDController _player = null;
    private ParticleSystem _ps = default;

    private void Awake()
    {
        _ps = GetComponentInChildren<ParticleSystem>();
    }

    void Start()
    {
        _ps.Stop();
    }

    void Update()
    {
        if (_player != null)
        {
            Vector3 targetPos = new Vector3(_player.transform.position.x, transform.position.y, _player.transform.position.z);
            _ps.transform.position = targetPos;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerTDController player))
        {
            _player = player;
            _ps.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerTDController player))
        {
            _player = null;
            _ps.Stop();
        }
    }
}
