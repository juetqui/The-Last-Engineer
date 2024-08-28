using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    public Transform spawnPoint; // El punto donde se instanciará el objeto
    public GameObject objectToInstantiate; // El objeto a instanciar
    public float interactionDistance = 3f; // Distancia a la cual se puede interactuar con el objeto

    private Transform playerTransform;

    void Start()
    {
        // Encuentra el transform del jugador al iniciar el script
        playerTransform = Camera.main.transform;
    }

    void Update()
    {
        // Comprueba la distancia entre el jugador y el objeto
        float distance = Vector3.Distance(playerTransform.position, transform.position);

        // Verifica si el jugador está cerca del objeto y ha presionado la tecla "E"
        if (distance <= interactionDistance && Input.GetKeyDown(KeyCode.E))
        {
            // Instancia el objeto en el punto de destino
            Instantiate(objectToInstantiate, spawnPoint.position, spawnPoint.rotation);
        }
    }
}

