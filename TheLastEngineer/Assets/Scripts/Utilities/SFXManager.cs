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
    
    public void PlaySFX(AudioClip clip, Transform parent, float volume)
    {
        var source = Instantiate(sourcePrefab.gameObject, parent.position, Quaternion.identity).GetComponent<AudioSource>();
        source.pitch = Random.Range(0.9f, 1.1f);
        source.clip = clip;
        source.volume = volume;
        source.Play();
        
        var clipLength = source.clip.length;
        Destroy(source.gameObject, clipLength);
    }

    public void PlayRandomSFX(AudioClip[] clip, Transform parent, float volume)
    {
        var i = Random.Range(0, clip.Length);
        PlaySFX(clip[i], parent, volume);
    }
}
