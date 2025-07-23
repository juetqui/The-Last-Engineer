using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] private Transform _camera;

    void Update()
    {
        transform.LookAt(_camera);
    }
}
