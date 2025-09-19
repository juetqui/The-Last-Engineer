using System;
using UnityEngine;

public class PlatformTeleport : MonoBehaviour, IInteractable
{
    public InteractablePriority Priority => InteractablePriority.Villero;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => false;

    [SerializeField] private PlatformTeleport _targetPlatform;
   // [SerializeField] private Camera portalCamera;
    [SerializeField] private Renderer _renderer;

    private RenderTexture portalTexture;
    private Camera _portalCamera;

    public PlatformTeleport TargetPlatform { get { return _targetPlatform; } }

    public Vector3 TargetPos {  get; private set; }

    public Action<bool> OnPlayerStepped = delegate { };

    private void Start()
    {
        TargetPos = _targetPlatform.transform.position;
        PlayerOn(false);
        OnPlayerStepped += PlayerOn;
        _targetPlatform.OnPlayerStepped += PlayerOn;

        _portalCamera = GetComponentInChildren<Camera>();
        _renderer = GetComponentInChildren<Renderer>();

        // Creamos RenderTexture para mostrar la c�mara del portal vinculado
        portalTexture = new RenderTexture(Screen.width, Screen.height, 24);
       // _targetPlatform.portalCamera.targetTexture = portalTexture;

        // Asignamos la RT al material del Quad
        _renderer.material.SetTexture("_MainTex", portalTexture);
    }

    private void PlayerOn(bool playerStepped)
    {
        if (playerStepped)
            _renderer.material.SetFloat("_TRansparencyREduction", 0f);
        else
            _renderer.material.SetFloat("_TRansparencyREduction", 1f);
    }

    public bool CanInteract(PlayerNodeHandler playerNodeHandler) => playerNodeHandler.CurrentType == NodeType.Corrupted && playerNodeHandler != null;

    public void Interact(PlayerNodeHandler playerNodeHandler, out bool succededInteraction)
    {
        if (!CanInteract(playerNodeHandler))
        {
            succededInteraction = false;
            return;
        }
        
        succededInteraction = true;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player))
        {
            OnPlayerStepped?.Invoke(true);
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player))
        {
            OnPlayerStepped?.Invoke(false);
        }
    }
}
