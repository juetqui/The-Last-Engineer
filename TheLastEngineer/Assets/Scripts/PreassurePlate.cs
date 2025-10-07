using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreassurePlate : MonoBehaviour
{
    Material material;
    public Vector3 boxSize = new Vector3(0.5f, 1f, 0.5f);
    public float castDistance = 2f;
    public Transform _boxCastOrigin;
    public LayerMask obstacleLayers;
    [SerializeField] bool charging;
    [SerializeField] bool uncharging;
    [SerializeField] float fillAmount=0;
    // Start is called before the first frame update
    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (charging)
        {
            fillAmount += Time.deltaTime;
        }
        if (uncharging)
        {
            fillAmount -= Time.deltaTime;
        }
        RaycastHit[] hits = Physics.BoxCastAll(_boxCastOrigin.position, boxSize / 2, _boxCastOrigin.up, transform.rotation, castDistance, obstacleLayers);
        for (int i = 0; i < hits.Length; i++)
        {
            if(hits[i].collider.GetComponent<PlayerController>()|| hits[i].collider.GetComponent<Glitcheable>())
            {
                if(!charging)
                StartCharging();
                return;
            }
        }
        if (charging)
        {
            StartUnCharging();
        }
    }
    public void StartCharging()
    {
        material.color = Color.green;
    }
    public void StartUnCharging()
    {
        material.color = Color.red;
    }
    private void OnTriggerEnter(Collider other)
    {
        material.color = Color.green;
    }
    private void OnTriggerExit(Collider other)
    {
        material.color = Color.red;
    }
    void OnDrawGizmos()
    {
        // Guardamos la matriz de transformación
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.color = Color.green;

        // Dibuja la caja inicial
        Gizmos.DrawWireCube(Vector3.zero, boxSize);

        // Dibuja la caja final (hasta donde llega el cast)
        Gizmos.color = new Color(0, 0, 0, 0.5f);
        Gizmos.DrawWireCube(Vector3.forward * castDistance, boxSize);

        // Dibuja una línea entre ambas cajas
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(Vector3.zero, Vector3.forward * castDistance);
    }

}
