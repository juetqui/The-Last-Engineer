using System.Collections.Generic;
using UnityEngine;

public class AntiCorruptionLser : Laser
{
    public List<ICorruptionCanceler> _stoppedObjects = new List<ICorruptionCanceler>();
    public List<ICorruptionCanceler> _hittedObjects = new List<ICorruptionCanceler>();

    protected override bool CollitionCheck(RaycastHit hit)
    {
        if (hit.transform.TryGetComponent(out ICorruptionCanceler corruptionCanceler))
        {
            if (!_hittedObjects.Contains(corruptionCanceler))
            {
                _hittedObjects.Add(corruptionCanceler);
                _onCollition += corruptionCanceler.CorruptionCancel;
            }
            
            return true;
        }
        
        return false;
    }
    protected override void CorruptionCheck()
    {
        //if (_hittedObjects.Count != 0)
        //{
        //    if (_stoppedObjects != null)
        //    {
        //        int acu = _stoppedObjects.Count;
        //        for (int i = 0; i < acu; i++)
        //        {

        //            if (!_hittedObjects.Contains(_stoppedObjects[i]))
        //            {
        //                print("ewewewe");
        //                acu--;
        //                _stoppedObjects[i].CorruptionRestore();
        //                _stoppedObjects.Remove(_stoppedObjects[i]);
        //                _onCollition -= _stoppedObjects[i].CorruptionCancel;
        //            }
        //        }
        //    }

        //}
        //foreach (var item in _hittedObjects)
        //{
        //    if (!_stoppedObjects.Contains(item))
        //    {
        //        _stoppedObjects.Add(item);
        //    }
        //}
        //print(_stoppedObjects.Count);
        //_hittedObjects.Clear();
        //print(_hittedObjects.Count);


    }
}
