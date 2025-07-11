using UnityEngine;
using UnityEngine.UI;


public class UpdateCrosshair : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] Image _circleImage;
    //[SerializeField] Image _crossImage;
    
    private Animator _myAnim;
   
    private void Awake()
    {
        _myAnim = GetComponent<Animator>();
        _myAnim.speed = 1f;
        UpdatePos(null);
    }

    void Start()
    {
        GlitchActive.Instance.OnStopableSelected += UpdatePos;
        GlitchActive.Instance.OnChangeObjectState += TryPlayInvalidAnim;
    }

    private void UpdatePos(Glitcheable glitcheable)
    {
        _myAnim.SetBool("IsActivated", false);
        _myAnim.SetBool("HasTarget", glitcheable != null);

        if (glitcheable == null)
        {
            ResetPos();
            return;
        }

        Vector3 targetPosition = glitcheable.transform.position;
        Vector3 screenPosition = _camera.WorldToScreenPoint(targetPosition);

        if (screenPosition.z > 0) CompareGlitchWithPlayerNode(glitcheable, screenPosition);
        else ResetPos();
    }
    
    private void ResetPos()
    {
        _circleImage.enabled = false;
        _circleImage.rectTransform.position = Vector3.zero;
        
        //_crossImage.enabled = false;
        //_crossImage.rectTransform.position = Vector3.zero;
        
        _myAnim.SetBool("IsActivated", false);
        _myAnim.SetBool("HasTarget", false);
    }

    private void CompareGlitchWithPlayerNode(Glitcheable glitcheable, Vector3 screenPosition)
    {
        bool compatible =
            (PlayerTDController.Instance.GetCurrentNode().NodeType == NodeType.Corrupted && glitcheable.IsCorrupted) ||
            (PlayerTDController.Instance.GetCurrentNode().NodeType == NodeType.Default && !glitcheable.IsCorrupted);

        //_crossImage.enabled = compatible;
        _circleImage.enabled = !compatible;
        _circleImage.rectTransform.position = screenPosition;

        //(_crossImage.enabled ? _crossImage : _circleImage).rectTransform.position = screenPosition;
    }

    private void TryPlayInvalidAnim(Glitcheable glitcheable, InteractionOutcome interactionResult)
    {
        if (glitcheable == null) return;
        
        if (interactionResult.Result == InteractResult.Invalid)
            _myAnim.SetTrigger("InvalidAction");
    }

    public void SetUpdateAnim()
    {
        _myAnim.SetBool("IsActivated", true);
    }
}
