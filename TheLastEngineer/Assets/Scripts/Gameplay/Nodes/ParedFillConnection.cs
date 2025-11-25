using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ParedFillConnection : MonoBehaviour
{
    [SerializeField] private float duration = 1.2f;

    private Connection _connection = default;
    private CancellationTokenSource _cancelSource = default;
    private Renderer _renderer = default;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _connection = GetComponentInParent<Connection>();
        _connection.OnNodeConnected += CheckConnectedNode;

        int isCorrupted = _connection.RequiredType == NodeType.Corrupted ? 1 : 0;

        _renderer.material.SetFloat("_IsCorrupted", isCorrupted);
    }

    private void CheckConnectedNode(NodeType nodeType, bool isConnected)
    {
        if (isConnected) Fill();
        else Empty();
    }

    public void Fill()
    {
        StartFill(1f);
    }

    public void Empty()
    {
        StartFill(-0.1f);
    }

    private void StartFill(float target)
    {
        _cancelSource?.Cancel();
        _cancelSource = new CancellationTokenSource();

        _ = AnimateFill(target, _cancelSource.Token);
    }

    private async Task AnimateFill(float target, CancellationToken token)
    {
        float startValue = _renderer.material.GetFloat("_SliderFill");
        float time = 0f;

        while (time < duration)
        {
            if (token.IsCancellationRequested) return;

            time += Time.deltaTime;
            float t = time / duration;

            float value = Mathf.Lerp(startValue, target, t);
            _renderer.material.SetFloat("_SliderFill", value);

            await Task.Yield();
        }

        _renderer.material.SetFloat("_SliderFill", target);
    }
}
