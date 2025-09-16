using UnityEngine;

public class UIInspectionable : MonoBehaviour
{
    [SerializeField] private InspectionType _type = InspectionType.None;

    public InspectionType Type {  get { return _type; } }
    public CorruptionGenerator CorruptionGenerator { get; private set; }

    private void Start()
    {
        CorruptionGenerator = GetComponentInChildren<CorruptionGenerator>();
    }
}
