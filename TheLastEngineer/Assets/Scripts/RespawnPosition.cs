using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPosition : MonoBehaviour
{
    [SerializeField] Transform _respawnPos;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController> (out PlayerController playerController))
        {
            playerController.SetCheckPointPos(_respawnPos.position);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
