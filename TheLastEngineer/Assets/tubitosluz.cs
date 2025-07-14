using System.Collections;
using UnityEngine;

public class tubitosluz : MonoBehaviour
{
    [SerializeField] private GenericConnectionController _connection;
    [SerializeField] private tubitosluz _nextConnection;

    private Material _material;
    private NodeType _requiredNode = NodeType.Default;
    private bool _connected = false;

    private void Start()
    {
        _material = GetComponent<Renderer>().material;

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

    public IEnumerator TurnOn()
    {
        float timer = 0;

        while (timer < 1f)
        {
            timer += Time.deltaTime * 10f;
            _material.SetFloat("_Step", timer);
            yield return null;
        }

        _material.SetFloat("_Step", 1f);

        if (_nextConnection != null) StartCoroutine(_nextConnection.TurnOn());
    }
    
    public IEnumerator TurnOff()
    {
        float timer = 1f;

        while (timer > 0f)
        {
            timer -= Time.deltaTime * 10f;
            _material.SetFloat("_Step", timer);
            yield return null;
        }

        _material.SetFloat("_Step", 0f);
        
        if (_nextConnection != null) StartCoroutine(_nextConnection.TurnOff());
    }
}
