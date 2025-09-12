using UnityEngine;
using UnityEngine.VFX;

public class GlitchAttractionController : MonoBehaviour
{
    private PlayerNodeHandler _playerNodeHandler = default;
    private VisualEffect _vfx = default;
    private Glitcheable _currentTarget = null;

    private void Start()
    {
        _playerNodeHandler = GetComponentInParent<PlayerNodeHandler>();
        _vfx = GetComponent<VisualEffect>();
        _vfx.Stop();

        _playerNodeHandler.OnNodeGrabbed += CheckNode;
    }

    private void CheckNode(bool hasNode, NodeType nodeType)
    {
        if (!hasNode || nodeType != NodeType.Corrupted)
        {
            _vfx.Stop();
            _vfx.enabled = false;
            return;
        }

        _vfx.enabled = true;
        _vfx.Stop();
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out Glitcheable glitcheable))
        {
            _currentTarget = glitcheable;
            _vfx.Play();
            _vfx.SetVector3("AttractTarget", glitcheable.transform.position);
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out Glitcheable glitcheable) && glitcheable == _currentTarget)
        {
            _currentTarget = null;
            _vfx.Stop();
            _vfx.SetVector3("AttractTarget", Vector3.zero);
        }
    }
}
