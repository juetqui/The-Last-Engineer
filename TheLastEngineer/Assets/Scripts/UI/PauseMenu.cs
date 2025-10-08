using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _pauseRoot;   // Panel padre (Canvas hijo) del menú de pausa
    [SerializeField] private Button _btnContinue;
    [SerializeField] private Button _btnOptions;
    [SerializeField] private Button _btnSave;
    [SerializeField] private Button _btnMainMenu;

    [Header("Escenas")]
    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    [Header("Comportamiento")]
    [Tooltip("Si es true, AudioListener.pause se activa al pausar.")]
    [SerializeField] private bool _pauseAudioListener = true;

    public bool IsPaused { get; private set; }

    /// <summary>
    /// Evento para pedir abrir el menú de Opciones (aún no implementado).
    /// Suscribí tu OptionsMenuController cuando lo tengas listo.
    /// </summary>
    public event Action OnOptionsRequested;

    private void Awake()
    {
        // Asegurar que empieza oculto
        if (_pauseRoot != null) _pauseRoot.SetActive(false);
        IsPaused = false;
    }

    private void OnEnable()
    {
        if (_btnContinue) _btnContinue.onClick.AddListener(HandleContinue);
        if (_btnOptions) _btnOptions.onClick.AddListener(HandleOptions);
        if (_btnSave) _btnSave.onClick.AddListener(HandleSave);
        if (_btnMainMenu) _btnMainMenu.onClick.AddListener(HandleMainMenu);
    }

    private void OnDisable()
    {
        if (_btnContinue) _btnContinue.onClick.RemoveListener(HandleContinue);
        if (_btnOptions) _btnOptions.onClick.RemoveListener(HandleOptions);
        if (_btnSave) _btnSave.onClick.RemoveListener(HandleSave);
        if (_btnMainMenu) _btnMainMenu.onClick.RemoveListener(HandleMainMenu);
    }

    private void Update()
    {
        // Hotkey: Esc para pausar/reanudar
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused) ResumeGame();
            else PauseGame();
        }
    }

    // ====== Botones ======

    private void HandleContinue()
    {
        ResumeGame();
    }

    private void HandleOptions()
    {
        // Cierra el menú de pausa y notifica para abrir Opciones
        ResumeGame();
        OnOptionsRequested?.Invoke();
        Debug.Log("[PauseMenu] Opciones solicitado (implementalo en tu OptionsMenuController).");
    }

    private void HandleSave()
    {
        // Llama a tu SaveSystem (por ahora, stub)
        StartCoroutine(SaveGameStub());
    }

    private void HandleMainMenu()
    {
        StartCoroutine(LoadMainMenuRoutine());
    }

    // ====== Lógica de pausa ======

    public void PauseGame()
    {
        if (IsPaused) return;

        IsPaused = true;
        Time.timeScale = 0f;
        if (_pauseRoot) _pauseRoot.SetActive(true);

        if (_pauseAudioListener) AudioListener.pause = true;

        // Cursor visible y liberado
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("[PauseMenu] Juego pausado.");
    }

    public void ResumeGame()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;
        if (_pauseRoot) _pauseRoot.SetActive(false);

        if (_pauseAudioListener) AudioListener.pause = false;

        // Cursor según tu juego (por defecto: bloqueado y oculto)
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("[PauseMenu] Juego reanudado.");
    }

    private IEnumerator LoadMainMenuRoutine()
    {
        // Asegura normalizar tiempo/sonido antes de cambiar de escena
        Time.timeScale = 1f;
        if (_pauseAudioListener) AudioListener.pause = false;

        // Opcional: pequeña transición (deshabilitar menú para evitar doble click)
        if (_pauseRoot) _pauseRoot.SetActive(false);

        AsyncOperation op = SceneManager.LoadSceneAsync(_mainMenuSceneName, LoadSceneMode.Single);
        if (op == null)
        {
            Debug.LogError($"[PauseMenu] No se pudo cargar la escena '{_mainMenuSceneName}'. Verifica el nombre en Build Settings.");
            yield break;
        }

        // Podrías mostrar un spinner / fade aquí si quisieras.
        while (!op.isDone) yield return null;
    }

    private IEnumerator SaveGameStub()
    {
        // Acá luego llamás a tu clase SaveSystem.SaveToJson(...)
        Debug.Log("[PauseMenu] Guardado solicitado (pendiente implementar JSON).");
        // Simulación rápida (feedback):
        if (_btnSave) _btnSave.interactable = false;
        yield return null; // frame
        if (_btnSave) _btnSave.interactable = true;
    }

    private void OnApplicationQuit()
    {
        // Evita que el editor quede en timescale 0 si salís durante pausa
        Time.timeScale = 1f;
        if (_pauseAudioListener) AudioListener.pause = false;
    }
}
