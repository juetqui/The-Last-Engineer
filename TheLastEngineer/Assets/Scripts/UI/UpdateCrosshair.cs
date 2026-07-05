using System;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class UpdateCrosshair : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] Image _circleImage;

    [SerializeField] private Color defaultColor;
    [SerializeField] private Color glitchColor;

    [Header("Crosshair Scale")]
    [SerializeField] private float targetMin = 0.01f;
    [SerializeField] private float targetMax = 1f;
    [SerializeField] private float easeTime = 0.5f;
    [SerializeField] private float easeDelay = 0.25f;
    [SerializeField] private Ease easeType = Ease.OutBack;

    private Animator _myAnim;
    private Glitcheable _currentTarget;
   
    private void Awake()
    {
        _myAnim = GetComponent<Animator>();
        _myAnim.speed = 1f;
        UpdatePos(null);
    }

    void Start()
    {
        PlayerController.Instance.OnGlitcheableInArea += UpdatePos;
    }

    private void Update()
    {
        if (_currentTarget == null) return;

        var targetPosition = _currentTarget.transform.position;
        var screenPosition = _camera.WorldToScreenPoint(targetPosition);

        if (screenPosition.z > 0) CompareGlitchWithPlayerNode(_currentTarget, screenPosition);
        else ResetPos();
    }

    private void UpdatePos(Glitcheable glitcheable)
    {
        _myAnim.SetBool("IsActivated", false);
        _myAnim.SetBool("HasTarget", glitcheable != null);
        _currentTarget = glitcheable;

        if (glitcheable == null || PlayerNodeHandler.Instance.CurrentType == NodeType.None)
        {
            ResetPos();
            return;
        }
    }
    
    private void ResetPos()
    {
        SetCircleEnabled(false);
        _circleImage.rectTransform.position = Vector3.zero;
        
        _myAnim.SetBool("IsActivated", false);
        _myAnim.SetBool("HasTarget", false);
    }

    private void CompareGlitchWithPlayerNode(Glitcheable glitcheable, Vector3 screenPosition)
    {
        var compatible =
            (PlayerNodeHandler.Instance.CurrentType == NodeType.Corrupted && glitcheable.IsCorrupted) ||
            (PlayerNodeHandler.Instance.CurrentType == NodeType.Default && !glitcheable.IsCorrupted);

        SetCircleEnabled(!compatible);
        _circleImage.rectTransform.position = screenPosition;
        _circleImage.color = glitcheable.IsCorrupted ? glitchColor : defaultColor;
    }

    private void SetCircleEnabled(bool value)
    {
        if (_circleImage.enabled == value) return;

        Tween.StopAll(onTarget: _circleImage.rectTransform);

        if (value)
        {
            _circleImage.enabled = true;
            Tween.Scale(_circleImage.rectTransform, targetMax, easeTime, easeType, 1, CycleMode.Restart, easeDelay);
        }
        else
        {
            Tween.Scale(_circleImage.rectTransform, targetMin, easeTime, easeType, 1, CycleMode.Restart, easeDelay)
                .OnComplete(() => _circleImage.enabled = false);
        }
    }

    public void SetUpdateAnim()
    {
        _myAnim.SetBool("IsActivated", true);
    }
}
