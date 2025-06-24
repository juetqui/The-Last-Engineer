using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private Material screenTransitionMaterial;
    [SerializeField] private float transitionTime = 1.0f;
    [SerializeField] private string propertyName = "_Progress";

    public UnityEvent OnTransitionDone;
    public UnityEvent OnTransitionStart;

    private void Start()
    {
        StartCoroutine(TransitionCoroutineStart());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(TransitionCoroutineEnd());
            //LoadNextLevel();
        }
    }


    private IEnumerator TransitionCoroutineStart()
    {
        float currentTime = 0f;
        while (currentTime < transitionTime)
        {
            currentTime += Time.deltaTime;
            float progress = Mathf.Clamp01(currentTime / transitionTime);
            screenTransitionMaterial.SetFloat(propertyName, progress);
            yield return null;
        }
        OnTransitionStart?.Invoke();
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private IEnumerator TransitionCoroutineEnd()
    {
        float currentTime = 0f;
        while (currentTime < transitionTime)
        {
            currentTime += Time.deltaTime;
            float progress = Mathf.Clamp01(currentTime / transitionTime);
            screenTransitionMaterial.SetFloat(propertyName, progress);
            yield return null;
        }
        LoadNextLevel();
    }

    //public void LoadNextLevel()
    //{
    //    StartCoroutine(TransitionCoroutine(SceneManager.GetActiveScene().buildIndex + 1));
    //}
}
