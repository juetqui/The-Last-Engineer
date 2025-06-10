using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlitchActive : MonoBehaviour
{
    public static GlitchActive Instance = null;

    [SerializeField] private NodeType _requiredNode = NodeType.Green;
    [SerializeField] private float _detectionRange = 6f;
    [SerializeField] private float _scaleSpeed = 10f;
    [SerializeField] private GameObject _interactionArea;

    private PlayerTDController _player = default;
    private List<Glitcheable> _glitcheables = default;
    private float _detectionOffset = 5f, _offsetRange = 0f;
    private int _index = 0;
    private bool _enabled = false;

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
            _glitcheables = GetStopablesInArea(ref _glitcheables);
            Glitcheable glitcheable = _glitcheables.Count > 0 ? _glitcheables[_index] : _glitcheables.FirstOrDefault();
            OnStopableSelected?.Invoke(glitcheable);

            if (Input.GetKeyDown(KeyCode.I) && _glitcheables.Count > 0)
            {
                if (_index >= _glitcheables.Count - 1)
                {
                    _index = 0;
                }
                else
                {
                    _index++;
                }
            }
            
            if (Input.GetKeyDown(KeyCode.U) && _glitcheables.Count > 0)
            {
                if (_index <= 0)
                {
                    _index = _glitcheables.Count - 1;
                }
                else
                {
                    _index--;
                }
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
        if (!hasNode || nodeType != _requiredNode)
        {
            OnStopableSelected(null);
            _index = 0;
            StartCoroutine(DeactivateArea());
            return;
        }

        StartCoroutine(ActivateArea());
    }
    private Glitcheable GetNearestStopable()
    {
        Collider[] hitColliders = Physics.OverlapSphere(_player.transform.position, _detectionRange*40);
        Glitcheable closest = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out Glitcheable glitcheable))
            {
                if (Vector2.Distance(new Vector2(_player.transform.position.x, _player.transform.position.z), new Vector2(hit.ClosestPoint(_player.transform.position).x, hit.ClosestPoint(_player.transform.position).z)) <= _detectionRange)
                {
                    float distance = Vector3.Distance(_player.transform.position, glitcheable.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closest = glitcheable;
                    }

                }

            }
        }

        return closest;
    }

    private List<Glitcheable> GetStopablesInArea(ref List<Glitcheable> glitcheables)
    {
        glitcheables.Clear();
        
        Collider[] hitColliders = Physics.OverlapSphere(_player.transform.position, _detectionRange*40);

        foreach (var hit in hitColliders)
        {
            
                if (hit.TryGetComponent(out Glitcheable glitcheable))
            {
                if (Vector2.Distance(new Vector2(_player.transform.position.x, _player.transform.position.z), new Vector2(hit.ClosestPoint(_player.transform.position).x, hit.ClosestPoint(_player.transform.position).z)) <= _detectionRange)
                {
                    //print(Vector2.Distance(new Vector2(_player.transform.position.x, _player.transform.position.z), new Vector2(hit.transform.position.x, hit.transform.position.z)));
                    //print(_detectionRange);

                    glitcheables.Add(glitcheable);

                }
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
