using System;
using UnityEngine;

public class Inspectionable : MonoBehaviour, IInteractable
{
    #region -----INTERFACE VARIABLES-----
    public InteractablePriority Priority => InteractablePriority.Low;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => false;
    #endregion

    [SerializeField] private InspectionType _type = InspectionType.None;
    
    private ParticleSystem _ps = default;

    public event Action OnFinished;

    public InspectionType Type { get { return _type; } }

    private void Start()
    {
        _ps = GetComponentInChildren<ParticleSystem>();
    }

    public bool CanInteract(PlayerNodeHandler playerNodeHandler)
    {
        return playerNodeHandler != null;
    }

    public void Interact(PlayerNodeHandler playerNodeHandler, out bool succededInteraction)
    {
        _ps.Play();
        succededInteraction = true;
    }

    public void StopInteraction()
    {
        _ps.Stop();
        OnFinished?.Invoke();
    }
}

public enum InspectionType
{
    None,
    Panel,
    Battery
}
