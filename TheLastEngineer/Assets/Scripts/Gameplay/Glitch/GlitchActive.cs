using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlitchActive : MonoBehaviour
{
    public static GlitchActive Instance = null;

    [SerializeField] private float _detectionRange = 10f;
    [SerializeField] private GameObject _interactionArea;

    private PlayerNodeHandler _player = default;
    private Glitcheable _selectedGlitcheable = null;
    private List<Glitcheable> _glitcheables = default;
    private int _index = 0;
    private bool _enabled = false;
    private Coroutine _currentCoroutine = null;

    public Action<Glitcheable> OnStopableSelected = delegate { };
    public Action<Glitcheable, InteractionOutcome> OnChangeObjectState = delegate { };

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        _player = GetComponent<PlayerNodeHandler>();
        _glitcheables = new List<Glitcheable>();
    }

    void Start()
    {
        PlayerNodeHandler.Instance.OnNodeGrabbed += CheckNode;
    }

    void Update()
    {
        if (!_enabled) return;

        _glitcheables = GetStopablesInArea();

        if (_selectedGlitcheable != null && !_glitcheables.Contains(_selectedGlitcheable))
        {
            _selectedGlitcheable = null;
            _index = 0;
        }

        if (_glitcheables.Count > 0)
        {
            if (_index >= _glitcheables.Count)
                _index = 0;

            _selectedGlitcheable = _glitcheables[_index];
        }
        else
        {
            _selectedGlitcheable = null;
            _index = 0;
        }

        OnStopableSelected?.Invoke(_selectedGlitcheable);

        if (Input.GetKeyDown(KeyCode.I) && _glitcheables.Count > 0)
            _index = (_index >= _glitcheables.Count - 1) ? 0 : _index + 1;

        if (Input.GetKeyDown(KeyCode.U) && _glitcheables.Count > 0)
            _index = (_index <= 0) ? _glitcheables.Count - 1 : _index - 1;
    }

    public void ChangeObjectState()
    {
        OnChangeObjectState?.Invoke(_selectedGlitcheable, CheckInteraction());
    }

    public InteractionOutcome CheckInteraction()
    {
        if (_selectedGlitcheable == null)
            return new InteractionOutcome(InteractResult.Invalid);

        bool incompatible =
            (_player.CurrentType == NodeType.Corrupted && _selectedGlitcheable.IsCorrupted) ||
            (_player.CurrentType == NodeType.Default   && !_selectedGlitcheable.IsCorrupted);

        if (incompatible)
            return new InteractionOutcome(InteractResult.Invalid);

        return new InteractionOutcome(InteractResult.Valid);
    }

    private void CheckNode(bool hasNode, NodeType nodeType)
    {
        if (!hasNode || nodeType == NodeType.None)
        {
            OnStopableSelected(null);
            _index = 0;
            _enabled = false;
            return;
        }

        _currentCoroutine = StartCoroutine(ActivateArea());
    }

    private List<Glitcheable> GetStopablesInArea()
    {
        List<Glitcheable> glitcheables = new List<Glitcheable>();
        
        Collider[] hitColliders = Physics.OverlapSphere(_player.transform.position, _detectionRange);

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out Glitcheable glitcheable))
            {
                if (glitcheables.Contains(glitcheable)) glitcheables.Remove(glitcheable);
                else glitcheables.Add(glitcheable);
            }
        }

        return glitcheables.OrderBy(glitch => Vector3.Distance(_player.transform.position, glitch.transform.position)).ToList();
    }

    private IEnumerator ActivateArea()
    {
        yield return new WaitForSeconds(0.5f);
        _enabled = true;
    }
}

public enum InteractResult { Valid, Invalid }

public readonly struct InteractionOutcome
{
    public readonly InteractResult Result;
    public InteractionOutcome(InteractResult result) => Result = result;
}
