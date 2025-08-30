using System.Collections;
using UnityEngine;

public class TubeLight : MonoBehaviour
{
    [SerializeField] private GenericConnectionController _connection;
    [SerializeField] private TubeLight _nextConnection;

    private Renderer _renderer;
    private bool _connected = false;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _connection.OnNodeConnected += TurnOnOff;
    }

    private void TurnOnOff(NodeType nodeType, bool connected)
    {
        if (!_connected && connected) StartCoroutine(TurnOn());
        else if (_connected && !connected) StartCoroutine(TurnOff());
    }

    public IEnumerator TurnOn()
    {
        _connected = true;
        yield return LerpLight(0f, 1f);
        _nextConnection?.StartCoroutine(_nextConnection.TurnOn());
    }

    public IEnumerator TurnOff()
    {
        _connected = false;
        yield return LerpLight(1f, 0f);
        _nextConnection?.StartCoroutine(_nextConnection.TurnOff());
    }

    private IEnumerator LerpLight(float from, float to)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 10f;
            _renderer.material.SetFloat("_Step", Mathf.Lerp(from, to, t));
            yield return null;
        }
    }
}
