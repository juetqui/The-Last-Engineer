using UnityEngine;

public class UIInspectionable : MonoBehaviour
{
    [SerializeField] private InspectionType _type = InspectionType.None;

    public InspectionType Type {  get { return _type; } }
    public CorruptionGenerator CorruptionGenerator { get; private set; }
    public Corruption UICorruption { get; private set; }

    private void Awake()
    {
        UICorruption = GetComponentInChildren<Corruption>();
    }

    public void SetUpGenerator(CorruptionGenerator generator)
    {
        CorruptionGenerator = generator;
        UICorruption.EnableCorruptionEvents(false);

        if (generator != null)
        {
            CorruptionGenerator = generator;
            CorruptionGenerator.RefreshCorruptionVisual(UICorruption);
            UICorruption.SetUpGenerator(CorruptionGenerator);
            UICorruption.EnableCorruptionEvents(true);
        }
    }
}
