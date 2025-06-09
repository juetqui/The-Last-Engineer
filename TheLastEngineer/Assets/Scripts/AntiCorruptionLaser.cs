using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AntiCorruptionLser : Laser
{
    protected override bool CollitionCheck(RaycastHit hit)
    {
        if (hit.transform.TryGetComponent<ICorruptionCanceler>(out ICorruptionCanceler corruptionCanceler))
        {
            _onCollition+=corruptionCanceler.CorruptionCancel;
            return true;
        }
        return false;

    }
}
