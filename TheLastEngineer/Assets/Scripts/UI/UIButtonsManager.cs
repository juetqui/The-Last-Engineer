using System;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonsManager : MonoBehaviour
{
    public static UIButtonsManager Instance;
    
    [SerializeField] private AudioClip openMenuSound;
    [SerializeField] private AudioClip buttonUpSound;
    [SerializeField] private AudioClip buttonDownSound;
    [SerializeField] private AudioClip clickSound;
    
    private AudioSource _audioSource;

    private enum ButtonType
    {
        ButtonUp,
        ButtonDown,
        ButtonClick,
        MenuBtn
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        var canvasButtons = GetComponentsInChildren<Button>(true);

        foreach (var b in canvasButtons)
        {
            if (b.gameObject.GetComponent<DirectionalButtonListener>() == null)
                b.gameObject.AddComponent<DirectionalButtonListener>();
        }
    }

    private AudioClip TargetSound(ButtonType buttonType) => buttonType switch
    {
        ButtonType.MenuBtn => openMenuSound,
        ButtonType.ButtonUp => buttonUpSound,
        ButtonType.ButtonDown => buttonDownSound,
        ButtonType.ButtonClick => clickSound,
        _ => throw new ArgumentOutOfRangeException(nameof(buttonType), buttonType, null)
    };

    private void PlayTargetSound(ButtonType buttonType)
    {
        var targetSound = TargetSound(buttonType);
        _audioSource.clip = targetSound;
        _audioSource.Play();
    }
    
    public void PlaySoundClick() 
    {
        PlayTargetSound(ButtonType.ButtonClick);
    }

    public void PlaySoundUp() 
    {
        PlayTargetSound(ButtonType.ButtonUp);
    }

    public void PlaySoundDown() 
    {
        PlayTargetSound(ButtonType.ButtonDown);
    }

    public void PlaySoundOpenMenu() 
    {
        PlayTargetSound(ButtonType.MenuBtn);
    }
}
