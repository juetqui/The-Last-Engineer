using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICorruptionCanceler
{
    public void CorruptionCancel();
    public void CorruptionRestore();
    public void CorruptionCheck();
}
