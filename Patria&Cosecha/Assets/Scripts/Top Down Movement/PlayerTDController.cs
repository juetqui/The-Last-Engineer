using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTDController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed, _rotSpeed;

    private float _horizontalInput, _verticalInput;

    private Vector3 _moveDir;

    [SerializeField] private LayerMask _taskObjectsLayer;
    [SerializeField] private float _taskInteractionDistance = 2f;

    public float TaskInteractionDistance { get { return _taskInteractionDistance; } }

    //private void Start()
    //{

    //}

    private void Update()
    {
        MovePlayer(GetMoveInput());
        InitTask();

        ResetLevel();
    }

    private Vector3 GetMoveInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        _moveDir = new Vector3(_horizontalInput, 0, _verticalInput);

        return _moveDir;
    }

    private void MovePlayer(Vector3 moveDir)
    {
        if (moveDir.magnitude > 0.1f) RotatePlayer(moveDir);

        transform.position += moveDir.normalized * _moveSpeed * Time.deltaTime;
    }

    private void RotatePlayer(Vector3 rotDir)
    {
        Quaternion toRotation = Quaternion.LookRotation(rotDir.normalized, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, _rotSpeed * Time.deltaTime);
    }

    private void InitTask()
    {
        RaycastHit hit;

        Vector3 rayPos = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);

        if (Physics.Raycast(transform.position, transform.forward, out hit, _taskInteractionDistance, _taskObjectsLayer))
        {
            CowsTaskManager taskObject = hit.transform.gameObject.GetComponent<CowsTaskManager>();

            if (taskObject != null && Input.GetKeyDown(KeyCode.E)) taskObject.StartTask();
        }
    }

    private void ResetLevel()
    {
        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
