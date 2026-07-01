using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;
    
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private Image loadingFade;
    [SerializeField] private Image loadingBar;
    [SerializeField] private float loadingScale;
    [SerializeField] private float loadingOffset;

    [Header("Fade")]
    [SerializeField] private float fadeDuration;
    [SerializeField] private float levelChangeDelay;
    [Range(0f, 0.9f)]
    [SerializeField] private float fadeInProgress;

    [Header("Debug")]
    [SerializeField] private string debugScene;

    private string _scene;
    private float _target;
    private bool _canUpdateLoadingBar;
    
    public Action OnLoading;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        loadingCanvas.SetActive(false);
    }

    private void Start()
    {
        Tween.Delay(1.5f);
        Tween.Alpha(loadingFade, 0f, fadeDuration);
    }

    private void Update()
    {
        if (!_canUpdateLoadingBar) return;
        
        loadingBar.fillAmount = Mathf.MoveTowards(loadingBar.fillAmount, _target, loadingScale * Time.deltaTime);
    }

    private async Task ChangeLevel()
    {
        _target = 0f;
        loadingBar.fillAmount = 0f;
        
        var asyncScene = SceneManager.LoadSceneAsync(_scene);
        asyncScene.allowSceneActivation = false;

        loadingCanvas.SetActive(true);

        var fadeStarted = false;

        do
        {
            await Task.Delay(100);
            _target = loadingOffset + asyncScene.progress;

            if (!fadeStarted && asyncScene.progress >= fadeInProgress)
            {
                fadeStarted = true;
                Tween.Delay(levelChangeDelay);
                Tween.Alpha(loadingFade, 1f, fadeDuration);
            }
        } while (asyncScene.progress < 0.9f);

        if (!fadeStarted)
        {
            Tween.Delay(levelChangeDelay);
            Tween.Alpha(loadingFade, 1f, fadeDuration);
        }

        await Task.Delay(1000);

        asyncScene.allowSceneActivation = true;
        loadingCanvas.SetActive(false);
        _canUpdateLoadingBar = false;
    }

    public void SetScene(string scene)
    {
        if (_canUpdateLoadingBar) return;

        SetFade(scene);
    }

    private async void SetFade(string scene)
    {
        await Tween.Alpha(loadingFade, 1f, fadeDuration);

        OnLoading?.Invoke();
        _scene = scene;
        _canUpdateLoadingBar = true;
        var changeTask = ChangeLevel();

        await Tween.Alpha(loadingFade, 0f, fadeDuration);
        await changeTask;
        await Tween.Delay(levelChangeDelay);
        await Tween.Alpha(loadingFade, 0f, fadeDuration);
    }

    public void PerformDebug() => SetScene(debugScene);
}
