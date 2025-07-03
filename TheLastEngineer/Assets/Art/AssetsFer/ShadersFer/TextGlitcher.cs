//using TMPro;
//using UnityEngine;

//public class TextGlitcher : MonoBehaviour
//{
//    public TextMeshProUGUI textMesh;
//    public float glitchFrequency = 0.2f;
//    public string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@#";
//    public float glitchRestore = 1;

//    private string originalText;

//    void Start()
//    {
//        originalText = textMesh.text;
//        InvokeRepeating(nameof(DoGlitch), 0f, glitchFrequency);
//    }



//    void DoGlitch()
//    {
//        char[] glitched = originalText.ToCharArray();
//        int count = Random.Range(3, glitched.Length);
//        for (int i = 0; i < count; i++)
//        {
//            int index = Random.Range(0, glitched.Length);
//            glitched[index] = characters[Random.Range(0, characters.Length)];
//        }

//        textMesh.text = new string(glitched);

//        // Restaurar luego de breve tiempo
//        Invoke(nameof(RestoreText), glitchRestore);
//    }

//    void RestoreText()
//    {
//        textMesh.text = originalText;
//    }
//}

using System.Collections;
using TMPro;
using UnityEngine;

public class TextGlitcher : MonoBehaviour
{
    public TextMeshPro textMesh;
    [Tooltip("Time to display original text before glitching (seconds)")]
    public float originalTextDuration = 2f;
    [Tooltip("Total duration of the glitch effect (seconds)")]
    public float glitchDuration = 1f;
    [Tooltip("Frequency of text changes during glitch (seconds)")]
    public float glitchChangeFrequency = 0.1f;
    [Tooltip("Characters used for glitch effect")]
    public string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@#";

    private string originalText;

    void Start()
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
        }
        originalText = textMesh.text;
        StartCoroutine(GlitchCycle());
    }

    private IEnumerator GlitchCycle()
    {
        while (true)
        {
            // Display original text
            textMesh.text = originalText;
            yield return new WaitForSeconds(originalTextDuration);

            // Start glitch effect
            float glitchEndTime = Time.time + glitchDuration;
            while (Time.time < glitchEndTime)
            {
                DoGlitch();
                yield return new WaitForSeconds(glitchChangeFrequency);
            }

            // Ensure text is restored at the end
            textMesh.text = originalText;
        }
    }

    private void DoGlitch()
    {
        char[] glitched = originalText.ToCharArray();
        int count = Random.Range(3, glitched.Length);
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, glitched.Length);
            glitched[index] = characters[Random.Range(0, characters.Length)];
        }
        textMesh.text = new string(glitched);
    }
}