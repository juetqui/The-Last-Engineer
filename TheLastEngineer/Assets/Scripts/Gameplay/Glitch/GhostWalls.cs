using UnityEngine;

public class GhostWalls : MonoBehaviour
{
    private MeshRenderer _renderer;
    Material material;
    PlayerController player;
    Vector2 vector2=new Vector2(-6.5f,2.5f);
    [SerializeField] Vector2 _minMaxPos;
    public bool playerHasCorruption;
    public bool IsDisolving;
    private void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        material = _renderer.material;
       // material.SetVector("_MinMaxPos",vector2);
        player = PlayerController.Instance;
        material.SetFloat("_MinPos", _minMaxPos.x);
        material.SetFloat("_MaxPos", _minMaxPos.y);
    }
    private void Update()
    {
        if (PlayerNodeHandler.Instance.CurrentType != NodeType.Corrupted)
        {
            material.SetFloat("_Alpha", 0f);
            playerHasCorruption=false;
            GetComponent<Collider>().enabled = false;
        }
        else if(!IsDisolving)
        {
            material.SetFloat("_Alpha", 1f);
            playerHasCorruption = true;
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
