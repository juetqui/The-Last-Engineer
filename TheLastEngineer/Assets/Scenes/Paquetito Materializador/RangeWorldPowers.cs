using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class RangeWorldPowers : MonoBehaviour
{

    public Transform Spawnpoint;
    int SelectedItem = 0;
    PlayerTDController playerTDController;
    public List<Materializer> ObjetosIntecambiables; // Editable desde el Inspector
    public static RangeWorldPowers Instance;
    public event Action WorldChange;
    //public Camera Camera;
    public Transform plyaertransform;
    public LayerMask LayerMask;
    public float TeleportSphereRange;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        StartInputs();
        playerTDController = GetComponent<PlayerTDController>();
    }
    // Start is called before the first frame update
    public void Update()
    {

        if (Input.GetKey(KeyCode.Space) && playerTDController._currentNodeType == NodeType.Blue)
        {
            List<Materializer> DetectionHits = new List<Materializer>(); // Editable desde el Inspector
            Collider[] hitColliders = Physics.OverlapSphere(plyaertransform.position, TeleportSphereRange, LayerMask);
            foreach (Collider hit in hitColliders)
            {
                print(hit.gameObject.name);

                if (hit.GetComponent<Materializer>() != null)
                {
                    print("s");

                    DetectionHits.Add(hit.GetComponent<Materializer>());
                }

            }
            if (ObjetosIntecambiables.Count != 0)
            {
                foreach (var item in ObjetosIntecambiables)
                {
                    if (!DetectionHits.Contains(item))
                    {
                        item.IsAble = false;
                        ObjetosIntecambiables.Remove(item);
                        if (ObjetosIntecambiables.Count - 1 <= 0)
                        {
                            break;
                        }
                    }
                }
            }
            foreach (var item in DetectionHits)
            {
                if (ObjetosIntecambiables.Count == 0)
                {
                    ObjetosIntecambiables.Add(item);
                    item.IsAble = true;


                }
                else if (!ObjetosIntecambiables.Contains(item))
                {
                    ObjetosIntecambiables.Add(item);
                    item.IsAble = true;

                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                ObjetosIntecambiables[SelectedItem].IsSelected = false;
                SelectedItem++;
                if (SelectedItem >= ObjetosIntecambiables.Count)
                {
                    SelectedItem = 0;
                }
                ObjetosIntecambiables[SelectedItem].IsSelected = true;
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                ObjetosIntecambiables[SelectedItem].IsSelected = false;
                SelectedItem--;
                if (SelectedItem < 0)
                {
                    SelectedItem = ObjetosIntecambiables.Count - 1;
                }
                ObjetosIntecambiables[SelectedItem].IsSelected = true;
            }

        }
        else if (Input.GetKeyUp(KeyCode.Space) && playerTDController._currentNodeType == NodeType.Blue)
        {
            if (WorldChange != null)

            {
                WorldChange.Invoke();

            }
            foreach (var item in ObjetosIntecambiables)
            {
                if (item.IsSelected)
                {
                   
                    item.ArtificialMaterialize();

                }
                item.IsSelected = false;

                item.IsAble = false;

            }
            ObjetosIntecambiables.Clear();
        }
    }
    public void StartInputs()
    {
        InputManager.Instance.onInputsEnabled += OnEnableInputs;
        InputManager.Instance.onInputsDisabled += OnDisableInputs;
        if (InputManager.Instance.playerInputs.Player.enabled) OnEnableInputs();
    }
    public void OnEnableInputs()
    {
        InputManager.Instance.shieldInput.performed += ModoDetective;
        InputManager.Instance.modoIzq.performed += ModoDetective;
        InputManager.Instance.modoDer.performed += ModoDetective;
    }
    public void OnDisableInputs()
    {
        InputManager.Instance.shieldInput.performed -= ModoDetective;
        InputManager.Instance.modoIzq.performed -= ModoDetectiveIzq;
        InputManager.Instance.modoDer.performed -= ModoDetectiveDer;

    }
    private void OnDestroy()
    {
        InputManager.Instance.onInputsEnabled -= OnEnableInputs;
        InputManager.Instance.onInputsDisabled -= OnDisableInputs;
    }

    public void ModoDetective(InputAction.CallbackContext context)
    {

    }
    public void ModoDetectiveIzq(InputAction.CallbackContext context)
    {
        print("hola");
    }
    public void ModoDetectiveDer(InputAction.CallbackContext context)
    {

    }
}
