using UnityEngine;

/// <summary>Draws a <see cref="Rope"/> as one continuous LineRenderer through its segment joints.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(LineRenderer))]
public class RopeLine : MonoBehaviour
{
    [Tooltip("Rope to draw. Empty = the Rope on this GameObject.")]
    [SerializeField] private Rope _rope;

    private LineRenderer _line;
    private Vector3[] _points = new Vector3[0];

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        if (_rope == null && !TryGetComponent(out _rope))
        {
            Debug.LogError($"[RopeLine] '{name}' has no Rope to draw; disabled.", this);
            enabled = false;
            return;
        }

        _line.useWorldSpace = true;
        _line.positionCount = 0;
    }

    private void Start()
    {
        _line.widthMultiplier = _rope.Diameter;
    }

    private void LateUpdate()
    {
        int count = _rope.PointCount;
        if (_points.Length != count) _points = new Vector3[count];

        for (int i = 0; i < count; i++)
            _points[i] = _rope.GetPoint(i);

        _line.positionCount = count;
        _line.SetPositions(_points);
    }
}
