using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColorError : MonoBehaviour
{
    [SerializeField] private Material currentMat, normalMat, errorMat;
    private Renderer rdrMat;
    private GenericConnectionController connection;
    private void Awake()
    {
        currentMat = normalMat;
    }

    private void Start()
    {
        connection = GetComponentInParent<GenericConnectionController>();
        connection.OnNodeConnected += CheckConnection;
        rdrMat = GetComponent<Renderer>();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void ChangeMatError()
    {
        rdrMat.material = errorMat;
    }

    void CheckConnection(NodeType nodeType, bool connected)
    { 
        if(connected && nodeType == NodeType.Corrupted)
        {
            ChangeMatError();
        }
        else
        {
            rdrMat.material = normalMat;
        }
    }
}
