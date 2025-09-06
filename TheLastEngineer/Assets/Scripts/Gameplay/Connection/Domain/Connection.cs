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
    protected MainTM _mainTM = default;
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

    public abstract void SetSecTM(T secTM);
    protected abstract void SetNode(NodeController node);
    public abstract void UnsetNode(NodeController node = null);

    public void SetMainTM()
    {
        _mainTM = MainTM.Instance;
    }

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
    
    public void SetPositiveFeedback(bool Active)
    {
        refuerzoPositivo.SetActive(Active);

        if (_changingColor != null) StopCoroutine(_changingColor);

        Color targetColor = Active ? Color.cyan : Color.red;
        _changingColor = StartCoroutine(ChangeColor(targetColor));
    }

    private IEnumerator ChangeColor(Color targetColor)
    {
        Renderer renderer = GetComponent<Renderer>();
        float counter = 0f;

        while (counter < 1f)
        {
            counter += Time.deltaTime * 0.05f;

            Color currentColor = renderer.material.GetColor("_EmissiveColor");
            Color newColor = Color.Lerp(currentColor, targetColor, counter);

            renderer.material.SetColor("_EmissiveColor", newColor);
            yield return null;
        }

        renderer.material.SetColor("_EmissiveColor", targetColor);
        _changingColor = null;
    }

    public void Interact(GameObject interactor)
    {
        throw new System.NotImplementedException();
    }

    public bool CanInteract(GameObject interactor)
    {
        throw new System.NotImplementedException();
    }
}
