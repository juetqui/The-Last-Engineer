using System;
using System.Collections;
using UnityEngine;

public class GlitchActive : MonoBehaviour
{
    public static GlitchActive Instance = null;

    [SerializeField] private NodeType _requiredNode = NodeType.Green;
    [SerializeField] private float _detectionRange = 6f;
    [SerializeField] private float _scaleSpeed = 10f;
    [SerializeField] private GameObject _interactionArea;

    private PlayerTDController _player = default;
    private float _detectionOffset = 2f, _offsetRange = 0f;
    private bool _enabled = false;

    public Action<Glitcheable> OnStopObject = delegate { };
    public Action<Glitcheable> OnStopableSelected = delegate { };

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        _player = GetComponent<PlayerTDController>();
        _offsetRange = _detectionRange + _detectionOffset;
    }

    void Start()
    {
        PlayerTDController.Instance.OnNodeGrabed += CheckNode;
    }

    void Update()
    {
        if (_enabled)
        {
            Glitcheable glitcheable = GetNearestStopable();

            OnStopableSelected?.Invoke(glitcheable);

            if (Input.GetKeyDown(KeyCode.V) && glitcheable != null)
            {
                OnStopObject?.Invoke(glitcheable);
                Debug.Log("HOLA");
            }
        }
    }

    private void CheckNode(bool hasNode, NodeType nodeType)
    {
        if (!hasNode || nodeType != _requiredNode)
        {
            StartCoroutine(DeactivateArea());
            return;
        }

        StartCoroutine(ActivateArea());
        _enabled = true;
    }

    private Glitcheable GetNearestStopable()
    {
        Collider[] hitColliders = Physics.OverlapSphere(_player.transform.position, _detectionRange);
        Glitcheable closest = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out Glitcheable glitcheable))
            {
                float distance = Vector3.Distance(_player.transform.position, glitcheable.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = glitcheable;
                }
            }
        }

        return closest;
    }

    private IEnumerator ActivateArea()
    {
        while (_interactionArea.transform.localScale.x < _offsetRange)
        {
            Vector3 newScale = new Vector3(_scaleSpeed, 0f, _scaleSpeed);

            _interactionArea.transform.localScale += newScale * Time.deltaTime;
            yield return null;
        }

        _interactionArea.transform.localScale = new Vector3(_offsetRange, 0.1f, _offsetRange);
        _enabled = true;
    }

    private IEnumerator DeactivateArea()
    {
        _enabled = false;

        while (_interactionArea.transform.localScale.x > 0.1f)
        {
            Vector3 newScale = new Vector3(5f, 0f, 5f);

            _interactionArea.transform.localScale -= newScale * Time.deltaTime;
            yield return null;
        }

        _interactionArea.transform.localScale = Vector3.one * 0.1f;
    }
}
