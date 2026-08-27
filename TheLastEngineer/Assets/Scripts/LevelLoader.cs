using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;

    [SerializeField] private GameObject loadingAnimator;
    [SerializeField] private Image loadingFade;

    [SerializeField] private Animator elevatorAnim;

    [Header("Loading")]
    [Tooltip("Segundos minimos que se muestra la pantalla de carga, aunque la escena ya este lista.")]
    [SerializeField] private float minLoadingDuration = 3f;

    [Header("Fade")]
    [SerializeField] private float fadeDuration;
    [SerializeField] private float levelChangeDelay;
    [SerializeField] private float initialFadeDelay = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool debug = false;
    [SerializeField] private string debugScene;

    private string _scene;
    private float _loadingStartTime;
    private bool _isLoading;
    private bool _isDestroyed;
    private AsyncOperation _asyncScene;

    public Action OnLoading;

    private void Awake()
    {
        // La instancia duplicada se destruye y corta aca: sin el return seguiria
        // ejecutando (Awake y Start) sobre un objeto que ya esta marcado para destruirse.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        loadingAnimator.SetActive(false);
    }

    private void Start()
    {
        if (Instance != this) return;

        Tween.Alpha(loadingFade, 0f, fadeDuration, startDelay: initialFadeDelay, useUnscaledTime: true);
    }

    private void OnDestroy()
    {
        _isDestroyed = true;

        // Si nos destruyen a mitad de una carga, liberamos la escena pendiente
        // para que no quede trabada esperando el allowSceneActivation.
        if (_asyncScene != null) _asyncScene.allowSceneActivation = true;

        if (Instance == this) Instance = null;
    }

    // La escena no se activa hasta que pasen los segundos minimos de show Y el disco este listo.
    // Con allowSceneActivation = false, AsyncOperation.progress topea en 0.9f.
    private async Task WaitForLoadingScreen()
    {
        while (!_isDestroyed && (Time.unscaledTime - _loadingStartTime < minLoadingDuration || _asyncScene.progress < 0.9f))
            await Task.Yield();
    }

    private async Task ChangeLevel()
    {
        _asyncScene = SceneManager.LoadSceneAsync(_scene);
        _asyncScene.allowSceneActivation = false;

        loadingAnimator.SetActive(true);
        _loadingStartTime = Time.unscaledTime;

        elevatorAnim.SetTrigger("NewStartLoading");
        elevatorAnim.SetTrigger("IsLoading");

        await WaitForLoadingScreen();
        if (_isDestroyed) return;

        elevatorAnim.ResetTrigger("NewStartLoading");
        elevatorAnim.ResetTrigger("IsLoading");
        elevatorAnim.SetTrigger("LoadingComplete");

        await Tween.Delay(levelChangeDelay, useUnscaledTime: true);
        if (_isDestroyed) return;

        await Tween.Alpha(loadingFade, 1f, fadeDuration, useUnscaledTime: true);
        if (_isDestroyed) return;

        _asyncScene.allowSceneActivation = true;
        while (!_isDestroyed && !_asyncScene.isDone) await Task.Yield();
        if (_isDestroyed) return;

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
        if (_isDestroyed) return;

        OnLoading?.Invoke();
        _scene = scene;
        var changeTask = ChangeLevel();

        await Tween.Alpha(loadingFade, 0f, fadeDuration, useUnscaledTime: true);
        await changeTask;
        if (_isDestroyed) return;

        await Tween.Alpha(loadingFade, 0f, fadeDuration, useUnscaledTime: true);
        if (_isDestroyed) return;

        _isLoading = false;
    }

    public void PerformDebug() => SetScene(debugScene);
}
