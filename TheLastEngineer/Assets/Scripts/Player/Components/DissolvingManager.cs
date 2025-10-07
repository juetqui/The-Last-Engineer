using UnityEngine;

public class DissolvingManager : MonoBehaviour
{
    private Renderer _renderer = default;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        PlayerController.Instance.OnDissolving += Dissolve;
    }

    private void Dissolve(float timer)
    {
        _renderer.material.SetFloat("_DissolveAmount", timer);
    }
}
