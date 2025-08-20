using System.Collections;
using UnityEngine;

public class TubeLight : MonoBehaviour
{
    [SerializeField] private GenericConnectionController _connection;
    [SerializeField] private TubeLight _nextConnection;

    private Renderer _renderer;
    private NodeType _requiredNode = NodeType.Default;
    private bool _connected = false;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();

        if (_connection != null) _connection.OnNodeConnected += TurnOnOff;
    }

    private void TurnOnOff(NodeType nodeType, bool connected)
    {
        if (!_connected && connected && nodeType == _requiredNode)
        {
            _connected = true;
            StartCoroutine(TurnOn());
        }
        else if (_connected && !connected && nodeType == _requiredNode)
        {
            _connected = false;
            StartCoroutine(TurnOff());
        }
    }
    
    public void ReceptorTurnOn()
    {
        StartCoroutine(TurnOn());
    }

    public void ReceptorTurnOff()
    {
        StartCoroutine(TurnOff());
    }

    public IEnumerator TurnOn()
    {
        _connected = true;
        float timer = 0;

        while (timer < 1f)
        {
            timer += Time.deltaTime * 10f;
            _renderer.material.SetFloat("_Step", timer);
            yield return null;
        }

        _renderer.material.SetFloat("_Step", 1f);

        if (_nextConnection != null) StartCoroutine(_nextConnection.TurnOn());
    }
    
    public IEnumerator TurnOff()
    {
        _connected = false;
        float timer = 1f;

        while (timer > 0f)
        {
            timer -= Time.deltaTime * 10f;
            _renderer.material.SetFloat("_Step", timer);
            yield return null;
        }

        _renderer.material.SetFloat("_Step", 0f);
        
        if (_nextConnection != null) StartCoroutine(_nextConnection.TurnOff());
    }
}
