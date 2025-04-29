using UnityEngine;

public class MaterializerNode : NodeController
{
    private PlayerTDController _player = null;

    private void Awake()
    {
        OnAwake();
    }

    void Start()
    {
        OnStart();
        _player = PlayerTDController.Instance;
    }

    void Update()
    {
        OnUpdate();
    }

    protected override void Attach(PlayerTDController player, Vector3 newPos)
    {
        if (_player != null) _player.OnDash += UseHability;

        base.Attach(player, newPos);
    }

    public override void Attach(Vector3 newPos, Transform newParent = null, Vector3 newScale = default, bool parentIsPlayer = false)
    {
        if (!parentIsPlayer && _player != null) _player.OnDash -= UseHability;

        base.Attach (newPos, newParent, newScale, parentIsPlayer);
    }
}
