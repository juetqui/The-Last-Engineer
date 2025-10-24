using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorruptionGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _prefabToSpawn;
    [SerializeField] private int _minInstances = 5;
    [SerializeField] private int _maxInstances = 10;
    [SerializeField] private float _offsetAboveSurface = 0.1f;

    private List<(int index, Vector3 position, Quaternion rotation)> _generatedCorruption = new List<(int index, Vector3 position, Quaternion rotation)>();
    private Corruption _currentActive = default;
    private Mesh _mesh = default;

    private float _maxParticles = 2000f;
    private float[] _triangleAreas = default;
    private float _totalArea = default;

    private int _index = 0;
    private int _totalInstances = 0;
    private int _cleanedInstances = 0;

    public int TotalInstances { get { return _totalInstances; } }
    public int CleanedInstances { get { return _cleanedInstances; } }

    public Action OnUpdatedInstances = delegate { };
    public Action<CorruptionGenerator> OnObjectCleaned = delegate { };

    void Start()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
        _currentActive = GetComponentInChildren<Corruption>();
        _currentActive.SetUpGenerator(this);

        SetUpObjectCorruption();
    }

    private void SetUpObjectCorruption()
    {
        _generatedCorruption.Clear();
        CalculateTriangleAreas(_mesh);

        _totalInstances = UnityEngine.Random.Range(_minInstances, _maxInstances);

        for (int i = 0; i < _totalInstances; i++)
        {
            (Vector3 point, Vector3 normal) = GetRandomPointOnMesh(_mesh);
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

            _generatedCorruption.Add((i, point, rotation));
        }

        SetUpNextCorruption();
    }

    private void CalculateTriangleAreas(Mesh targetMesh)
    {
        Vector3[] vertices = targetMesh.vertices;
        int[] triangles = targetMesh.triangles;
        Vector3 meshCenter = targetMesh.bounds.center;

        _triangleAreas = new float[triangles.Length / 3];
        _totalArea = 0f;

        for (int i = 0; i < triangles.Length / 3; i++)
        {
            Vector3 a = vertices[triangles[i * 3]];
            Vector3 b = vertices[triangles[i * 3 + 1]];
            Vector3 c = vertices[triangles[i * 3 + 2]];

            if (!IsTriangleFacingOut(a, b, c, meshCenter))
            {
                _triangleAreas[i] = 0f;
                continue;
            }

            float area = Vector3.Cross(b - a, c - a).magnitude / 2f;
            _triangleAreas[i] = area;
            _totalArea += area;
        }
    }

    private (Vector3 point, Vector3 normal) GetRandomPointOnMesh(Mesh mesh)
    {
        float randomValue = UnityEngine.Random.Range(0f, _totalArea);
        int selectedTriangle = -1;
        float accumulatedArea = 0f;

        for (int i = 0; i < _triangleAreas.Length; i++)
        {
            accumulatedArea += _triangleAreas[i];

            if (randomValue <= accumulatedArea)
            {
                selectedTriangle = i;
                break;
            }
        }

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        
        Vector3 a = vertices[triangles[selectedTriangle * 3]];
        Vector3 b = vertices[triangles[selectedTriangle * 3 + 1]];
        Vector3 c = vertices[triangles[selectedTriangle * 3 + 2]];

        float r1 = UnityEngine.Random.Range(0f, 1f);
        float r2 = UnityEngine.Random.Range(0f, 1f);
        
        if (r1 + r2 > 1f)
        {
            r1 = 1f - r1;
            r2 = 1f - r2;
        }

        Vector3 localPoint = a + r1 * (b - a) + r2 * (c - a);
        Vector3 localNormal = Vector3.Normalize(Vector3.Cross(b - a, c - a));

        localPoint += localNormal * _offsetAboveSurface;
        return (localPoint, localNormal);
    }

    private bool IsTriangleFacingOut(Vector3 a, Vector3 b, Vector3 c, Vector3 meshCenter)
    {
        Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
        Vector3 toCenter = (meshCenter - (a + b + c) / 3f).normalized;

        return Vector3.Dot(normal, toCenter) < 0f;
    }

    private void SetUpNextCorruption()
    {
        if (_index >= _generatedCorruption.Count())
        {
            OnObjectCleaned?.Invoke(this);
            _currentActive = null;
            return;
        }

        _currentActive.SetPos(_generatedCorruption[_index]);
    }

    public void RemoveCorruption()
    {
        _index++;
        _cleanedInstances++;
        OnUpdatedInstances?.Invoke();
        SetUpNextCorruption();
    }

    private void OnDrawGizmos()
    {
        if (_generatedCorruption == null || _generatedCorruption.Count == 0)
            return;

        Gizmos.color = Color.yellow;

        foreach (var (index, position, rotation) in _generatedCorruption)
        {
            Gizmos.DrawSphere(position, 0.02f);

            Vector3 normalDir = rotation * Vector3.up;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(position, position + normalDir * 0.1f);

#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(position + Vector3.up * 0.05f, index.ToString());
#endif

            Gizmos.color = Color.yellow;
        }
    }
}
