using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeInController : MonoBehaviour
{
    public Image blackScreen;
    public float fadeDuration = 2f;

    private void Awake()
    {
        blackScreen.gameObject.SetActive(true);
        blackScreen.color = new Color(0, 0, 0, 1);
    }

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1 - (timer / fadeDuration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        blackScreen.color = new Color(0, 0, 0, 0);

        blackScreen.gameObject.SetActive(false);
    }
}