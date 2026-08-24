using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIActionController : MonoBehaviour
{
    [SerializeField] private Image inputBtn;
    [SerializeField] private TextMeshProUGUI inputText;

    private Image _bgImg;
    
    private void Start()
    {
        _bgImg = GetComponent<Image>();
        SetUpUI(false);
        
        PlayerController.Instance.OnInteractableDetected += OnInteractableDetected;
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
        if (interactable == null)
        {
            inputText.text = "";   
        }
        else if (interactable is Connection)
        {
            inputText.text = "Put";   
        }
        else if (interactable is NodeController)
        {
            inputText.text = "Take";
        }
        else if (interactable is Glitcheable)
        {
            var glitcheable = (Glitcheable)interactable;
            inputText.text = glitcheable.IsCorrupted ? "Un-Glitch" : "Glitch";
        }
        else
        {
            inputText.text = "Caso no contemplado";
        }
    }
}
