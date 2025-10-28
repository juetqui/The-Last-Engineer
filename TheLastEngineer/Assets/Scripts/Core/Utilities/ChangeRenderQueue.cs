using UnityEngine;

public class ChangeRenderQueue : MonoBehaviour
{
    [SerializeField] private int customRenderQueue = 5000;

    void Start()
    {
        Renderer objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null && objectRenderer.material != null)
        {
            objectRenderer.material.renderQueue = customRenderQueue;
        }
    }
}
