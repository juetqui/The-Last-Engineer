using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PromptAction = InputPromptDatabase.PromptAction;

/// <summary>
/// HUD de la acción disponible. En modo Interactable muestra el prompt de Interact (Put / Grab)
/// sobre el nodo o la conexión seleccionada. En modo Glitcheable vive en un panel padre con un
/// hijo por acción (Set y Take) y prende cada uno según lo que la carga permita mover.
/// </summary>
public class UIActionController : MonoBehaviour
{
    [Tooltip("Que señal del PlayerController muestra este HUD.")]
    [SerializeField] private UITargetSource _source = UITargetSource.Interactable;

    [Header("Interactable")]
    [SerializeField] private Image inputBtn;
    [SerializeField] private TextMeshProUGUI inputText;

    [Header("Glitcheable")]
    [Tooltip("Panel padre. No se desactiva, solo se escala.")]
    [SerializeField] private RectTransform _glitchRoot;
    [SerializeField] private GlitchActionPanel _setPanel = new GlitchActionPanel();
    [SerializeField] private GlitchActionPanel _takePanel = new GlitchActionPanel();

    [Header("Glitcheable Scale Tween")]
    [SerializeField] private float _minScale = 0f;
    [SerializeField] private float _maxScale = 1f;
    [SerializeField] private float _scaleDuration = 0.2f;
    [SerializeField] private Ease _scaleEase = Ease.OutBack;

    [Serializable]
    private class GlitchActionPanel
    {
        public RectTransform panel;
        public Image inputBtn;
        public TextMeshProUGUI inputText;

        [NonSerialized] public bool shown;
        [NonSerialized] public LayoutElement layout;
    }

    private InputPromptIcon _promptIcon;
    private UIScaleTween _scaleTween;
    private bool _rootShown;

    private void Awake()
    {
        _scaleTween = new UIScaleTween(_minScale, _maxScale, _scaleDuration, _scaleEase);
    }

    private void Start()
    {
        if (_source == UITargetSource.Glitcheable)
        {
            InitGlitchPanels();
        }
        else
        {
            _promptIcon = inputBtn.GetComponent<InputPromptIcon>();
            SetUpUI(inputBtn, inputText, false);
        }

        Subscribe(true);
    }

    private void OnDestroy()
    {
        _scaleTween?.Stop(_glitchRoot);
        _scaleTween?.Stop(_setPanel.panel);
        _scaleTween?.Stop(_takePanel.panel);

        Subscribe(false);
    }

    private void Subscribe(bool value)
    {
        var player = PlayerController.Instance;
        if (player == null) return;

        if (_source == UITargetSource.Glitcheable)
        {
            if (value) player.OnGlitcheableDetected += OnGlitcheableDetected;
            else player.OnGlitcheableDetected -= OnGlitcheableDetected;
        }
        else
        {
            if (value) player.OnInteractableDetected += OnInteractableDetected;
            else player.OnInteractableDetected -= OnInteractableDetected;
        }
    }

    private static void SetUpUI(Image btn, TextMeshProUGUI text, bool active)
    {
        if (btn != null) btn.gameObject.SetActive(active);
        if (text != null) text.gameObject.SetActive(active);
    }

    #region -----INTERACTABLE-----
    private void OnInteractableDetected(IInteractable interactable)
    {
        if (interactable == null)
        {
            SetUpUI(inputBtn, inputText, false);
            return;
        }

        SetUpText(interactable);
        SetUpUI(inputBtn, inputText, true);
    }

    private void SetUpText(IInteractable interactable)
    {
        if (interactable is Connection)
        {
            inputText.text = "Put";
        }
        else if (interactable is NodeController)
        {
            // "Grab" y no "Take": Take es ahora la acción de sacarle glitch a un objeto.
            inputText.text = "Grab";
        }
        else
        {
            inputText.text = "Caso no contemplado";
        }

        if (_promptIcon != null) _promptIcon.SetAction(PromptAction.Interact);
    }
    #endregion

    #region -----GLITCHEABLE-----
    private void InitGlitchPanels()
    {
        _rootShown = false;
        _scaleTween.SnapHidden(_glitchRoot);

        InitGlitchPanel(_setPanel);
        InitGlitchPanel(_takePanel);
    }

    private void InitGlitchPanel(GlitchActionPanel p)
    {
        if (p.panel == null) return;

        // El LayoutGroup del padre ignora la escala: un hijo achicado a cero seguiría ocupando su
        // lugar y dejaría un hueco. Se lo saca del layout mientras está oculto.
        p.layout = p.panel.GetComponent<LayoutElement>();
        if (p.layout == null) p.layout = p.panel.gameObject.AddComponent<LayoutElement>();

        p.shown = false;
        p.layout.ignoreLayout = true;
        _scaleTween.SnapHidden(p.panel);
        SetUpUI(p.inputBtn, p.inputText, false);
    }

    /// <summary>
    /// Llega todos los frames. Solo dispara tweens cuando cambia lo que se puede hacer, y cada
    /// panel se toca únicamente si su propio estado cambió: si deja de poder hacerse Take pero Set
    /// sigue, solo se achica Take.
    /// </summary>
    private void OnGlitcheableDetected(Glitcheable glitcheable)
    {
        var node = PlayerNodeHandler.Instance != null ? PlayerNodeHandler.Instance.CurrentGlitch : null;
        var obj = glitcheable != null ? glitcheable.Glitch : null;

        var canSet = GlitchTransferManager.Preview(node, obj) == GlitchTransferResult.Transferred;
        var canTake = GlitchTransferManager.Preview(obj, node) == GlitchTransferResult.Transferred;
        var showRoot = canSet || canTake;

        if (showRoot && !_rootShown)
        {
            _rootShown = true;
            _scaleTween.Show(_glitchRoot);
        }

        SetGlitchPanel(_setPanel, canSet);
        SetGlitchPanel(_takePanel, canTake);

        if (!showRoot && _rootShown)
        {
            _rootShown = false;
            _scaleTween.Hide(_glitchRoot);
        }
    }

    private void SetGlitchPanel(GlitchActionPanel p, bool show)
    {
        if (p.panel == null || p.shown == show) return;

        p.shown = show;

        if (show)
        {
            // Vuelve al layout antes de crecer, así crece ya en su lugar final.
            p.layout.ignoreLayout = false;
            SetUpUI(p.inputBtn, p.inputText, true);
            _scaleTween.Show(p.panel);
        }
        else
        {
            SetUpUI(p.inputBtn, p.inputText, false);

            // Sale del layout recién al terminar de achicarse: si saliera al pedir el ocultado, el
            // hermano saltaría a su lugar mientras este todavía se ve.
            _scaleTween.Hide(p.panel, () => p.layout.ignoreLayout = true);
        }
    }
    #endregion
}
