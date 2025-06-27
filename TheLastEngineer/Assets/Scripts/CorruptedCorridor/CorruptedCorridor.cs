using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptedCorridor : MonoBehaviour
{
    bool _isActive;
    [SerializeField] float _safetyDistance;
    [SerializeField] GameObject transportSphere;
    [SerializeField] float sphereSpeed;
    [SerializeField] List<Transform> nodesTransform=new List<Transform>();
    int closestNodeIndex;
    int acum=0;
    [SerializeField] Vector3 transportOffset;
    // Start is called before the first frame update
    private void Start()
    {
        transportSphere.SetActive(false);
    }
    //private void OnCollisionEnter(Collision other)
    //{
    //    print("dasdasd");
    //    if (other.gameObject.GetComponent<PlayerTDController>())
    //    {
    //        if (!PlayerTDController.Instance.IsCorrupted)
    //        {
    //            transportSphere.SetActive(true);
    //            transportSphere.transform.position = PlayerTDController.Instance.gameObject.transform.position + transportOffset;
    //            _isActive = true;
    //            acum = GetCloserNode();
    //        }
    //    }
    //}
    private void OnTriggerEnter(Collider other)
    {
        print("dasdasd");
        if (other.gameObject.GetComponent<PlayerTDController>())
        {
            if (!PlayerTDController.Instance.IsCorrupted)
            {
                transportSphere.SetActive(true);
                transportSphere.transform.position = PlayerTDController.Instance.gameObject.transform.position + transportOffset;
                PlayerTDController.Instance._cc.enabled = false;
                PlayerTDController.Instance.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
                _isActive = true;
                acum = GetCloserNode();
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerTDController>())
        {
            _isActive = false;
        }
    }
    private void Update()
    {
        if (_isActive && acum>=0)
        {
            SphereMove(nodesTransform[acum]);
        }
        if(_isActive && acum < 0)
        {
            _isActive = false;
            transportSphere.SetActive(false);
            PlayerTDController.Instance._cc.enabled = true;
            PlayerTDController.Instance.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;


        }
    }
    void SphereMove(Transform transform)
    {
        Vector3 velocity = (transform.position - transportSphere.transform.position).normalized;
        if (_safetyDistance < Vector3.Distance(transform.position, transportSphere.transform.position))
        {
            transportSphere.transform.position = transportSphere.transform.position + velocity * sphereSpeed * Time.deltaTime;
            PlayerTDController.Instance.gameObject.transform.position = transportSphere.transform.position;
        }
        else
        {
            acum--;
        }
    }
    int GetCloserNode()
    {
        float distance=9999999f;
        foreach (var item in nodesTransform)
        {
            if (Vector3.Distance(item.position, PlayerTDController.Instance.gameObject.transform.position) <distance)
            {
                distance = Vector3.Distance(item.position, PlayerTDController.Instance.gameObject.transform.position);
                closestNodeIndex =nodesTransform.IndexOf(item);
            }
        }
        return closestNodeIndex;
    }



}
