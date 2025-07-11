using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class tubitosluz : GenericTM
{
    Material material;
    protected override void ValidateAllConnections()
    {
        _running = CheckRequirements();

        if (_running)
        {
            if(material==null)
            material = GetComponent<MeshRenderer>().material;

            material.SetFloat("_Step", 1);
        }
        else
        {
            if (material == null)
                material = GetComponent<MeshRenderer>().material;

            material.SetFloat("_Step", 0);
        }

        //onRunning?.Invoke(_running);
    }
}
