using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissiveControl : MonoBehaviour
{
    [SerializeField] private GenericConnectionController _connection;
    [SerializeField] private NodeType _requiredNode = NodeType.None;
    [SerializeField] Material m_EmissiveOn;
    [SerializeField] Material m_EmissiveOff;
    [SerializeField] Material m_EmissiveCompleted;
    [SerializeField] bool startsOn;
    [SerializeField] bool canBeTurnedOff=true;
    Material emissiveMaterial;
    MeshRenderer meshRenderer;
    // Start is called before the first frame update
    private void Awake()
    {
        if(_connection!=null)
        _connection.OnNodeConnected += SetEmissiveOn;
        meshRenderer = GetComponent<MeshRenderer>();
        emissiveMaterial = meshRenderer.materials[meshRenderer.materials.Length-1];
    }
    void Start()
    {
        if (startsOn)
        {
            TurnOn();
        }
        else
        {
            TurnOff();
        }
    }
    public void TurnOn()
    {
        print("prendo");
        meshRenderer.materials[meshRenderer.materials.Length - 1] = m_EmissiveOn;
        List<Material> materials = new List<Material>();
        for(int i=0; i< meshRenderer.materials.Length; i++)
        {
            materials.Add(meshRenderer.materials[i]);
        }
        meshRenderer.SetMaterials(materials);
    }
    public void TurnOnCompleated()
    {
        meshRenderer.materials[meshRenderer.materials.Length - 1] = m_EmissiveCompleted;

    }
    public void TurnOff()
    {
        print("apago");
        meshRenderer.materials[meshRenderer.materials.Length - 1] = m_EmissiveOff;
        List<Material> materials = new List<Material>();
        for (int i = 0; i < meshRenderer.materials.Length; i++)
        {
            materials.Add(meshRenderer.materials[i]);
        }
        meshRenderer.SetMaterials(materials);
    }
    // Update is called once per frame

    public void SetEmissiveOn(NodeType node, bool isActive)
    {
        if (node == _requiredNode && isActive)
        {
            TurnOn();
        }
        else
        {
            if (canBeTurnedOff)
            {
                TurnOff();
            }
        }
    }
}
