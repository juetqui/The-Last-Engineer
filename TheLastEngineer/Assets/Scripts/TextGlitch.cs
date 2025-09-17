using System.Collections;
using UnityEngine;
using TMPro;

public class TextGlitch : MonoBehaviour
{
    [Header("Referencia")]
    [SerializeField] public TMP_Text tmpText; // Asigná tu TextMeshPro en el inspector

    [Header("Configuración Glitch")]
    [SerializeField] private float glitchDuration = 0.5f;  // Duración total del glitch
    [SerializeField] private float glitchInterval = 0.05f; // Cada cuánto cambiar los caracteres
    [SerializeField] private string randomChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!#$%&?"; 

    private string originalText; // Guarda el texto original
    private Coroutine glitchCoroutine;

    private void Awake()
    {
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();

        originalText = tmpText.text;
    }

    /// <summary>
    /// Muestra un texto específico inmediatamente.
    /// Se puede llamar desde un AnimationEvent.
    /// </summary>
    public void ShowText(string newText)
    {
        if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);
        tmpText.text = newText;
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