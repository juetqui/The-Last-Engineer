using UnityEngine;
using UnityEngine.UI;

public class ConnectionUIManager : MonoBehaviour
{
    private Connection _connectionParent = default;
    private Animator _animator = default;
    private Image _image = default;
    private Color _defaultColor;

    private void Awake()
    {
        _connectionParent = GetComponentInParent<Connection>();
        _animator = GetComponent<Animator>();
        _image = GetComponent<Image>();
        _connectionParent.OnAvailableToConnect += SetAnimation;
        _connectionParent.OnInitialized += Setup;
    }

    private void Setup()
    {
        _defaultColor = _connectionParent.RequiredType == NodeType.Default ? Color.cyan : Color.magenta;
        _image.color = _defaultColor;
    }

    private void SetAnimation(bool setAnim)
    {
        _image.color = setAnim ? Color.white : _defaultColor;
        _animator.SetBool("HasCorrectNode", setAnim);
    }
}
