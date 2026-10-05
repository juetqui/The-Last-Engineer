using System;
using UnityEngine;

public class Inspectionable : MonoBehaviour, IInteractable, IActivationSource
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

    // Una vez limpio queda limpio: como fuente de activacion nunca vuelve a false.
    private bool _isCleaned = false;

    public CorruptionGenerator CorruptionGenerator { get; private set; }

    public event Action OnFinished;
    public event Action OnCleaned;
    public event Action<bool> OnActivationChanged;

    public InspectionType Type { get { return _type; } }
    public bool IsActive => _isCleaned;

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
        _negativeFM.StopParticles();
        _positiveFM.StartParticles();
        OnFinished?.Invoke();
        OnCleaned?.Invoke();

        _isCleaned = true;
        OnActivationChanged?.Invoke(true);

        _collider.enabled = false;
    }
}

public enum InspectionType
{
    None,
    Panel,
    Battery
}
