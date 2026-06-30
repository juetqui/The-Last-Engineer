using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;
    
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private Image loadingBar;
    [SerializeField] private float loadingScale;
    [SerializeField] private float loadingOffset;

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

    private void Update()
    {
        if (!_canUpdateLoadingBar) return;
        
        loadingBar.fillAmount = Mathf.MoveTowards(loadingBar.fillAmount, _target, loadingScale * Time.deltaTime);
    }

    private async void ChangeLevel()
    {
        _target = 0f;
        loadingBar.fillAmount = 0f;
        
        var asyncScene = SceneManager.LoadSceneAsync(_scene);
        asyncScene.allowSceneActivation = false;
        
        loadingCanvas.SetActive(true);

        do
        {
            await Task.Delay(100);
            _target = loadingOffset + asyncScene.progress;
        } while (asyncScene.progress < 0.9f);
        
        await Task.Delay(1000);

        asyncScene.allowSceneActivation = true;
        loadingCanvas.SetActive(false);
        _canUpdateLoadingBar = false;
    }

    public void SetScene(string scene)
    {
        if (_canUpdateLoadingBar) return;
        
        _scene = scene;
        _canUpdateLoadingBar = true;
        ChangeLevel();
        OnLoading?.Invoke();
    }

    public void PerformDebug() => SetScene(debugScene);
}
