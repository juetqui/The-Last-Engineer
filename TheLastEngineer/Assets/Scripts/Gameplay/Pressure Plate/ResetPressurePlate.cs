using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class ResetPressurePlate : MonoBehaviour
{
    bool _taskFinished;
    [SerializeField] UnityEvent OnPress;
    [SerializeField] UnityEvent OnRealase;

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.GetComponent<PlayerTDController>() && !_taskFinished)
        {
            OnPress.Invoke();
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.GetComponent<PlayerTDController>() && !_taskFinished)
        {
            OnRealase.Invoke();
        }
    }
    public void TaskEnd()
    {
        _taskFinished = true;
    }
    public void ResetTask()
    {
        _taskFinished = false;
    }

}
