using UnityEngine;

public class CutoutObject : MonoBehaviour
{
    [SerializeField] private Transform _target;

    private Camera _mainCamera = default;

    private void Awake()
    {
        _mainCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        CheckCutout();
    }

    private void CheckCutout()
    {
        Vector3 hitPoint = _target.position;
        Vector3 cutoutPos = _mainCamera.WorldToViewportPoint(hitPoint);
        cutoutPos.y /= (Screen.width / Screen.height);

        Shader.SetGlobalVector("_CutoutPos", cutoutPos);
        Shader.SetGlobalFloat("_CutoutSize", 0.1f);
        Shader.SetGlobalFloat("_FalloffSize", 0.05f);
    }
}
