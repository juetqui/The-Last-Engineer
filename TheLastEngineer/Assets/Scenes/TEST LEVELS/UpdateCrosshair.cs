using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class UpdateCrosshair : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] List<Image> _imageList = new List<Image>();
    [SerializeField] List<RectTransform> _imageRectTransformList = new List<RectTransform>();
    private RectTransform _crosshair = default;
    private Image _image = default;
    Animator _myAnim;
   
    bool IsAppear;
    private void Awake()
    {
        _crosshair = GetComponent<RectTransform>();
        _myAnim = GetComponent<Animator>();
        foreach (var item in _imageList)
        {
            _imageRectTransformList.Add(item.rectTransform);
        }
        //_image = GetComponent<Image>();
        //_camera = Camera.main;
        _myAnim.speed = 0;

        UpdatePos(null);
    }

    void Start()
    {
        GlitchActive.Instance.OnStopableSelected += UpdatePos;
        GlitchActive.Instance.OnStopableSelected += TargetChange;
    }

    private void UpdatePos(Glitcheable glitcheable)
    {
        if (glitcheable == null)
        {
            foreach (var item in _imageList)
            {
                item.enabled = false;
            }
            
            //_image.enabled = false;
            _crosshair.position = Vector3.zero;
            return;
        }
        else
        {
            foreach (var item in _imageList)
            {
                item.enabled = true;
            }
        }

        Vector3 targetPosition = glitcheable.transform.position;
        Vector3 screenPosition = _camera.WorldToScreenPoint(targetPosition);

        if (screenPosition.z > 0)
        {
            foreach (var item in _imageList)
            {
                item.enabled = true;
            }
            foreach (var item in _imageRectTransformList)
            {
                item.position = screenPosition;
            }
            //_image.enabled = true;
            //_crosshair.position = screenPosition;
        }
        else
        {
            foreach (var item in _imageList)
            {
                item.enabled = false;
            }
            //_image.enabled = false;
            _crosshair.position = Vector3.zero;
        }
    }
    public void TargetChange(Glitcheable glitcheable)
    {

        if (glitcheable == null)
        {
            _myAnim.SetBool("HasAppeared", false);
            _myAnim.SetBool("IsActivated", false);
            IsAppear = false;
            _myAnim.speed = 0;
            return;
        }
        if (glitcheable.IsStopped)
        {
            _myAnim.speed = 1;
            _myAnim.SetBool("HasAppeared", true);
            _myAnim.SetBool("IsActivated", true);
            IsAppear = true;
        }
        else if (IsAppear==false && !glitcheable.IsStopped)
        {

            _myAnim.speed = 1;

            //_myAnim.Play("LO-Appear");
        }
        else
        {
            _myAnim.SetBool("HasAppeared", true);
            _myAnim.SetBool("IsActivated", false);

        }
    }
    public void SetAppear()
    {
        IsAppear = true;
    }
}
