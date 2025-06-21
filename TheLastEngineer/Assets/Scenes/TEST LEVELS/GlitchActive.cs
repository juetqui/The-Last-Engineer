using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlitchActive : MonoBehaviour
{
    public static GlitchActive Instance = null;

    [SerializeField] private NodeType _requiredNode = NodeType.Green;
    [SerializeField] private float _detectionRange = 10f;
    [SerializeField] private float _scaleSpeed = 10f;
    [SerializeField] private GameObject _interactionArea;

    private PlayerTDController _player = default;
    private List<Glitcheable> _glitcheables = default;
    private float _detectionOffset = 5f, _offsetRange = 0f;
    private int _index = 0;
    private bool _enabled = false;
    private Coroutine _currentCoroutine = null;

    public Action<Glitcheable> OnStopObject = delegate { };
    public Action<Glitcheable> OnStopableSelected = delegate { };

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        _player = GetComponent<PlayerTDController>();
        _offsetRange = _detectionRange + _detectionOffset;
        _glitcheables = new List<Glitcheable>();
    }

    void Start()
    {
        PlayerTDController.Instance.OnNodeGrabed += CheckNode;
    }

    void Update()
    {
        if (_enabled)
        {
            Glitcheable glitcheable = null;

            _glitcheables = GetStopablesInArea(ref _glitcheables);

            if (_glitcheables.Count == 0)
            {
                _index = 0;
                OnStopableSelected?.Invoke(glitcheable);
            }
            else if (_index >= _glitcheables.Count)
            {
                _index = 0;
            }

            if (_glitcheables.Count > 0)
            {
                glitcheable = _glitcheables[_index];
            }

            OnStopableSelected?.Invoke(glitcheable);

            if (Input.GetKeyDown(KeyCode.I) && _glitcheables.Count > 0)
            {
                _index = (_index >= _glitcheables.Count - 1) ? 0 : _index + 1;
            }

            if (Input.GetKeyDown(KeyCode.U) && _glitcheables.Count > 0)
            {
                _index = (_index <= 0) ? _glitcheables.Count - 1 : _index - 1;
            }

            if (Input.GetKeyDown(KeyCode.V) && glitcheable != null)
                OnStopObject?.Invoke(glitcheable);
        }

        UpdateAreaPos();
    }

    private void UpdateAreaPos()
    {
        _interactionArea.transform.position = transform.position;
    }

    private void CheckNode(bool hasNode, NodeType nodeType)
    {
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        if (!hasNode || nodeType != _requiredNode)
        {
            OnStopableSelected(null);
            _index = 0;
            _currentCoroutine = StartCoroutine(DeactivateArea());
            return;
        }

        _currentCoroutine = StartCoroutine(ActivateArea());
    }

    private List<Glitcheable> GetStopablesInArea(ref List<Glitcheable> glitcheables)
    {
        glitcheables.Clear();
        
        Collider[] hitColliders = Physics.OverlapSphere(_player.transform.position, _detectionRange - _detectionOffset);

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out Glitcheable glitcheable))
            {
                glitcheables.Add(glitcheable);
            }
        }

        return glitcheables.OrderBy(glitch => Vector3.Distance(_player.transform.position, glitch.transform.position)).ToList();
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
            Vector3 newScale = new Vector3(_scaleSpeed, 0f, _scaleSpeed);

            _interactionArea.transform.localScale -= newScale * Time.deltaTime;
            yield return null;
        }

        _interactionArea.transform.localScale = Vector3.one * 0.1f;
    }
}
