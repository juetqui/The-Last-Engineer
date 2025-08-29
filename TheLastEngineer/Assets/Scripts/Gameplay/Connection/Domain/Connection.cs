using System.Collections;
using UnityEngine;

public abstract class Connection<T> : MonoBehaviour, IInteractable, IConnectable
{
    #region -----INTERFACE VARIABLES-----
    public InteractablePriority Priority => InteractablePriority.High;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => false;
    #endregion

    [SerializeField] protected NodeController _recievedNode = null;
    [SerializeField] protected GameObject refuerzoPositivo;

    private Coroutine _changingColor = null;

    public bool StartsConnected { get; private set; }

    private void Start()
    {
        if (_recievedNode != null)
        {
            SetNode(_recievedNode);
            StartsConnected = true;
        }
        else StartsConnected = false;
    }

    public abstract void SetSecTM(T secTM);               // compat: hoy es no-op
    protected abstract void SetNode(NodeController node);
    public abstract void UnsetNode(NodeController node = null);
    public abstract bool CanInteract(PlayerTDController player);

    public void Interact(PlayerTDController player, out bool succededInteraction)
    {
        if (_recievedNode != null)
        {
            succededInteraction = false;
        }
        else if (CanInteract(player) && player.GetCurrentNode() != null)
        {
            NodeController node = player.GetCurrentNode();
            SetNode(node);
            succededInteraction = true;
        }
        else
        {
            succededInteraction = false;
        }
    }

    public void SetPositiveFeedback(bool active)
    {
        if (refuerzoPositivo != null) refuerzoPositivo.SetActive(active);

        if (_changingColor != null) StopCoroutine(_changingColor);

        var targetColor = active ? Color.cyan : Color.red;
        _changingColor = StartCoroutine(ChangeColor(targetColor));
    }

    private IEnumerator ChangeColor(Color targetColor)
    {
        var renderer = GetComponent<Renderer>();
        if (renderer == null || renderer.material == null) yield break;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.05f;
            var current = renderer.material.GetColor("_EmissiveColor");
            var next = Color.Lerp(current, targetColor, t);
            renderer.material.SetColor("_EmissiveColor", next);
            yield return null;
        }

        renderer.material.SetColor("_EmissiveColor", targetColor);
        _changingColor = null;
    }

    // No usados por tu gameplay actual
    public void Interact(GameObject interactor) => throw new System.NotImplementedException();
    public bool CanInteract(GameObject interactor) => throw new System.NotImplementedException();
}
