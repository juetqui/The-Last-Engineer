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
    [SerializeField] private Collider _collider = default;
    [SerializeField] private ParticlesFeedbackManager _positiveFM = default;
    [SerializeField] private ParticlesFeedbackManager _negativeFM = default;

    public CorruptionGenerator CorruptionGenerator { get; private set; }

    public event Action OnFinished;
    public event Action OnCleaned;

    public InspectionType Type { get { return _type; } }

    private void Start()
    {
        CorruptionGenerator = GetComponent<CorruptionGenerator>();
    }

    public bool CanInteract(PlayerNodeHandler playerNodeHandler)
    {
        return playerNodeHandler != null;
    }

    public void Interact(PlayerNodeHandler playerNodeHandler, out bool succededInteraction)
    {
        succededInteraction = true;
    }

    public void StopInteraction()
    {
        OnFinished?.Invoke();
    }

    public void CorruptionCleaned(CorruptionGenerator generator)
    {
        generator.OnObjectCleaned -= CorruptionCleaned;
        _collider.enabled = false;
        _negativeFM.StopParticles();
        _positiveFM.StartParticles();
        OnFinished?.Invoke();
        OnCleaned?.Invoke();
    }
}

public enum InspectionType
{
    None,
    Panel,
    Battery
}
