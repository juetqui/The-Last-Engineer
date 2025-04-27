using System.Collections;
using UnityEngine;

public class RotateSpace : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 90f;
    
    private bool _isRotating = false;
    private float _currentAngle = default, _targetAngle = default;

    private void Start()
    {
        _currentAngle = transform.eulerAngles.x;
        _targetAngle = _currentAngle;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !_isRotating)
        {
            _targetAngle += 90f;
            StartCoroutine(Rotate());
        }
    }

    private IEnumerator Rotate()
    {
        _isRotating = true;

        while (!Mathf.Approximately(_currentAngle, _targetAngle))
        {
            float step = _rotationSpeed * Time.deltaTime;
            
            _currentAngle = Mathf.MoveTowards(_currentAngle, _targetAngle, step);
            transform.eulerAngles = new Vector3(_currentAngle, 0, 0);
            
            yield return null;
        }

        _currentAngle = _targetAngle;
        transform.eulerAngles = new Vector3(_currentAngle, 0, 0);
        _isRotating = false;
    }
}
