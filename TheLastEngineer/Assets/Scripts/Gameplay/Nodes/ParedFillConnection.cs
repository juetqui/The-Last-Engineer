using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ParedFillConnection : MonoBehaviour
{
    private Renderer _renderer = default;

    public void IsCorrupted(int boleano)
    {
        _renderer = GetComponent<Renderer>();
        _renderer.material.SetFloat("_IsCorrupted", boleano);
    }

    [SerializeField] private float duration = 1.2f;

    private CancellationTokenSource cancelSource;

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
        // Cancelar animación anterior
        cancelSource?.Cancel();
        cancelSource = new CancellationTokenSource();

        _ = AnimateFill(target, cancelSource.Token);
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
