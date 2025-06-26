using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AntiCorruptionMachine : MonoBehaviour
{
    [SerializeField] GenericConnectionController _connection;
    [SerializeField] private NodeType _requiredNode = NodeType.Corrupted;
    [SerializeField] bool _isActive;
    [SerializeField] Transform originTransform;
    [SerializeField] float reductionByFrame;
    [SerializeField] float currentRadius;
    [SerializeField] float machineStartingRadius;
    private List<Collider> objectsInside = new List<Collider>(); // Objetos que actualmente están dentro
    private List<Collider> previousObjectsInside = new List<Collider>(); // Objetos que estaban dentro en el frame anterior
    private Collider[] hitColliders = new Collider[50]; // Un array para evitar asignaciones cada frame (ajusta el tamaño según tus necesidades)
    Vector2 origin2Dvector;
    Vector3 playerPosition;



    // Start is called before the first frame update
    void Awake()
    {
        _connection.OnNodeConnected += SetActive;
        if (originTransform == null) originTransform = transform;
        origin2Dvector = new Vector2(originTransform.position.x, originTransform.position.z);

    }
    public void SetActive(NodeType nodeType, bool IsActive)
    {
        playerPosition = PlayerTDController.Instance.transform.position;
        print("NODO");
        if (nodeType == _requiredNode && !_isActive)
        {
            _isActive = true;

            StartMachine();

        }
        else
        {
            _isActive = false;
        }
    }

    public void StartMachine()
    {
        StartCoroutine(DetectionCorountine());
        
    }
    IEnumerator DetectionCorountine()
    {
        currentRadius = machineStartingRadius;

        while (currentRadius>=0)
        {
            previousObjectsInside.Clear();
            previousObjectsInside.AddRange(objectsInside);
            objectsInside.Clear();
            //float maxDistance = Mathf.Pow(currentRadius, 2);
            int numColliders = Physics.OverlapSphereNonAlloc(originTransform.position, currentRadius*5, hitColliders);

            for (int i = 0; i < numColliders; i++)
            {
                Collider hitCollider = hitColliders[i];

                if (hitCollider.gameObject.GetComponent<AntiCorruptionTest>())
                {
                    objectsInside.Add(hitCollider);
                    if (!previousObjectsInside.Contains(hitCollider))
                    {
                        hitCollider.gameObject.GetComponent<AntiCorruptionTest>().onRange();
                        // Llama al método del script del objeto que entró

                    }
                }

                // Si el objeto no estaba en la lista anterior, acaba de entrar

            }
            foreach (Collider prevCollider in previousObjectsInside)
            {
                Vector2 vector = new Vector2(prevCollider.ClosestPoint(originTransform.position).x, prevCollider.ClosestPoint(originTransform.position).z);
                if (Vector2.Distance(vector, origin2Dvector) >currentRadius)
                {
                    prevCollider.gameObject.GetComponent<AntiCorruptionTest>().OutofRange();
                    print(prevCollider.gameObject.transform.position.ToString() + " " + Vector2.Distance(vector, origin2Dvector).ToString() + currentRadius.ToString());
                }
            }
            if (!_isActive)
            {
                foreach (Collider prevCollider in previousObjectsInside)
                {
                    prevCollider.gameObject.GetComponent<AntiCorruptionTest>().OutofRange();


                }
                foreach (Collider prevCollider in objectsInside)
                {
                    prevCollider.gameObject.GetComponent<AntiCorruptionTest>().OutofRange();


                }
                previousObjectsInside.Clear();
                objectsInside.Clear();

                break;
            }
            currentRadius-=reductionByFrame;
            yield return null;

        }
        if (currentRadius<=0)
        {
            RemoveNodeFromMachine();
        }

    }
    public void RemoveNodeFromMachine()
    {
        _connection.EjectNode(playerPosition);
    }
    //IEnumerator DetectionCorountine()
    //{
    //    currentRadius = machineStartingRadius;
    //    while (currentRadius>=0)
    //    {
    //        previousObjectsInside.Clear();
    //        previousObjectsInside.AddRange(objectsInside);
    //        objectsInside.Clear();

    //        int numColliders = Physics.OverlapSphereNonAlloc(originTransform.position, currentRadius, hitColliders);

    //        for (int i = 0; i < numColliders; i++)
    //        {
    //            Collider hitCollider = hitColliders[i];

    //            if (hitCollider.gameObject.GetComponent<AntiCorruptionTest>())
    //            {
    //                objectsInside.Add(hitCollider);
    //                if (!previousObjectsInside.Contains(hitCollider))
    //                {
    //                    hitCollider.gameObject.GetComponent<AntiCorruptionTest>().onRange();
    //                    // Llama al método del script del objeto que entró

    //                }
    //            }

    //            // Si el objeto no estaba en la lista anterior, acaba de entrar

    //        }
    //        foreach (Collider prevCollider in previousObjectsInside)
    //        {
    //            if (!objectsInside.Contains(prevCollider))
    //            {
    //                prevCollider.gameObject.GetComponent<AntiCorruptionTest>().OutofRange();
    //            }
    //        }
    //        if (!_isActive)
    //        {
    //            foreach (Collider prevCollider in previousObjectsInside)
    //            {
    //                if (!objectsInside.Contains(prevCollider))
    //                {
    //                    prevCollider.gameObject.GetComponent<AntiCorruptionTest>().OutofRange();
    //                }
    //            }
    //        }
    //        currentRadius-=reductionByFrame;
    //        yield return null;

    //    }

    //}
    void OnDrawGizmos()
    {
        // Establece el color de los Gizmos antes de dibujarlos
        Gizmos.color = Color.blue;

        // Dibuja una esfera en la posición del GameObject
        Gizmos.DrawWireSphere(transform.position, currentRadius);

        // Si quieres una esfera sólida, usa Gizmos.DrawSphere en su lugar:
        // Gizmos.DrawSphere(transform.position, radioEsfera); 
    }
}
