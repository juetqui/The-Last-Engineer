using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformWalls : MonoBehaviour
{
    MeshRenderer renderer;
    Material material;
    PlayerTDController player;
    Vector2 vector2=new Vector2(-6.5f,2.5f);
    public bool playerHasCorruption;
    public bool IsDisolving;
    private void Start()
    {
        renderer = GetComponent<MeshRenderer>();
        material = renderer.material;
        material.SetVector("_MinMaxPos",vector2);
        player = PlayerTDController.Instance;
    }
    private void Update()
    {
        if (player.GetCurrentNodeType() == NodeType.Corrupted)
        {
            material.SetFloat("_Alpha", 0f);
            playerHasCorruption=true;
            GetComponent<Collider>().enabled = false;
        }
        else if(!IsDisolving)
        {
            material.SetFloat("_Alpha", 1f);
            playerHasCorruption = false;
            GetComponent<Collider>().enabled = true;

        }
    }
    public void ResetDesintegrateWall()
    {
        IsDisolving=false;
    }
    public void StopDesintegrateWall()
    {
        material.SetFloat("_Alpha", 0f);
    }
    public void SetDesintegrateWall(float alpha)
    {
        material.SetFloat("_Alpha", alpha);

        if (!playerHasCorruption)
        {
            IsDisolving = true;
            material.SetFloat("_Alpha", alpha);
        }
        else 
        {
            material.SetFloat("_Alpha", 0);
        }
    }
}
