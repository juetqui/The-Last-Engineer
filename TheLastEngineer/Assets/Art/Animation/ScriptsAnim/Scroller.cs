using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{
    [SerializeField] private RawImage[] images;
    [SerializeField] private float x;
    [SerializeField] private float y;

    void Update()
    {
        foreach (RawImage img in images)
        {
            img.uvRect = new Rect(
                img.uvRect.position + new Vector2(x, y) * Time.deltaTime,
                img.uvRect.size
            );
        }
    }
}
