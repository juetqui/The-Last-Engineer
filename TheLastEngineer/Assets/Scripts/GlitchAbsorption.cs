using UnityEngine;

public class GlitchAbsorption : MonoBehaviour
{
    [SerializeField] private GameObject _vfx;
    [SerializeField] private GameObject _playerGhost;

    void Start()
    {
        SetCorruptedState(false);
        PlayerTDController.Instance.OnAbsorbCorruption += SetCorruptedState;
    }

    private void SetCorruptedState(bool setCorruption)
    {
        if (_vfx != null)
            _vfx.SetActive(setCorruption);

        if (_playerGhost != null)
        {
            _playerGhost.SetActive(setCorruption);
            _playerGhost.transform.position = PlayerTDController.Instance.transform.position;
            _playerGhost.transform.rotation = PlayerTDController.Instance.transform.rotation;

        }

        
    }
}
