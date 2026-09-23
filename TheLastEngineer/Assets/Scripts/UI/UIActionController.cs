using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PromptAction = InputPromptDatabase.PromptAction;

public class UIActionController : MonoBehaviour
{
    [SerializeField] private Image inputBtn;
    [SerializeField] private TextMeshProUGUI inputText;

    private Image _bgImg;
    private InputPromptIcon _promptIcon;

    private void Start()
    {
        _bgImg = GetComponent<Image>();
        _promptIcon = inputBtn.GetComponent<InputPromptIcon>();
        SetUpUI(false);

        PlayerController.Instance.OnInteractableDetected += OnInteractableDetected;
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.OnInteractableDetected -= OnInteractableDetected;
    }

    private void OnInteractableDetected(IInteractable interactable)
    {
        if (interactable == null)
        {
            SetUpUI(false);
            return;
        }

        SetUpText(interactable);
        SetUpUI(true);
    }

    private void SetUpUI(bool active)
    {
        // _bgImg.enabled = active;
        inputBtn.gameObject.SetActive(active);
        inputText.gameObject.SetActive(active);
    }

    private void SetUpText(IInteractable interactable)
    {
        var action = PromptAction.Interact;

        if (interactable is Connection)
        {
            inputText.text = "Put";
        }
        else if (interactable is NodeController)
        {
            // "Grab" y no "Take": Take es ahora la acción de sacarle glitch a un objeto.
            inputText.text = "Grab";
        }
        else if (interactable is Glitcheable glitcheable)
        {
            action = SetUpGlitchText(glitcheable);
        }
        else
        {
            inputText.text = "Caso no contemplado";
        }

        if (_promptIcon != null) _promptIcon.SetAction(action);
    }

    /// <summary>
    /// El Glitcheable solo llega acá como fallback de PlayerController.ResolveGlitchHudTarget, que
    /// ya garantiza que al menos una dirección es posible. Las dos a la vez solo se dan con objeto
    /// y nodo en Intangible: el texto nombra ambas y el ícono muestra Set.
    /// </summary>
    private PromptAction SetUpGlitchText(Glitcheable glitcheable)
    {
        var node = PlayerNodeHandler.Instance != null ? PlayerNodeHandler.Instance.CurrentGlitch : null;

        var canSet = GlitchTransferManager.Preview(node, glitcheable.Glitch) == GlitchTransferResult.Transferred;
        var canTake = GlitchTransferManager.Preview(glitcheable.Glitch, node) == GlitchTransferResult.Transferred;

        if (canSet && canTake)
        {
            inputText.text = "Set / Take";
            return PromptAction.Set;
        }

        if (canTake)
        {
            inputText.text = "Take";
            return PromptAction.Take;
        }

        inputText.text = "Set";
        return PromptAction.Set;
    }
}
