using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [SerializeField] private AudioSource sourcePrefab;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlaySFX(AudioClip clip, Transform parent, float volume, bool is3D = true, float maxDistance = 20f)
    {
        PlaySFX(clip, parent.position, volume, is3D, maxDistance);
    }

    // Sobrecarga por posición: para sonidos en puntos sin Transform propio (ej: impactos de partículas)
    public void PlaySFX(AudioClip clip, Vector3 position, float volume, bool is3D = true, float maxDistance = 20f)
    {
        var source = Instantiate(sourcePrefab.gameObject, position, Quaternion.identity).GetComponent<AudioSource>();
        source.pitch = Random.Range(0.9f, 1.1f);
        source.clip = clip;
        source.volume = volume;
        // Se setea por llamada para que el mismo prefab sirva para sonidos 2D (UI/jugador) y 3D (ambiente)
        source.spatialBlend = is3D ? 1f : 0f;
        source.maxDistance = maxDistance;
        source.Play();

        var clipLength = source.clip.length;
        Destroy(source.gameObject, clipLength);
    }

    public void PlayRandomSFX(AudioClip[] clip, Transform parent, float volume, bool is3D = true, float maxDistance = 20f)
    {
        var i = Random.Range(0, clip.Length);
        PlaySFX(clip[i], parent, volume, is3D, maxDistance);
    }

    public void PlayRandomSFX(AudioClip[] clip, Vector3 position, float volume, bool is3D = true, float maxDistance = 20f)
    {
        var i = Random.Range(0, clip.Length);
        PlaySFX(clip[i], position, volume, is3D, maxDistance);
    }
}
