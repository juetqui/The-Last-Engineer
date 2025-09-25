using UnityEngine;

public class DoorInspectionController : MonoBehaviour
{
    [SerializeField] private Inspectionable _inspectionable;

    private DoorsView _view = default;

    void Start()
    {
        _view = GetComponent<DoorsView>();
        _inspectionable.OnCleaned += OpenDoor;
    }

    private void OpenDoor()
    {
        _view.OpenDoor(true);
    }
}
