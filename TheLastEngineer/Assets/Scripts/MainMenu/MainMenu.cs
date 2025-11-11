using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Tween Transition")]
    [SerializeField] private LeanTweenType _tweentype = LeanTweenType.easeInOutSine;
    [SerializeField] private float _duration = 1f;

    [Header("Fade Transition")]
    [SerializeField] private Image _blackScreen;
    [SerializeField] private float fadeOutDuration = 2f;


    [SerializeField] private Transform _startPos = default;

    private string _targetLevel = "";

    void Start()
    {
        MoveToPos(_startPos);
        StartCoroutine(FadeTo(0f));
    }

    public void MoveToPos(Transform newPos)
    {
        LeanTween.move(gameObject, newPos.position, _duration).setEase(_tweentype);
        LeanTween.rotate(gameObject, newPos.rotation.eulerAngles, _duration).setEase(_tweentype);
    }

    public void MoveToPosAndFade(Transform newPos)
    {
        LeanTween.rotate(gameObject, newPos.rotation.eulerAngles, _duration)
            .setEase(_tweentype);

        LeanTween.move(gameObject, newPos.position, _duration)
            .setEase(_tweentype)
            .setOnComplete(() => StartCoroutine(FadeTo(1f, true)));
    }
    
    public void SetTargetLevel(string targetLevel)
    {
        _targetLevel = targetLevel;
    }

    public void Quit()
    {
        Application.Quit();
    }

    private IEnumerator FadeTo(float targetValue, bool loadLevel = false)
    {
        _blackScreen.gameObject.SetActive(true);

        float startAlpha = _blackScreen.color.a;
        float timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeOutDuration;
            float newAlpha = Mathf.Lerp(startAlpha, targetValue, t);

            _blackScreen.color = new Color(0, 0, 0, newAlpha);
            yield return null;
        }

        _blackScreen.color = new Color(0, 0, 0, targetValue);

        if (loadLevel && Mathf.Approximately(targetValue, 1f))
            SceneManager.LoadScene(_targetLevel);
        else
            _blackScreen.gameObject.SetActive(false);

    }
}
