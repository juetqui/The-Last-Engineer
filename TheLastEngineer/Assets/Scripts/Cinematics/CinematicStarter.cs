using UnityEngine;

public class CinematicStarter : MonoBehaviour
{
    [SerializeField] private bool _startOnTriggerEnter = false;
    [SerializeField] private bool _startOnStart = false;
    [SerializeField] private float _delayBeforeStart = 0f;

    private void Start()
    {
        if (_startOnStart) Invoke(nameof(StartCinematic), _delayBeforeStart);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_startOnTriggerEnter && other.TryGetComponent(out PlayerController player))
            StartCinematic();
    }

    public void StartCinematic()
    {
        Debug.Log("Starting Cinematic");
        
        if (CinematicManager.Instance != null)
            CinematicManager.Instance.StartCinematic();
        else
            Debug.LogError("CinematicStarter: CinematicManager not found in scene!");
    }
}

