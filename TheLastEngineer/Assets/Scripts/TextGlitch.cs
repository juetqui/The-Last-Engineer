using System.Collections;
using UnityEngine;
using TMPro;

public class TextGlitch : MonoBehaviour
{
    [Header("Referencia")]
    [SerializeField] private string _tutorialText;

    [Header("Configuración Glitch")]
    [SerializeField] private float glitchDuration = 0.5f;
    [SerializeField] private float glitchInterval = 0.05f;
    [SerializeField] private string randomChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!#$%&?"; 

    private TMP_Text tmpText = default;
    private string originalText = default;

    private Coroutine glitchCoroutine = default;

    private void Awake()
    {
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();

        originalText = _tutorialText;
        tmpText.text = originalText;
    }

    /// <summary>
    /// Muestra un texto específico inmediatamente.
    /// Se puede llamar desde un AnimationEvent.
    /// </summary>
    public void ShowText()
    {
        if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);
        tmpText.text = originalText;
    }

    /// <summary>
    /// Vuelve al texto original, pero antes hace glitch.
    /// Se puede llamar desde un AnimationEvent.
    /// </summary>
    public void ReturnWithGlitch()
    {
        if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);
        glitchCoroutine = StartCoroutine(DoGlitch(originalText));
    }

    private IEnumerator DoGlitch(string finalText)
    {
        float timer = 0f;

        while (timer < glitchDuration)
        {
            tmpText.text = GetRandomizedText(finalText.Length);
            yield return new WaitForSeconds(glitchInterval);
            timer += glitchInterval;
        }

        tmpText.text = finalText;
        glitchCoroutine = null;
    }

    private string GetRandomizedText(int length)
    {
        char[] chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = randomChars[Random.Range(0, randomChars.Length)];
        }
        return new string(chars);
    }
}