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
    [SerializeField] private GameObject loadingAnimator;
    [SerializeField] private Image loadingFade;
    [SerializeField] private Image loadingBar;
    
    [SerializeField] private Animator elevatorAnim;

    [Header("Loading Bar")]
    [SerializeField] private float loadingOffset = 0.1f;
    [Tooltip("Segundos minimos que tarda la barra en llenarse, aunque la escena ya este lista.")]
    [SerializeField] private float minLoadingDuration = 3f;

    [Header("Fade")]
    [SerializeField] private float fadeDuration;
    [SerializeField] private float levelChangeDelay;
    [SerializeField] private float initialFadeDelay = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool debug = false;
    [SerializeField] private string debugScene;

    private string _scene;
    private float _barProgress;
    private float _loadingStartTime;
    private bool _canUpdateLoadingBar;
    private bool _isLoading;
    private AsyncOperation _asyncScene;
    
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
        loadingAnimator.SetActive(false);
    }

    private void Start()
    {
        Tween.Alpha(loadingFade, 0f, fadeDuration, startDelay: initialFadeDelay, useUnscaledTime: true);
    }

    private void Update()
    {
        if (!_canUpdateLoadingBar || _asyncScene == null) return;

        // La barra nunca adelanta ni al disco ni al reloj: manda el que va mas atrasado.
        var sceneProgress = Mathf.Clamp01(loadingOffset + _asyncScene.progress);
        var timeProgress = minLoadingDuration <= 0f
            ? 1f
            : Mathf.Clamp01((Time.unscaledTime - _loadingStartTime) / minLoadingDuration);

        _barProgress = Mathf.Min(sceneProgress, timeProgress);
        loadingBar.fillAmount = _barProgress;
    }

    private async Task ChangeLevel()
    {
        _barProgress = 0f;
        loadingBar.fillAmount = 0f;
        
        _asyncScene = SceneManager.LoadSceneAsync(_scene);
        _asyncScene.allowSceneActivation = false;

        loadingCanvas.SetActive(true);
        loadingAnimator.SetActive(true);
        _loadingStartTime = Time.unscaledTime;
        _canUpdateLoadingBar = true;
        
        elevatorAnim.SetTrigger("NewStartLoading");
        elevatorAnim.SetTrigger("IsLoading");

        // El fade arranca recien cuando la barra completo su fill, no cuando termino de cargar la escena.
        while (_barProgress < 1f) await Task.Yield();

        elevatorAnim.ResetTrigger("NewStartLoading");
        elevatorAnim.ResetTrigger("IsLoading");
        elevatorAnim.SetTrigger("LoadingComplete");

        _canUpdateLoadingBar = false;

        await Tween.Delay(levelChangeDelay, useUnscaledTime: true);
        await Tween.Alpha(loadingFade, 1f, fadeDuration, useUnscaledTime: true);

        _asyncScene.allowSceneActivation = true;
        while (!_asyncScene.isDone) await Task.Yield();

        loadingCanvas.SetActive(false);
        loadingAnimator.SetActive(false);
        
        _asyncScene = null;
    }

    public void SetScene(string scene)
    {
        if (_isLoading) return;

        SetFade(scene);
    }

    private async void SetFade(string scene)
    {
        _isLoading = true;

        await Tween.Alpha(loadingFade, 1f, fadeDuration, useUnscaledTime: true);

        OnLoading?.Invoke();
        _scene = scene;
        var changeTask = ChangeLevel();

        await Tween.Alpha(loadingFade, 0f, fadeDuration, useUnscaledTime: true);
        await changeTask;
        await Tween.Alpha(loadingFade, 0f, fadeDuration, useUnscaledTime: true);

        _isLoading = false;
    }

    public void PerformDebug() => SetScene(debugScene);
}
