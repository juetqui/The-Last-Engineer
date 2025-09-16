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

    private List<Corruption> _corruptionList = default;
    private Corruption _currentActive = default;
    private Mesh _mesh = default;
    private ParticleSystem _ps = default;

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
        _ps = GetComponentInChildren<ParticleSystem>();
        SetUpObjectCorruption();
        SetParticlesAmount();
    }

    private void SetUpObjectCorruption()
    {
        _corruptionList = new List<Corruption>();
        CalculateTriangleAreas(_mesh);

        _totalInstances = UnityEngine.Random.Range(_minInstances, _maxInstances);

        for (int i = 0; i < _totalInstances; i++)
        {
            (Vector3 point, Vector3 normal) = GetRandomPointOnMesh(_mesh);
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
            GameObject currentInstance = Instantiate(_prefabToSpawn, point, rotation, transform);
            Corruption corruptionCleaner = currentInstance.GetComponent<Corruption>();
            corruptionCleaner.SetUpGenerator(this);

            _corruptionList.Add(corruptionCleaner);
        }

        foreach (var item in _corruptionList)
        {
            item.TurnOnOff(false);
        }

        SetUpNextCorruption();
    }

    private void CalculateTriangleAreas(Mesh targetMesh)
    {
        Vector3[] vertices = targetMesh.vertices;
        int[] triangles = targetMesh.triangles;
        
        _triangleAreas = new float[triangles.Length / 3];
        _totalArea = 0f;

        for (int i = 0; i < triangles.Length / 3; i++)
        {
            Vector3 a = vertices[triangles[i * 3]];
            Vector3 b = vertices[triangles[i * 3 + 1]];
            Vector3 c = vertices[triangles[i * 3 + 2]];

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
        
        Vector3 point = a + r1 * (b - a) + r2 * (c - a);
        Vector3 normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
        Vector3 worldPoint = transform.TransformPoint(point);
        Vector3 worldNormal = transform.TransformDirection(normal);
        worldPoint += worldNormal * _offsetAboveSurface;

        return (worldPoint, worldNormal);
    }

    private void SetUpNextCorruption()
    {
        if (_currentActive != null)
            _currentActive.TurnOnOff(false);

        if (_index >= _corruptionList.Count())
        {
            OnObjectCleaned?.Invoke(this);
            _currentActive = null;
            return;
        }

        _currentActive = _corruptionList[_index];
        _currentActive.TurnOnOff(true);
    }

    public void RemoveCorruption(Corruption corruptionCleaner)
    {
        if (_currentActive != corruptionCleaner) return;

        _index++;
        _cleanedInstances++;
        OnUpdatedInstances?.Invoke();
        SetUpNextCorruption();
        SetParticlesAmount();
    }

    private void SetParticlesAmount()
    {
        float amount = _maxParticles * (1f - ((float)_index / _corruptionList.Count()));
        var psShape = _ps.emission;
        psShape.rateOverTime = amount;
    }
}
